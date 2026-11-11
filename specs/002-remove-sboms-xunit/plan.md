# Plano de Implementação: Remoção de SBOMs e adoção do xUnit

**Branch**: `002-remove-sboms-xunit` | **Data**: 2026-11-10 | **Spec**: [spec.md](spec.md)

**Entrada**: Requisitos em `specs/002-remove-sboms-xunit/spec.md`.

**Origem**: solicitação humana. **Revisor humano**: CEO da Altivy e owner do GarageMotor.

## Resumo

Migrar os testes backend hoje baseados em `MSTest.Sdk` para `xunit.v3` 4.0.3, preservando a integração nativa com Microsoft.Testing.Platform (MTP) v2 e classificando cada teste com trait `Category=Unit` ou `Category=Integration`. Atualizar os filtros dos perfis `unit`, `integration` e `load` em `tools/scripts/validate-feature.ps1`.

Excluir os 12 artefatos SPDX comprimidos em `deploy/sboms/`, remover seus metadados derivados de `deploy/images.lock.json` e substituir o inventário detalhado de camadas em `docs/dependencies.md` por uma política em pt-BR que registre que SBOMs não são mantidos no repositório como requisito geral. Preservar pins de imagem por digest/plataforma, origens e informações de licença/termos necessárias.

## Contexto Técnico

**Linguagem/Versão**: C#/.NET SDK 10.0.403; `global.json` já seleciona MTP para `dotnet test`.

**Dependências Principais**: `xunit.v3` 4.0.3, Apache-2.0, com suporte MTP v2 e .NET 8+; `xunit.analyzers` 2.3.0, Apache-2.0. Versões exatas verificadas nos nuspecs oficiais NuGet. O lock NuGet deve ser resolvido em container e auditado antes de uso. Não adicionar `xunit.runner.visualstudio` ou `Microsoft.NET.Test.Sdk` sem necessidade de compatibilidade demonstrada.

**Persistência**: Sem alteração de schema. Os 15 testes de integração usam PostgreSQL real; testes HTTP continuam acessando a aplicação somente pela borda nginx em container.

**Testes**: Há 17 métodos backend atualmente: 15 já marcados como integração e 2 testes unitários sem categoria. Migrar os quatro arquivos de teste em `Foundation/` e `Operations/`, substituir APIs MSTest, classificar todos os métodos e preservar serialização de testes que compartilham recursos. O helper `RealEnvironment.cs` não contém testes.

**Plataforma Alvo**: runners e aplicação executados em containers Linux Podman; nenhuma API, imagem de runtime ou comportamento de produção é alterado.

**Tipo de Projeto**: Manutenção interna de testes backend, scripts de validação, inventário de imagens e documentação.

**Metas de Desempenho**: N/A. A migração não deve aumentar materialmente o tempo da suíte nem reduzir testes descobertos.

**Restrições**: Manter MTP e `global.json`; categorias obrigatórias `Unit` e `Integration`; testes de integração continuam serializados onde há estado compartilhado. Não manter artefatos SBOM ou caminhos para arquivos removidos. Não remover digest pins ou obrigações de terceiros em caso de distribuição. Nenhuma dependência sem licença permissiva, gratuita e verificada.

**Escala/Escopo**: Um projeto de teste e 17 métodos, cinco arquivos-fonte de teste/fixture, um script runner, um lock de imagens, um inventário de dependências e 12 arquivos SPDX comprimidos.

## Verificação da Constituição

*GATE: revisar após o design da Fase 1; implementação permanece bloqueada até análise, revisão e autorização humana explícita.*

| Princípio | Situação | Tratamento |
|---|---|---|
| I. AGPL e reciprocidade | Conforme | Nenhuma alteração à licença do produto; dependências xUnit são Apache-2.0 conforme nuspec oficial. |
| II. Self-hosted | Conforme | Nenhum serviço SaaS, licenciamento online ou dependência de produção adicionados. |
| III. Tecnologia e dependências | Conforme, sujeito ao lock | xUnit 4.0.3 e analyzer 2.3.0 têm licença Apache-2.0 verificada; resolver e auditar o lock transitivo antes da implementação. Manter imagem e licença de projeto separadas dos SBOMs removidos. |
| IV. Segurança, confidencialidade e integridade | Conforme | Preservar testes PostgreSQL reais, sessão/CSRF, rate limits e serialização dos testes concorrentes; não alterar código de produção. |
| V. Mudanças verificáveis e autorizadas | Conforme com gates | A spec e o plano não autorizam código; tarefas devem ser analisadas e autorizadas antes de executar. Qualidade/restore/testes permanecem em containers. |

Não é necessária emenda constitucional: a versão 2.0.0 já define SBOMs como opcionais para uso local/self-hosted. A documentação da feature explicará a decisão de não mantê-los no repositório sem proibir geração externa quando uma necessidade concreta surgir.

## Estrutura do Projeto

### Documentação desta Funcionalidade

```text
specs/002-remove-sboms-xunit/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── tasks.md
```

### Código-Fonte e Documentação Afetados

```text
backend/
├── Directory.Packages.props
└── GarageMotor.Tests/
    ├── GarageMotor.Tests.csproj
    ├── packages.lock.json
    ├── Foundation/{AuditTests,PersistenceTests,SecurityTests}.cs
    └── Operations/OperationStoreTests.cs
tools/scripts/validate-feature.ps1
deploy/
├── images.lock.json
└── sboms/                  # remover os 12 artefatos SPDX
docs/dependencies.md
```

**Decisão de Estrutura**: manter um único projeto backend de testes e atualizar seus quatro arquivos com métodos de teste. Centralizar a versão do xUnit no mecanismo NuGet já usado; preservar o lockfile do projeto. Categorizar cada método com trait, usando a coleção xUnit apropriada para preservar a execução serializada dos testes de integração. O inventário de imagens permanece em `deploy/images.lock.json`, sem snapshots SPDX.

Não há contrato externo, migration, entidade persistida ou alteração ao `global.json`; `contracts/` não se aplica.

## Registro de Complexidade

Não há violações da constituição nem necessidade de novos serviços, datastores, frameworks de UI ou dependências de produção.
