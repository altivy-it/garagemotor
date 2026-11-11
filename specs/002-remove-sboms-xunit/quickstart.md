# Guia de Validação: xUnit e inventário sem SBOMs

**Feature**: [spec.md](spec.md)

## Pré-requisitos

- Podman rootless disponível; build e runner são executados nos containers fixados pelo projeto.
- SDK .NET 10.0.403 selecionado por `global.json` e cache NuGet isolado/compartilhado conforme os scripts existentes.
- Para integração, serviços PostgreSQL/API/nginx em containers e caminho de secrets externo Linux `~/.microsoft/usersecrets/GarageMotor/.env`; não ler nem copiar seu conteúdo.

## Validar descoberta e categorias

Na raiz do repositório, executar pelo runner oficial:

```powershell
pwsh -NoProfile -File tools/scripts/validate-feature.ps1 -FeatureDirectory specs/002-remove-sboms-xunit -Profile unit
pwsh -NoProfile -File tools/scripts/validate-feature.ps1 -FeatureDirectory specs/002-remove-sboms-xunit -Profile integration -SecretFile ~/.microsoft/usersecrets/GarageMotor/.env
```

Resultado esperado: o perfil `unit` executa apenas testes com `Category=Unit`; `integration` executa apenas `Category=Integration` usando PostgreSQL real e HTTP somente pela borda nginx. A descoberta total deve preservar os 17 métodos de referência (15 Integration, 2 Unit), todos categorizados.

## Validar remoção dos artefatos

- Confirmar que `deploy/sboms/` não existe e que não há arquivos SPDX comprimidos versionados.
- Validar `deploy/images.lock.json` como JSON; conferir que todos os `digestsByPlatform` permanecem e que não há caminhos para `deploy/sboms/` nem metadados de hashes/contagens SBOM removidos.
- Pesquisar referências a caminhos removidos em scripts, inventário e documentação. A menção textual à política de SBOM em `docs/dependencies.md` é intencional; o que não pode permanecer são links ou artefatos inexistentes.
- Conferir a documentação em pt-BR e os avisos/licenças de terceiros preservados.
