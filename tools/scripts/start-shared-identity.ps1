[CmdletBinding()]
param(
    [string]$PlatformDirectory = (Join-Path $PSScriptRoot '../../../architecture-knowledge-base/platforms/identity'),
    [switch]$InitializeNewInstance,
    [switch]$PrepareOnly,
    [switch]$ValidateOnly,
    [switch]$StartPrepared
)

$ErrorActionPreference = 'Stop'
$platformRoot = [System.IO.Path]::GetFullPath($PlatformDirectory)
$approvedPlatformRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../architecture-knowledge-base/platforms/identity'))
if (-not [string]::Equals($platformRoot, $approvedPlatformRoot, [StringComparison]::Ordinal)) { throw 'Diretório de plataforma fora do caminho autorizado.' }
$modes = @($PrepareOnly, $ValidateOnly, $StartPrepared) | Where-Object { [bool]$_ }
if (@($modes).Count -gt 1) { throw 'PrepareOnly, ValidateOnly e StartPrepared são modos mutuamente exclusivos.' }
$privateRoot = [System.IO.Path]::GetFullPath((Join-Path $HOME '.microsoft/usersecrets/Altivy.Identity'))
$privateFile = Join-Path $privateRoot '.env'
$runtimeRoot = Join-Path $privateRoot 'runtime-rootless'
$dataRoot = Join-Path $privateRoot 'postgres-data'
$themesRoot = Join-Path $privateRoot 'themes'
$certificate = Join-Path $privateRoot 'certs/tls.crt'
$privateKey = Join-Path $privateRoot 'certs/tls.key'
$composeFile = Join-Path $runtimeRoot 'compose.rootless.json'
$initSql = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../deploy/identity-init.sql'))

if (-not [OperatingSystem]::IsLinux()) { throw 'Este bootstrap foi definido exclusivamente para Podman rootless em Linux.' }
$rootless = & podman info --format '{{.Host.Security.Rootless}}' 2>$null
if ($LASTEXITCODE -ne 0 -or "$rootless".Trim() -ne 'true') { throw 'Podman rootless é obrigatório.' }
foreach ($file in @($privateFile, $certificate, $privateKey, $initSql, (Join-Path $platformRoot 'gateway/nginx.conf'))) {
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) { throw 'Pré-requisito de arquivo ausente; nenhum secret foi lido.' }
}
foreach ($name in @('altivy', 'dokpod')) {
    if (-not (Test-Path -LiteralPath (Join-Path $themesRoot $name) -PathType Container)) { throw "Tema externo ausente: $name" }
}

function Invoke-PodmanSafe {
    param([string[]]$Arguments)
    & podman @Arguments *> $null
    if ($LASTEXITCODE -ne 0) { throw 'Operação Podman falhou; saída suprimida para proteger configuração privada.' }
}

function Write-PublicConfiguration {
    param([string]$Path, [string]$Content)
    $stream = [System.IO.File]::Open($Path, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Content)
        $stream.Write($bytes, 0, $bytes.Length)
    }
    finally { $stream.Dispose() }
}

function Start-PreparedIdentity {
    if (-not (Test-Path -LiteralPath $dataRoot) -and -not $InitializeNewInstance) {
        throw 'A criação inicial do diretório de dados exige InitializeNewInstance.'
    }
    Invoke-PodmanSafe -Arguments @('compose', '--env-file', $privateFile, '-f', $composeFile, 'up', '--detach', 'postgres')
    Invoke-PodmanSafe -Arguments @('wait', '--condition', 'healthy', '--condition', 'unhealthy', '--condition', 'exited', 'altivy-identity-postgres')
    $health = & podman inspect --format '{{.State.Health.Status}}' altivy-identity-postgres 2>$null
    if ($LASTEXITCODE -ne 0 -or "$health".Trim() -ne 'healthy') { throw 'PostgreSQL não ficou saudável; os demais serviços não foram iniciados.' }
    Invoke-PodmanSafe -Arguments @('compose', '--env-file', $privateFile, '-f', $composeFile, 'up', '--detach', 'keycloak', 'gateway')
    Write-Output 'Stack Identity compartilhada iniciada em Podman rootless; PostgreSQL saudável. Validar Keycloak e gateway TLS antes de conectar produtos.'
}

if (Test-Path -LiteralPath $composeFile -PathType Leaf) {
    if (-not $ValidateOnly -and -not $StartPrepared) { throw 'Configuração rootless já existe; não será sobrescrita ou reinicializada automaticamente.' }
    Invoke-PodmanSafe -Arguments @('compose', '--env-file', $privateFile, '-f', $composeFile, 'config', '--services')
    if ($StartPrepared) { Start-PreparedIdentity; exit 0 }
    Write-Output 'Configuração rootless existente validada, sem iniciar serviços.'
    exit 0
}
if ($ValidateOnly) { throw 'Configuração rootless ainda não preparada.' }
if (-not $InitializeNewInstance) { throw 'O primeiro bootstrap exige autorização explícita de instância nova.' }
if (Test-Path -LiteralPath $dataRoot) { throw 'Destino de dados já existe; bootstrap novo recusado para preservar dados.' }
$existing = @(& podman ps -a --filter 'label=com.docker.compose.project=altivy-identity' --format '{{.Names}}' 2>$null)
if ($LASTEXITCODE -ne 0 -or $existing.Count -gt 0) { throw 'Já existe stack Identity ou sua ausência não pôde ser confirmada.' }
foreach ($port in @(5432, 7443)) {
    $listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, $port)
    try { $listener.Start() } finally { $listener.Stop() }
}

[System.IO.Directory]::CreateDirectory($runtimeRoot) | Out-Null
[System.IO.File]::SetUnixFileMode($runtimeRoot, [System.IO.UnixFileMode]::UserRead -bor [System.IO.UnixFileMode]::UserWrite -bor [System.IO.UnixFileMode]::UserExecute)
if (-not (Test-Path -LiteralPath (Join-Path $themesRoot 'identity') -PathType Container)) {
    Copy-Item -LiteralPath (Join-Path $platformRoot 'themes/identity') -Destination (Join-Path $themesRoot 'identity') -Recurse -ErrorAction Stop
}
Invoke-PodmanSafe -Arguments @('network', 'create', '--ignore', 'identity-global')
$network = @(& podman network inspect identity-global 2>$null) -join "`n" | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) { throw 'Não foi possível verificar a rede compartilhada.' }
$dns = [System.Net.IPAddress]::Parse($network[0].subnets[0].gateway).ToString()
$gatewayConfig = Get-Content -LiteralPath (Join-Path $platformRoot 'gateway/nginx.conf') -Raw
$gatewayConfig = $gatewayConfig.Replace('resolver 127.0.0.11 ', "resolver $dns ")
Write-PublicConfiguration -Path (Join-Path $runtimeRoot 'nginx.conf') -Content $gatewayConfig

$postgresImage = 'docker.io/lzocateli/postgresql:18.4-pgvector0.8.6-bookworm@sha256:10d93543368ebe4d69a473e068cda1a7a8a94e812579d6a29a1b73a851eafab2'
$nginxImage = 'docker.io/lzocateli/nginx:1.28.0-bookworm@sha256:e46f7470fc0f6d758d5b9ba5651fd1bba1236ae33d7a269faf68f535abf98298'
Invoke-PodmanSafe -Arguments @('pull', 'docker.io/lzocateli/keycloak:26.7.0')
$keycloakReference = @(& podman image inspect --format '{{index .RepoDigests 0}}' docker.io/lzocateli/keycloak:26.7.0 2>$null)[0]
if ($LASTEXITCODE -ne 0 -or $keycloakReference -notmatch '@sha256:[a-f0-9]{64}$') { throw 'Digest Keycloak não confirmado.' }

function Mount {
    param([string]$Source, [string]$Target, [switch]$ReadOnly)
    return @{ type = 'bind'; source = $Source; target = $Target; read_only = [bool]$ReadOnly; bind = @{ selinux = 'z' } }
}
$configuration = @{
    name = 'altivy-identity'
    services = @{
        postgres = @{
            image = $postgresImage
            container_name = 'altivy-identity-postgres'
            restart = 'unless-stopped'
            environment = @{
                POSTGRES_USER = '${POSTGRES_ADMIN_USERNAME:?POSTGRES_ADMIN_USERNAME obrigatório}'
                POSTGRES_PASSWORD = '${POSTGRES_ADMIN_PASSWORD:?POSTGRES_ADMIN_PASSWORD obrigatório}'
                POSTGRES_DB = 'keycloak'
            }
            ports = @('127.0.0.1:5432:5432')
            volumes = @((Mount $dataRoot '/var/lib/postgresql'), (Mount $initSql '/docker-entrypoint-initdb.d/10-identity-schema.sql' -ReadOnly))
            networks = @('identity-data', 'identity-client')
            healthcheck = @{ test = @('CMD-SHELL', 'pg_isready -h 127.0.0.1 -U "$$POSTGRES_USER" -d keycloak'); interval = '5s'; timeout = '5s'; retries = 24 }
        }
        keycloak = @{
            image = $keycloakReference
            container_name = 'altivy-identity-keycloak'
            restart = 'unless-stopped'
            command = @('start-dev', '--spi-theme--static-max-age=-1', '--spi-theme--cache-themes=false', '--spi-theme--cache-templates=false')
            environment = @{
                KC_BOOTSTRAP_ADMIN_USERNAME = '${KEYCLOAK_MASTER_USERNAME:?KEYCLOAK_MASTER_USERNAME obrigatório}'
                KC_BOOTSTRAP_ADMIN_PASSWORD = '${KEYCLOAK_MASTER_PASSWORD:?KEYCLOAK_MASTER_PASSWORD obrigatório}'
                KC_DB = 'postgres'; KC_DB_URL = 'jdbc:postgresql://postgres:5432/keycloak'; KC_DB_SCHEMA = 'identity'
                KC_DB_USERNAME = '${POSTGRES_ADMIN_USERNAME:?POSTGRES_ADMIN_USERNAME obrigatório}'
                KC_DB_PASSWORD = '${POSTGRES_ADMIN_PASSWORD:?POSTGRES_ADMIN_PASSWORD obrigatório}'
                KC_HOSTNAME = 'https://localhost:7443'; KC_HOSTNAME_ADMIN = 'https://localhost:7443'
                KC_PROXY_HEADERS = 'xforwarded'; KC_HTTP_ENABLED = 'true'; KC_HEALTH_ENABLED = 'true'; KC_METRICS_ENABLED = 'true'
            }
            volumes = @((Mount (Join-Path $themesRoot 'identity') '/opt/keycloak/themes/identity' -ReadOnly), (Mount (Join-Path $themesRoot 'altivy') '/opt/keycloak/themes/altivy' -ReadOnly), (Mount (Join-Path $themesRoot 'dokpod') '/opt/keycloak/themes/dokpod' -ReadOnly))
            networks = @{ 'identity-data' = @{}; 'identity-global' = @{ aliases = @('keycloak', 'auth-keycloak') } }
            depends_on = @{ postgres = @{ condition = 'service_healthy' } }
        }
        gateway = @{
            image = $nginxImage
            container_name = 'altivy-identity-gateway'
            restart = 'unless-stopped'
            ports = @('127.0.0.1:7443:7443')
            volumes = @((Mount (Join-Path $runtimeRoot 'nginx.conf') '/etc/nginx/nginx.conf' -ReadOnly), (Mount (Join-Path $platformRoot 'gateway/conf.d') '/etc/nginx/conf.d' -ReadOnly), (Mount (Join-Path $platformRoot 'gateway/error-page') '/usr/share/nginx/html/custom-error-page' -ReadOnly), (Mount $certificate '/etc/nginx/certs/tls.crt' -ReadOnly), (Mount $privateKey '/etc/nginx/certs/tls.key' -ReadOnly))
            networks = @{ 'identity-global' = @{ aliases = @('gateway') } }
            depends_on = @{ keycloak = @{ condition = 'service_started' } }
        }
    }
    networks = @{
        'identity-global' = @{ external = $true; name = 'identity-global' }
        'identity-data' = @{ name = 'identity-data'; internal = $true }
        'identity-client' = @{ name = 'identity-client' }
    }
}
Write-PublicConfiguration -Path $composeFile -Content ($configuration | ConvertTo-Json -Depth 15)
Invoke-PodmanSafe -Arguments @('compose', '--env-file', $privateFile, '-f', $composeFile, 'config', '--services')
if ($PrepareOnly) { Write-Output 'Adaptador rootless preparado e validado; nenhum serviço iniciado e arquivo privado preservado.'; exit 0 }
Start-PreparedIdentity
