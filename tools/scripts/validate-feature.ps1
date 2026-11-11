[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$FeatureDirectory = "specs/001-garagem-pessoal",
    [Parameter(Mandatory = $true)]
    [ValidateSet("quality", "unit", "integration", "web-e2e", "android", "load", "restore")]
    [string]$Profile,
    [ValidateSet("podman")]
    [string]$Engine = "podman",
    [string]$SecretFile,
    [switch]$KeepRunning
)

$ErrorActionPreference = "Stop"

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
$featurePath = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot $FeatureDirectory))
$specificationsRoot = [System.IO.Path]::GetFullPath((Join-Path $repositoryRoot "specs"))
$featurePrefix = $specificationsRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar

function Write-RunSummary {
    param([string]$State, [int]$ExitCode, [string]$Message)

    [ordered]@{
        profile = $Profile
        state = $State
        exitCode = $ExitCode
        summary = $Message
    } | ConvertTo-Json -Compress | Write-Output
}

function Get-ImageReference {
    param([string]$Component)

    $entry = $imageLock.images | Where-Object component -eq $Component | Select-Object -First 1
    if ($null -eq $entry) {
        throw "Image component is missing from deploy/images.lock.json: $Component"
    }

    $digestProperty = $entry.digestsByPlatform.PSObject.Properties[$containerPlatform]
    if ($null -eq $digestProperty) {
        throw "Image has no locked digest for ${containerPlatform}: $Component"
    }

    $image = $entry.image
    if ($null -ne $entry.imageByPlatform) {
        $platformImageProperty = $entry.imageByPlatform.PSObject.Properties[$containerPlatform]
        if ($null -ne $platformImageProperty) {
            $image = $platformImageProperty.Value
        }
    }

    return "$image@$($digestProperty.Value)"
}

function Invoke-ContainerCommand {
    param(
        [string]$Component,
        [string]$WorkingDirectory,
        [string[]]$CommandArguments,
        [string]$Network,
        [switch]$UseSecretFile,
        [switch]$NuGetCache,
        [switch]$SharedGateway
    )

    $runArguments = @(
        "run", "--rm", "--pull=missing", "--platform", $containerPlatform,
        "--user", "0:0", "-v", "${repositoryRoot}:/workspace:Z"
    )
    if ($Network) {
        $runArguments += @("--network", $Network)
    }
    if ($NuGetCache) {
        $runArguments += @("-v", "garagemotor-nuget-cache:/root/.nuget/packages")
    }
    if ($UseSecretFile) {
        $runArguments += @("--env-file", $secretPath)
    }
    if ($SharedGateway) {
        $caPath = [System.IO.Path]::GetFullPath((Join-Path $HOME ".microsoft/usersecrets/Altivy.Identity/certs/rootCA.pem"))
        if (-not (Test-Path -LiteralPath $caPath -PathType Leaf)) {
            throw "CA pública da plataforma compartilhada indisponível; nenhum bypass TLS permitido."
        }
        $gatewayAddress = & $Engine inspect --format '{{(index .NetworkSettings.Networks "identity-global").IPAddress}}' altivy-identity-gateway 2>$null
        $parsedAddress = $null
        if ($LASTEXITCODE -ne 0 -or -not [System.Net.IPAddress]::TryParse("$gatewayAddress".Trim(), [ref]$parsedAddress) -or $parsedAddress.AddressFamily -ne [System.Net.Sockets.AddressFamily]::InterNetwork) {
            throw "IPv4 público da rede do gateway indisponível."
        }
        $runArguments += @("-v", "${caPath}:/trust/rootCA.pem:ro,z", "--env", "GARAGEMOTOR_GATEWAY_IP=$parsedAddress", "--env", "PWTEST_CACHE_DIR=/tmp/garagemotor-shared-$([guid]::NewGuid().ToString('N'))")
        foreach ($proxy in @("HTTP_PROXY", "HTTPS_PROXY", "ALL_PROXY", "http_proxy", "https_proxy", "all_proxy")) {
            $runArguments += @("--env", "$proxy=")
        }
        $runArguments += @("--env", "NO_PROXY=localhost,127.0.0.1", "--env", "no_proxy=localhost,127.0.0.1")
    }

    $runArguments += @("-w", $WorkingDirectory, (Get-ImageReference -Component $Component))
    $runArguments += $CommandArguments
    & $Engine @runArguments *> $null
    if ($LASTEXITCODE -ne 0) {
        throw "Container profile step failed; output was suppressed to avoid exposing configuration data."
    }
}

function Invoke-LocalScript {
    param([string]$Path, [string[]]$Arguments = @())

    & pwsh -NoProfile -File (Join-Path $repositoryRoot $Path) @Arguments *> $null
    if ($LASTEXITCODE -ne 0) {
        throw "Local orchestration step failed; output was suppressed to avoid exposing configuration data."
    }
}

function Ensure-NuGetCache {
    & $Engine volume inspect garagemotor-nuget-cache *> $null
    if ($LASTEXITCODE -eq 0) {
        return
    }

    & $Engine volume create garagemotor-nuget-cache *> $null
    if ($LASTEXITCODE -ne 0) {
        throw "Could not prepare the isolated NuGet cache volume."
    }
}

function Get-MissingProfileArtifacts {
    param([string]$SelectedProfile)

    $requiredPaths = switch ($SelectedProfile) {
        "quality" { @("frontend/web/src/main.ts", "frontend/web/src/index.html") }
        "unit" { @("backend/GarageMotor.Tests/GarageMotor.Tests.csproj") }
        "integration" { @("deploy/Dockerfile.api", "tools/scripts/start-local.ps1", "backend/GarageMotor.Tests/Fixtures/RealEnvironment.cs") }
        "web-e2e" { @("deploy/Dockerfile.api", "deploy/Dockerfile.web", "tools/scripts/start-local.ps1", "frontend/tests/shared.playwright.config.ts", "frontend/tests/integrated/run-shared-tests.cjs", "frontend/tests/integrated/auth-session.spec.ts") }
        "android" { @("tools/scripts/test-android.ps1", "frontend/web/android") }
        "load" { @("deploy/Dockerfile.api", "tools/scripts/start-local.ps1", "backend/GarageMotor.Tests/Performance/history-workload.json") }
        "restore" { @("tools/scripts/restore.ps1", "backend/GarageMotor.Api/Migrations/001_identity_and_operations.sql") }
    }

    $missing = @($requiredPaths | Where-Object { -not (Test-Path -LiteralPath (Join-Path $repositoryRoot $_)) })
    if ($SelectedProfile -in @("unit", "integration")) {
        $testSources = Get-ChildItem -LiteralPath (Join-Path $repositoryRoot "backend/GarageMotor.Tests") -Filter "*Tests.cs" -File -Recurse -ErrorAction SilentlyContinue
        if (-not $testSources) {
            $missing += "backend/GarageMotor.Tests/**/*Tests.cs"
        }
    }
    if ($SelectedProfile -eq "web-e2e") {
        $webTests = Get-ChildItem -LiteralPath (Join-Path $repositoryRoot "frontend/tests") -Filter "*.spec.ts" -File -Recurse -ErrorAction SilentlyContinue
        if (-not $webTests) {
            $missing += "frontend/tests/**/*.spec.ts"
        }
    }

    return $missing
}

function Get-RequiredImages {
    param([string]$SelectedProfile)

    switch ($SelectedProfile) {
        "quality" { return @("node-lockfile-runner", "dotnet-sdk-lockfile-runner") }
        "unit" { return @("dotnet-sdk-lockfile-runner") }
        "integration" { return @("dotnet-sdk-lockfile-runner", "postgresql", "nginx") }
        "web-e2e" { return @("playwright-test-runner", "postgresql", "nginx") }
        "android" { return @("playwright-test-runner") }
        "load" { return @("dotnet-sdk-lockfile-runner", "postgresql", "nginx") }
        "restore" { return @("dotnet-sdk-lockfile-runner", "postgresql") }
    }
}

function Assert-PackageLocks {
    $npmLock = Get-Content -LiteralPath (Join-Path $repositoryRoot "frontend/web/package-lock.json") -Raw | ConvertFrom-Json -AsHashtable
    $permissiveLicenses = @("MIT", "Apache-2.0", "BSD-2-Clause", "BSD-3-Clause", "ISC", "0BSD", "BlueOak-1.0.0", "Unlicense")
    $approvedNpm = @{
        "node_modules/lightningcss" = @("1.33.0", "sha512-WkUDrojuJs0xkgGf2udWxa3yGBRxPtxUkB79i6aCZLRgc7PM8fZe9TosfPDcvEpQZbuFASnHYmRLBLUbmLOIIA==")
        "node_modules/lightningcss-linux-x64-gnu" = @("1.33.0", "sha512-ar+Ju7LmcN0Jo4FpL4hpFybwNG9/3A/Br5KW2n2jyODg3MEZXaDYADdemoNS+BDNfMgKvylJLj4S5tyRActuAg==")
        "node_modules/caniuse-lite" = @("1.0.30001823", "sha512-TQ5s/djDjNk1axreUW+4rULH+BmdCMLiJkhaXnO5wqivG7tITkSUj4ItIdzqdZHKaXcHAV8dhH3JxOM7/7bwqw==")
    }

    foreach ($package in $npmLock.packages.GetEnumerator()) {
        if (-not $package.Key.StartsWith("node_modules/")) {
            continue
        }
        if ([string]::IsNullOrWhiteSpace($package.Value.license) -or [string]::IsNullOrWhiteSpace($package.Value.integrity)) {
            throw "npm package metadata is incomplete: $($package.Key)"
        }
        $licenseIds = @($package.Value.license -split "\s+(?:AND|OR)\s+" | ForEach-Object { $_.Trim(" ()") })
        $notPermissive = @($licenseIds | Where-Object { $_ -notin $permissiveLicenses })
        if ($notPermissive.Count -gt 0) {
            $approved = $approvedNpm[$package.Key]
            if ($null -ne $approved -and $package.Value.version -eq $approved[0] -and $package.Value.integrity -eq $approved[1]) {
                continue
            }
            $installedPath = Join-Path $repositoryRoot "frontend/web/$($package.Key)"
            if ($package.Value.optional -and -not (Test-Path -LiteralPath $installedPath)) {
                continue
            }
            throw "npm package license is not covered by the allowlist or exact constitutional exception: $($package.Key)"
        }
    }

    foreach ($lockPath in @("backend/GarageMotor.Api/packages.lock.json", "backend/GarageMotor.Tests/packages.lock.json")) {
        $lock = Get-Content -LiteralPath (Join-Path $repositoryRoot $lockPath) -Raw | ConvertFrom-Json
        foreach ($package in $lock.dependencies."net10.0".PSObject.Properties) {
            if ($package.Value.type -ne "Project" -and [string]::IsNullOrWhiteSpace($package.Value.contentHash)) {
                throw "NuGet package contentHash is missing: $($package.Name)"
            }
            if ($package.Name -eq "Microsoft.Testing.Extensions.CodeCoverage" -and ($package.Value.resolved -ne "18.12.0" -or $package.Value.contentHash -ne "BST/sWYpKk9OXO7Yxktcg6FWORRsHU0EdCbMwZ7d3OqJsMKlrNHIZ30g0Ib4k5T5acJYXRBmg9mNrTU/N6cy1g==")) {
                throw "CodeCoverage package does not match the exact constitutional exception."
            }
        }
    }
}

if (-not $featurePath.StartsWith($featurePrefix, [System.StringComparison]::Ordinal)) {
    Write-RunSummary -State "blocked" -ExitCode 2 -Message "FeatureDirectory must resolve beneath the repository specs directory."
    exit 2
}
if (-not (Test-Path -LiteralPath (Join-Path $featurePath "tasks.md")) -or -not (Test-Path -LiteralPath (Join-Path $featurePath "spec.md"))) {
    Write-RunSummary -State "blocked" -ExitCode 2 -Message "Feature spec/tasks are missing."
    exit 2
}
if (-not (Get-Command $Engine -ErrorAction SilentlyContinue)) {
    Write-RunSummary -State "blocked" -ExitCode 2 -Message "Container CLI is unavailable."
    exit 2
}

try {
    $rootless = & $Engine info --format '{{.Host.Security.Rootless}}' 2>$null
    if ($LASTEXITCODE -ne 0 -or "$rootless".Trim() -ne "true") {
        Write-RunSummary -State "blocked" -ExitCode 2 -Message "Only rootless Podman is allowed; no profile was started."
        exit 2
    }
    $imageLock = Get-Content -LiteralPath (Join-Path $repositoryRoot "deploy/images.lock.json") -Raw | ConvertFrom-Json
    $containerPlatform = switch ([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture) {
        X64 { "linux/amd64" }
        Arm64 { "linux/arm64" }
        default { throw "Unsupported container architecture." }
    }
    try {
        Assert-PackageLocks
    }
    catch {
        Write-RunSummary -State "blocked" -ExitCode 2 -Message "Dependency lock/license validation failed; details are recorded in docs/dependencies.md."
        exit 2
    }

    $requiredImages = Get-RequiredImages -SelectedProfile $Profile
    $pendingImages = @()
    foreach ($component in $requiredImages) {
        $entry = $imageLock.images | Where-Object component -eq $component | Select-Object -First 1
        if ($null -eq $entry -or $null -eq $entry.digestsByPlatform.PSObject.Properties[$containerPlatform]) {
            $pendingImages += $component
        }
    }

    $missingArtifacts = Get-MissingProfileArtifacts -SelectedProfile $Profile
    if ($Profile -eq "android" -and (Select-String -LiteralPath (Join-Path $repositoryRoot "docs/android-toolchain.md") -Pattern "aceite ainda não foi registrado" -Quiet)) {
        $missingArtifacts += "human acceptance of Android SDK terms"
    }
    if ($pendingImages.Count -gt 0 -or $missingArtifacts.Count -gt 0) {
        $reasons = @()
        if ($pendingImages.Count -gt 0) { $reasons += "image digest pending: $($pendingImages -join ', ')" }
        if ($missingArtifacts.Count -gt 0) { $reasons += "profile artifacts/gates pending: $($missingArtifacts -join ', ')" }
        Write-RunSummary -State "blocked" -ExitCode 2 -Message ($reasons -join "; ")
        exit 2
    }

    $platform = $containerPlatform
    $serviceProfile = $Profile -in @("integration", "web-e2e", "load")
    if ($serviceProfile) {
        $approvedSecretPath = if ([OperatingSystem]::IsWindows()) {
            Join-Path $env:APPDATA "Microsoft/UserSecrets/GarageMotor/.env"
        }
        else {
            Join-Path $HOME ".microsoft/usersecrets/GarageMotor/.env"
        }
        if ([string]::IsNullOrWhiteSpace($SecretFile)) {
            $SecretFile = $approvedSecretPath
        }
        $secretPath = [System.IO.Path]::GetFullPath($SecretFile)
        $pathComparison = if ([OperatingSystem]::IsWindows()) { [StringComparison]::OrdinalIgnoreCase } else { [StringComparison]::Ordinal }
        if (-not [string]::Equals($secretPath, [System.IO.Path]::GetFullPath($approvedSecretPath), $pathComparison)) {
            Write-RunSummary -State "blocked" -ExitCode 2 -Message "Private configuration must use the approved external location; no file was read."
            exit 2
        }
        if (-not (Test-Path -LiteralPath $secretPath -PathType Leaf)) {
            Write-RunSummary -State "blocked" -ExitCode 2 -Message "External environment file is missing; its contents were not read."
            exit 2
        }
    }

    $startedLocal = $false
    try {
        if ($serviceProfile) {
            Invoke-LocalScript -Path "tools/scripts/start-local.ps1" -Arguments @("-Engine", $Engine, "-SecretFile", $secretPath)
            $startedLocal = $true
        }

        switch ($Profile) {
            "quality" {
                Invoke-ContainerCommand -Component "node-lockfile-runner" -WorkingDirectory "/workspace/frontend/web" -CommandArguments @("npm", "ci", "--ignore-scripts", "--no-audit", "--fund=false")
                Invoke-ContainerCommand -Component "node-lockfile-runner" -WorkingDirectory "/workspace/frontend/web" -CommandArguments @("./node_modules/.bin/prettier", "--check", ".")
                Ensure-NuGetCache
                Invoke-ContainerCommand -Component "dotnet-sdk-lockfile-runner" -WorkingDirectory "/workspace" -CommandArguments @("dotnet", "restore", "backend/GarageMotor.Tests/GarageMotor.Tests.csproj", "--configfile", "/workspace/backend/NuGet.Config", "--locked-mode") -NuGetCache
                Invoke-ContainerCommand -Component "dotnet-sdk-lockfile-runner" -WorkingDirectory "/workspace" -CommandArguments @("dotnet", "build", "backend/GarageMotor.Tests/GarageMotor.Tests.csproj", "--no-restore") -NuGetCache
                Invoke-ContainerCommand -Component "dotnet-sdk-lockfile-runner" -WorkingDirectory "/workspace" -CommandArguments @("dotnet", "format", "backend/GarageMotor.Tests/GarageMotor.Tests.csproj", "--verify-no-changes", "--no-restore") -NuGetCache
                Invoke-ContainerCommand -Component "node-lockfile-runner" -WorkingDirectory "/workspace/frontend/web" -CommandArguments @("./node_modules/.bin/ng", "build", "--configuration", "production")
            }
            "unit" {
                Invoke-ContainerCommand -Component "dotnet-sdk-lockfile-runner" -WorkingDirectory "/workspace" -CommandArguments @("dotnet", "test", "backend/GarageMotor.Tests/GarageMotor.Tests.csproj", "--filter-trait", "Category=Unit", "--property:RestoreLockedMode=true") -NuGetCache
            }
            "integration" {
                Invoke-ContainerCommand -Component "dotnet-sdk-lockfile-runner" -WorkingDirectory "/workspace" -CommandArguments @("dotnet", "test", "backend/GarageMotor.Tests/GarageMotor.Tests.csproj", "--filter-trait", "Category=Integration", "--property:RestoreLockedMode=true") -Network "garagemotor_application" -UseSecretFile -NuGetCache
            }
            "web-e2e" {
                Invoke-ContainerCommand -Component "node-lockfile-runner" -WorkingDirectory "/workspace/frontend/web" -CommandArguments @("npm", "ci", "--ignore-scripts", "--no-audit", "--fund=false")
                Invoke-ContainerCommand -Component "playwright-test-runner" -WorkingDirectory "/workspace/frontend/tests" -CommandArguments @("node", "integrated/run-shared-tests.cjs") -Network "identity-global" -SharedGateway
            }
            "android" { Invoke-LocalScript -Path "tools/scripts/test-android.ps1" -Arguments @("-Engine", $Engine) }
            "load" { Invoke-ContainerCommand -Component "dotnet-sdk-lockfile-runner" -WorkingDirectory "/workspace" -CommandArguments @("dotnet", "test", "backend/GarageMotor.Tests/GarageMotor.Tests.csproj", "--filter-trait", "Category=Load", "--property:RestoreLockedMode=true") -Network "garagemotor_application" -UseSecretFile -NuGetCache }
            "restore" { Invoke-LocalScript -Path "tools/scripts/restore.ps1" -Arguments @("-Engine", $Engine) }
        }
    }
    finally {
        if ($startedLocal -and -not $KeepRunning) {
            Invoke-LocalScript -Path "tools/scripts/stop-local.ps1" -Arguments @("-Engine", $Engine, "-SecretFile", $secretPath)
        }
    }

    Write-RunSummary -State "passed" -ExitCode 0 -Message "Profile completed; details suppressed and no secrets were emitted."
    exit 0
}
catch {
    Write-RunSummary -State "failed" -ExitCode 1 -Message "Profile failed or is blocked by a required gate; sensitive tool output was suppressed."
    exit 1
}
