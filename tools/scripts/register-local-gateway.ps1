[CmdletBinding()]
param([switch]$Apply)

$ErrorActionPreference = 'Stop'
if (-not $Apply) { throw 'Aplicação da rota exige Apply explícito.' }
$rootless = & podman info --format '{{.Host.Security.Rootless}}' 2>$null
if ($LASTEXITCODE -ne 0 -or "$rootless".Trim() -ne 'true') { throw 'Podman rootless é obrigatório.' }
$platform = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../architecture-knowledge-base/platforms/identity'))
$root = [System.IO.Path]::GetFullPath((Join-Path $HOME '.microsoft/usersecrets/Altivy.Identity'))
$runtime = Join-Path $root 'runtime-rootless'
$composeFile = Join-Path $runtime 'compose.rootless.json'
$nginxFile = Join-Path $runtime 'nginx.conf'
$routesRoot = Join-Path $runtime 'garagemotor-routes'
$sourceServer = Join-Path $platform 'gateway/conf.d/identity.conf'
$sourceLocations = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../deploy/nginx.garagemotor.locations'))
foreach ($file in @($composeFile, $nginxFile, $sourceServer, $sourceLocations)) {
    if (-not (Test-Path -LiteralPath $file -PathType Leaf)) { throw 'Pré-requisito público de roteamento ausente.' }
}
if (Test-Path -LiteralPath $routesRoot) { throw 'Rota já preparada; não será sobrescrita automaticamente.' }
$composeOriginal = Get-Content -LiteralPath $composeFile -Raw
$configuration = $composeOriginal | ConvertFrom-Json
foreach ($service in $configuration.services.psobject.Properties.Value) {
    if ($service.environment) {
        foreach ($property in $service.environment.psobject.Properties) {
            if ($property.Name -match 'PASSWORD|USERNAME' -and -not "$($property.Value)".StartsWith('${', [StringComparison]::Ordinal)) {
                throw 'O adaptador público contém valor de credencial; operação recusada.'
            }
        }
    }
}
$nginxOriginal = Get-Content -LiteralPath $nginxFile -Raw
if ([regex]::Matches($nginxOriginal, [regex]::Escape('include /etc/nginx/conf.d/*.conf;')).Count -ne 1) { throw 'Include do gateway divergiu; revisão manual necessária.' }
$server = Get-Content -LiteralPath $sourceServer -Raw
if ([regex]::Matches($server, '(?m)^server\s*\{').Count -ne 1) { throw 'Servidor de referência não é único.' }
$closing = $server.LastIndexOf('}')
if ($closing -lt 0) { throw 'Servidor de referência inválido.' }
$locations = Get-Content -LiteralPath $sourceLocations -Raw
[System.IO.Directory]::CreateDirectory($routesRoot) | Out-Null
$serverPath = Join-Path $routesRoot 'identity.conf'
$locationsPath = Join-Path $routesRoot 'garagemotor.locations'
$candidatePath = Join-Path $routesRoot 'nginx-check.conf'
[System.IO.File]::WriteAllText($serverPath, $server.Insert($closing, "`n    include /etc/nginx/garagemotor-products/garagemotor.locations;`n"))
[System.IO.File]::WriteAllText($locationsPath, $locations)
$checkServer = Join-Path $routesRoot 'identity-check.conf'
[System.IO.File]::WriteAllText($checkServer, $server.Insert($closing, "`n$locations`n"))
[System.IO.File]::WriteAllText($candidatePath, $nginxOriginal.Replace('include /etc/nginx/conf.d/*.conf;', 'include /tmp/garagemotor-identity-check.conf;'))
& podman cp $checkServer altivy-identity-gateway:/tmp/garagemotor-identity-check.conf *> $null
if ($LASTEXITCODE -ne 0) { throw 'Cópia da configuração candidata falhou.' }
& podman cp $candidatePath altivy-identity-gateway:/tmp/garagemotor-nginx-check.conf *> $null
if ($LASTEXITCODE -ne 0) { throw 'Cópia da configuração candidata falhou.' }
& podman exec altivy-identity-gateway nginx -t -c /tmp/garagemotor-nginx-check.conf *> $null
if ($LASTEXITCODE -ne 0) { throw 'nginx -t recusou a configuração candidata; configuração ativa preservada.' }
$newMount = [pscustomobject]@{ type = 'bind'; source = $routesRoot; target = '/etc/nginx/garagemotor-products'; read_only = $true; bind = [pscustomobject]@{ selinux = 'z' } }
$configuration.services.gateway.volumes = @($configuration.services.gateway.volumes) + @($newMount)
[System.IO.File]::WriteAllText($composeFile, ($configuration | ConvertTo-Json -Depth 15))
[System.IO.File]::WriteAllText($nginxFile, $nginxOriginal.Replace('include /etc/nginx/conf.d/*.conf;', 'include /etc/nginx/garagemotor-products/identity.conf;'))
$privateFile = Join-Path $root '.env'
& podman compose --env-file $privateFile -f $composeFile up --detach --no-deps --force-recreate gateway *> $null
if ($LASTEXITCODE -ne 0) {
    [System.IO.File]::WriteAllText($composeFile, $composeOriginal)
    [System.IO.File]::WriteAllText($nginxFile, $nginxOriginal)
    throw 'Aplicação da rota falhou; configuração pública restaurada. Nenhum secret foi lido.'
}
& podman exec altivy-identity-gateway nginx -t *> $null
if ($LASTEXITCODE -ne 0) { throw 'Gateway não validou a configuração aplicada; revisar antes de conectar produtos.' }
Write-Output 'Rota /garagemotor/ instalada no gateway compartilhado após nginx -t; rotas de identidade/notes/dokpod preservadas no servidor de referência.'
