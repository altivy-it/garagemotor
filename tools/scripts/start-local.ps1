[CmdletBinding()]
param(
    [ValidateSet("podman")]
    [string]$Engine = "podman",
    [string]$SecretFile,
    [ValidateRange(1, 65535)]
    [int]$HttpPort = 8080,
    [ValidatePattern("^[a-zA-Z0-9][a-zA-Z0-9_.-]*$")]
    [string]$GatewayNetwork = "identity-global",
    [ValidatePattern("^[a-zA-Z0-9][a-zA-Z0-9_.-]*$")]
    [string]$DatabaseNetwork = "identity-client"
)

$ErrorActionPreference = "Stop"

if (-not (Get-Command $Engine -ErrorAction SilentlyContinue)) {
    throw "CLI de containers não encontrado: $Engine"
}

$rootless = & $Engine info --format '{{.Host.Security.Rootless}}' 2>$null
if ($LASTEXITCODE -ne 0 -or "$rootless".Trim() -ne "true") {
    throw "A execução exige Podman rootless; nenhum serviço foi iniciado."
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$composeFile = Join-Path $repositoryRoot "deploy/compose.yaml"
$lockFile = Join-Path $repositoryRoot "deploy/images.lock.json"

foreach ($network in @($GatewayNetwork, $DatabaseNetwork) | Select-Object -Unique) {
    & $Engine network inspect $network *> $null
    if ($LASTEXITCODE -ne 0) {
        throw "Rede compartilhada indisponível neste contexto: $network. Nenhum serviço de infraestrutura será criado."
    }
}

$approvedSecretFile = if ([OperatingSystem]::IsWindows()) {
    Join-Path $env:APPDATA "Microsoft/UserSecrets/GarageMotor/.env"
}
else {
    Join-Path $HOME ".microsoft/usersecrets/GarageMotor/.env"
}
if ([string]::IsNullOrWhiteSpace($SecretFile)) {
    $SecretFile = $approvedSecretFile
}

$secretPath = [System.IO.Path]::GetFullPath($SecretFile)
$pathComparison = if ([OperatingSystem]::IsWindows()) { [StringComparison]::OrdinalIgnoreCase } else { [StringComparison]::Ordinal }
if (-not [string]::Equals($secretPath, [System.IO.Path]::GetFullPath($approvedSecretFile), $pathComparison)) {
    throw "A configuração privada deve usar exclusivamente o local externo aprovado; nenhum arquivo foi lido."
}
if (-not (Test-Path -LiteralPath $secretPath -PathType Leaf)) {
    throw "Arquivo externo de configuração não encontrado; o conteúdo não foi lido."
}

$lock = Get-Content -LiteralPath $lockFile -Raw | ConvertFrom-Json
$platform = switch ([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture) {
    X64 { "linux/amd64" }
    Arm64 { "linux/arm64" }
    default { throw "Arquitetura não suportada para containers Linux." }
}

function Get-LockedImageReference {
    param([string]$Component, [string]$Platform)

    $entry = $lock.images | Where-Object component -eq $Component | Select-Object -First 1
    $digestProperty = $entry.digestsByPlatform.PSObject.Properties[$Platform]
    if ($null -eq $entry -or $null -eq $digestProperty) {
        throw "Imagem sem digest fixado para a plataforma requerida: $Component"
    }

    $image = $entry.image
    if ($null -ne $entry.imageByPlatform) {
        $platformImageProperty = $entry.imageByPlatform.PSObject.Properties[$Platform]
        if ($null -ne $platformImageProperty) {
            $image = $platformImageProperty.Value
        }
    }

    return "$image@$($digestProperty.Value)"
}

foreach ($dockerfile in @("deploy/Dockerfile.api", "deploy/Dockerfile.web")) {
    if (-not (Test-Path -LiteralPath (Join-Path $repositoryRoot $dockerfile) -PathType Leaf)) {
        throw "Dockerfile ainda não preparado: $dockerfile"
    }
}

$env:GARAGEMOTOR_PLATFORM = $platform
$env:GARAGEMOTOR_GATEWAY_NETWORK = $GatewayNetwork
$env:GARAGEMOTOR_DATABASE_NETWORK = $DatabaseNetwork
$gatewayAddress = & $Engine inspect --format "{{(index .NetworkSettings.Networks `"$GatewayNetwork`").IPAddress}}" altivy-identity-gateway 2>$null
$parsedAddress = $null
if ($LASTEXITCODE -ne 0 -or -not [System.Net.IPAddress]::TryParse("$gatewayAddress".Trim(), [ref]$parsedAddress)) {
    throw "Gateway compartilhado não pôde ser identificado; nenhum serviço iniciado."
}
$env:GARAGEMOTOR_TRUSTED_PROXY = $parsedAddress.ToString()
$env:GARAGEMOTOR_NGINX_IMAGE = Get-LockedImageReference -Component "nginx" -Platform $platform
$env:GARAGEMOTOR_POSTGRES_IMAGE = Get-LockedImageReference -Component "postgresql" -Platform $platform
$env:GARAGEMOTOR_NODE_IMAGE = Get-LockedImageReference -Component "node-lockfile-runner" -Platform $platform
$env:GARAGEMOTOR_DOTNET_SDK_IMAGE = Get-LockedImageReference -Component "dotnet-sdk-lockfile-runner" -Platform $platform
$env:GARAGEMOTOR_ASPNET_IMAGE = Get-LockedImageReference -Component "dotnet-aspnet-runtime" -Platform $platform

$composeArguments = @(
    "compose",
    "--env-file", $secretPath,
    "-f", $composeFile,
    "up", "--detach", "--build", "--force-recreate"
)

& $Engine @composeArguments *> $null
if ($LASTEXITCODE -ne 0) {
    throw "Falha ao iniciar os serviços locais pelo Compose."
}

Write-Output "API e web iniciadas nas redes externas. O acesso HTTP depende da rota configurada no nginx compartilhado; nenhuma porta foi publicada pelo GarageMotor."
