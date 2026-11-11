[CmdletBinding()]
param([switch]$InitializeNewDatabase)

$ErrorActionPreference = 'Stop'
if (-not $InitializeNewDatabase) { throw 'A criação inicial exige InitializeNewDatabase explícito.' }
if (-not [OperatingSystem]::IsLinux()) { throw 'Este provisionamento foi validado somente em Linux.' }
$rootless = & podman info --format '{{.Host.Security.Rootless}}' 2>$null
if ($LASTEXITCODE -ne 0 -or "$rootless".Trim() -ne 'true') { throw 'Podman rootless é obrigatório.' }
$root = [System.IO.Path]::GetFullPath((Join-Path $HOME '.microsoft/usersecrets/GarageMotor'))
$file = Join-Path $root '.env'
if (Test-Path -LiteralPath $file) { throw 'Configuração GarageMotor já existe; não será lida nem sobrescrita.' }
$health = & podman inspect --format '{{.State.Health.Status}}' altivy-identity-postgres 2>$null
if ($LASTEXITCODE -ne 0 -or "$health".Trim() -ne 'healthy') { throw 'PostgreSQL compartilhado não está saudável.' }
$migration = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../backend/GarageMotor.Api/Migrations/001_identity_and_operations.sql'))
if (-not (Test-Path -LiteralPath $migration -PathType Leaf)) { throw 'Migration do produto não encontrada.' }

function Invoke-PrivatePsql {
    param([string]$Database, [string]$Sql)
    if ($Database -notin @('postgres', 'garagemotor')) { throw 'Banco fora do escopo de provisionamento.' }
    $start = [System.Diagnostics.ProcessStartInfo]::new('podman')
    $start.UseShellExecute = $false
    $start.RedirectStandardInput = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    $command = 'export PGPASSWORD="$POSTGRES_PASSWORD"; export PGOPTIONS="-c log_statement=none -c log_min_error_statement=panic"; exec psql -X --username "$POSTGRES_USER" --dbname "$1" --set ON_ERROR_STOP=1 --no-align --tuples-only'
    foreach ($argument in @('exec', '--interactive', 'altivy-identity-postgres', '/bin/sh', '-c', $command, '--', $Database)) {
        $start.ArgumentList.Add($argument)
    }
    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $start
    try {
        if (-not $process.Start()) { throw 'Não foi possível iniciar o cliente de provisionamento.' }
        $output = $process.StandardOutput.ReadToEndAsync()
        $errors = $process.StandardError.ReadToEndAsync()
        $process.StandardInput.WriteLine($Sql)
        $process.StandardInput.Close()
        if (-not $process.WaitForExit(120000)) { $process.Kill($true); throw 'Provisionamento excedeu o tempo limite; preserve a configuração para revisão.' }
        [void]$errors.GetAwaiter().GetResult()
        if ($process.ExitCode -ne 0) { throw 'Operação PostgreSQL falhou; detalhes suprimidos. Não remova dados ou configuração para repetir automaticamente.' }
        return $output.GetAwaiter().GetResult().Trim()
    }
    finally { $process.Dispose() }
}

$exists = Invoke-PrivatePsql -Database postgres -Sql "SELECT EXISTS(SELECT 1 FROM pg_database WHERE datname='garagemotor') OR EXISTS(SELECT 1 FROM pg_roles WHERE rolname IN ('garagemotor_runtime','garagemotor_migrations'));"
if ($exists -ne 'f') { throw 'Banco ou papéis GarageMotor já existem; provisionamento recusado para preservar dados/credenciais.' }
[System.IO.Directory]::CreateDirectory($root) | Out-Null
[System.IO.File]::SetUnixFileMode($root, [System.IO.UnixFileMode]::UserRead -bor [System.IO.UnixFileMode]::UserWrite -bor [System.IO.UnixFileMode]::UserExecute)
$runtimePassword = [Convert]::ToHexString([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
$migrationPassword = [Convert]::ToHexString([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
$certificatePassword = [Convert]::ToHexString([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
$rsa = [System.Security.Cryptography.RSA]::Create(3072)
try {
    $request = [System.Security.Cryptography.X509Certificates.CertificateRequest]::new('CN=GarageMotor local Data Protection', $rsa, [System.Security.Cryptography.HashAlgorithmName]::SHA256, [System.Security.Cryptography.RSASignaturePadding]::Pkcs1)
    $certificate = $request.CreateSelfSigned([DateTimeOffset]::UtcNow.AddMinutes(-5), [DateTimeOffset]::UtcNow.AddYears(1))
    try { $certificateValue = [Convert]::ToBase64String($certificate.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Pkcs12, $certificatePassword)) }
    finally { $certificate.Dispose() }
}
finally { $rsa.Dispose() }
$lines = @(
    "GARAGEMOTOR_CONNECTION_STRING=Host=postgres;Port=5432;Database=garagemotor;Username=garagemotor_runtime;Password=$runtimePassword;Search Path=garagemotor;Maximum Pool Size=40",
    "GARAGEMOTOR_MIGRATIONS_CONNECTION_STRING=Host=postgres;Port=5432;Database=garagemotor;Username=garagemotor_migrations;Password=$migrationPassword;Search Path=garagemotor;Maximum Pool Size=10",
    'GARAGEMOTOR_PUBLIC_ORIGIN=https://localhost:7443',
    "GARAGEMOTOR_DATA_PROTECTION_CERTIFICATE_BASE64=$certificateValue",
    "GARAGEMOTOR_DATA_PROTECTION_CERTIFICATE_PASSWORD=$certificatePassword",
    "ConnectionStrings__GarageMotor=Host=postgres;Port=5432;Database=garagemotor;Username=garagemotor_runtime;Password=$runtimePassword;Search Path=garagemotor",
    "ConnectionStrings__GarageMotorMigrations=Host=postgres;Port=5432;Database=garagemotor;Username=garagemotor_migrations;Password=$migrationPassword;Search Path=garagemotor"
)
$stream = [System.IO.File]::Open($file, [System.IO.FileMode]::CreateNew, [System.IO.FileAccess]::Write, [System.IO.FileShare]::None)
try {
    [System.IO.File]::SetUnixFileMode($file, [System.IO.UnixFileMode]::UserRead -bor [System.IO.UnixFileMode]::UserWrite)
    $bytes = [System.Text.Encoding]::UTF8.GetBytes(($lines -join "`n") + "`n")
    $stream.Write($bytes, 0, $bytes.Length)
}
finally { $stream.Dispose() }
$sql = @"
BEGIN;
CREATE ROLE garagemotor_runtime LOGIN PASSWORD '$runtimePassword' NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOBYPASSRLS;
CREATE ROLE garagemotor_migrations LOGIN PASSWORD '$migrationPassword' NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOBYPASSRLS;
COMMIT;
CREATE DATABASE garagemotor OWNER garagemotor_migrations;
REVOKE ALL ON DATABASE garagemotor FROM PUBLIC;
GRANT CONNECT ON DATABASE garagemotor TO garagemotor_runtime, garagemotor_migrations;
"@
[void](Invoke-PrivatePsql -Database postgres -Sql $sql)
[void](Invoke-PrivatePsql -Database garagemotor -Sql (Get-Content -LiteralPath $migration -Raw))
$verified = Invoke-PrivatePsql -Database garagemotor -Sql "SELECT count(*)=2 AND bool_and(NOT rolsuper AND NOT rolbypassrls) FROM pg_roles WHERE rolname IN ('garagemotor_runtime','garagemotor_migrations');"
if ($verified -ne 't') { throw 'Verificação de papéis falhou; não iniciar a aplicação.' }
Write-Output 'Banco/papéis GarageMotor criados no PostgreSQL compartilhado, migration aplicada e configuração externa gravada com modo 0600. Nenhuma credencial foi exibida.'
