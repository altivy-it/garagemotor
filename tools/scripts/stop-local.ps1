[CmdletBinding()]
param(
    [ValidateSet("podman")]
    [string]$Engine = "podman",
    [string]$SecretFile
)

$ErrorActionPreference = "Stop"

if (-not (Get-Command $Engine -ErrorAction SilentlyContinue)) {
    throw "CLI de containers não encontrado: $Engine"
}

$rootless = & $Engine info --format '{{.Host.Security.Rootless}}' 2>$null
if ($LASTEXITCODE -ne 0 -or "$rootless".Trim() -ne "true") {
    throw "A execução exige Podman rootless; nenhum serviço foi parado."
}

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$composeFile = Join-Path $repositoryRoot "deploy/compose.yaml"
$lockFile = Join-Path $repositoryRoot "deploy/images.lock.json"
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

$env:GARAGEMOTOR_PLATFORM = $platform
$env:GARAGEMOTOR_PUBLIC_ORIGIN = "https://not-used.invalid"
$env:GARAGEMOTOR_TRUSTED_PROXY = "127.0.0.1"
$env:GARAGEMOTOR_DATA_PROTECTION_CERTIFICATE_BASE64 = "not-used-for-down"
$env:GARAGEMOTOR_DATA_PROTECTION_CERTIFICATE_PASSWORD = "not-used-for-down"
$env:GARAGEMOTOR_NGINX_IMAGE = Get-LockedImageReference -Component "nginx" -Platform $platform
$env:GARAGEMOTOR_POSTGRES_IMAGE = Get-LockedImageReference -Component "postgresql" -Platform $platform
$env:GARAGEMOTOR_NODE_IMAGE = Get-LockedImageReference -Component "node-lockfile-runner" -Platform $platform
$env:GARAGEMOTOR_DOTNET_SDK_IMAGE = Get-LockedImageReference -Component "dotnet-sdk-lockfile-runner" -Platform $platform
$env:GARAGEMOTOR_ASPNET_IMAGE = Get-LockedImageReference -Component "dotnet-aspnet-runtime" -Platform $platform

$composeArguments = @(
    "compose",
    "-f", $composeFile,
    "down"
)

& $Engine @composeArguments
if ($LASTEXITCODE -ne 0) {
    throw "Falha ao parar os serviços locais; volumes foram preservados."
}
