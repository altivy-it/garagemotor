---
description: "Tarefas para remover SBOMs versionados e migrar os testes backend para xUnit"
---

# Tarefas: Remoção de SBOMs e adoção do xUnit

**Entrada**: Documentos de design em `specs/002-remove-sboms-xunit/`.
**Pré-requisitos**: [spec](spec.md), [plano](plan.md), [pesquisa](research.md), [modelo](data-model.md) e [quickstart](quickstart.md).
**Origem**: solicitação humana. **Revisor humano**: CEO da Altivy e owner do GarageMotor.
**Estado**: execução autorizada pelo owner em 2026-11-11; T001–T014 executadas. Build, formato, suíte direta e perfis oficiais `quality`/`unit` passaram; `integration` bloqueia pela ausência de `deploy/Dockerfile.api` prevista em T067 da feature 001.

**Atualização de validação (2026-11-11)**: após formatação Prettier dos quatro arquivos listados em T013, o perfil oficial `quality` passou. O perfil `unit` também passou. O perfil `integration` agora ultrapassa o gate de digest, mas continua bloqueado por `deploy/Dockerfile.api` ausente; não acessou secrets nem iniciou serviços pelo script.

## Formato: `[ID] [P?] [Story] Descrição`

- **[P]**: pode ser executada em paralelo quando não houver dependências nem arquivos compartilhados.
- **[Story]**: história relacionada, obrigatória nas fases de histórias.
- Todas as tarefas indicam caminhos reais e preservam a política de containers do projeto.

## Fase 1: Preparação (Infraestrutura Compartilhada)

**Objetivo**: preparar a dependência xUnit com versão e licença verificadas.

- [X] T001 Substituir `MSTest.Sdk` por `xunit.v3` 4.0.3 em `backend/GarageMotor.Tests/GarageMotor.Tests.csproj`, centralizar a versão em `backend/Directory.Packages.props` e regenerar `backend/GarageMotor.Tests/packages.lock.json` em container; confirmar `Apache-2.0`, compatibilidade MTP v2/.NET 10 e auditar todas as dependências transitivas antes de uso. Restore locked passou; licenças xUnit/MTP e transitivas conferidas nos nuspecs oficiais.

## Fase 2: Fundação (Pré-requisitos Bloqueadores)

**Objetivo**: preservar serialização de testes que compartilham PostgreSQL real.

**CRÍTICO**: concluir esta fase antes de migrar testes de integração.

- [X] T002 Criar uma coleção xUnit não paralela em `backend/GarageMotor.Tests/Fixtures/IntegrationTestCollection.cs` e definir como os quatro grupos de teste em `backend/GarageMotor.Tests/Foundation/AuditTests.cs`, `backend/GarageMotor.Tests/Foundation/PersistenceTests.cs`, `backend/GarageMotor.Tests/Foundation/SecurityTests.cs` e `backend/GarageMotor.Tests/Operations/OperationStoreTests.cs` preservam o comportamento de `[DoNotParallelize]`. As quatro classes usam a collection desabilitada para paralelismo.

**Checkpoint**: pacote restaurado em modo locked e coleção de integração disponível; nenhum teste pode desaparecer nem concorrer pelo estado compartilhado.

## Fase 3: História de Usuário 1 - Executar testes backend por categoria (Prioridade: P1) MVP

**Objetivo**: migrar todos os testes .NET para xUnit e selecionar Unit/Integration por metadata.

**Teste independente**: descobrir 17 métodos, executar 2 com `Category=Unit` e 15 com `Category=Integration`, com integração real por containers/nginx.

### Testes da História de Usuário 1

- [X] T003 [P] [US1] Migrar `backend/GarageMotor.Tests/Foundation/AuditTests.cs` para atributos/assertions xUnit e atribuir `Category=Integration` ao teste PostgreSQL e `Category=Unit` ao teste de `ProblemResponses`.
- [X] T004 [P] [US1] Migrar `backend/GarageMotor.Tests/Foundation/PersistenceTests.cs` para xUnit e atribuir `Category=Integration` aos quatro testes PostgreSQL de RLS, contexto, recibos e partições.
- [X] T005 [P] [US1] Migrar `backend/GarageMotor.Tests/Foundation/SecurityTests.cs` para xUnit e atribuir `Category=Integration` aos oito testes de sessão, CSRF, recuperação e limites.
- [X] T006 [P] [US1] Migrar `backend/GarageMotor.Tests/Operations/OperationStoreTests.cs` para xUnit e atribuir `Category=Unit` ao hash JSON e `Category=Integration` aos dois testes de recibos PostgreSQL.

### Implementação da História de Usuário 1

- [X] T007 Atualizar `tools/scripts/validate-feature.ps1` para usar `--filter-trait Category=Unit`, `--filter-trait Category=Integration` e a mesma sintaxe para `Category=Load` nos perfis unit/integration/load; manter containers, filtros de segredo e endereço HTTP via nginx inalterados.
- [X] T008 Validar `backend/GarageMotor.Tests/GarageMotor.Tests.csproj` com restore locked, build, formato e execução completa e filtrada via runner MTP: preservar 17 testes descobertos, total Unit=2 e Integration=15, sem converter testes reais de integração em mocks. Build/formato passaram; Unit passou 2/2, Integration 15/15 e a suíte completa 17/17 pelo runner integrado.

**Checkpoint**: cada teste backend possui categoria e os perfis unit/integration executam somente seu conjunto.

## Fase 4: História de Usuário 2 - Manter o repositório sem SBOMs versionados (Prioridade: P1)

**Objetivo**: remover os artefatos SPDX sem perder pins nem informações necessárias de terceiros.

**Teste independente**: verificar ausência de arquivos SBOM e referências quebradas, validar `deploy/images.lock.json` e comparar os pins por plataforma.

### Testes da História de Usuário 2

- [X] T009 [P] [US2] Remover os 12 arquivos SPDX comprimidos de `deploy/sboms/` e excluir o diretório vazio; não remover qualquer imagem ou dado fora desse inventário.
- [X] T010 [P] [US2] Remover de `deploy/images.lock.json` caminhos/hash/contagens SBOM, `sbomByPlatform`, atestação/camada e relatórios derivados da análise de camadas; preservar tags, origens, `digestsByPlatform` e metadata de licença/termos aplicável.

### Implementação da História de Usuário 2

- [X] T011 Atualizar `docs/dependencies.md` para substituir o inventário derivado das SBOMs, documentar em pt-BR a política de não mantê-las no repositório e ressalvar obrigações de terceiros na distribuição; atualizar a tabela MSTest para `xunit.v3` 4.0.3/Apache-2.0 e manter registros históricos claramente identificados como superados.
- [X] T012 Validar `deploy/images.lock.json` com parser JSON, confirmar que todos os pins por plataforma continuam presentes, que `deploy/sboms/` e referências a caminhos removidos não existem em `deploy/`, `docs/` ou scripts, e que o texto normativo da política é a única menção SBOM intencional na documentação de dependências.

**Checkpoint**: nenhum artefato SBOM é versionado; imagem, digest, licenças e avisos necessários continuam rastreáveis.

## Fase 5: Refinamento e Aspectos Transversais

**Objetivo**: executar os gates da feature de ponta a ponta e registrar os resultados.

- [X] T013 Executar os perfis `quality`, `unit` e `integration` de `tools/scripts/validate-feature.ps1` para `specs/002-remove-sboms-xunit`, validar links e conteúdo de `docs/dependencies.md` e registrar evidências sanitizadas nesta feature; não ler secrets nem iniciar HTTP fora da borda nginx. Resultado após T014: unit passou; integration bloqueia por `deploy/Dockerfile.api` ausente antes de acessar secrets; quality falha em `prettier --check` em `frontend/web/src/app/app.routes.ts`, `frontend/web/src/app/core/api-client.ts`, `frontend/web/src/app/core/session.ts` e `frontend/web/src/app/features/auth/auth.component.ts`, sem alterá-los. A remoção do gate de camada foi confirmada; a validação direta passou build/format dos testes, Unit 2/2, Integration 15/15, suíte 17/17, parser JSON e link local.

## Dependências e Ordem de Execução

### Dependências entre Fases

- **Preparação (Fase 1)**: T001 fixa e restaura a dependência xUnit sob locked mode.
- **Fundação (Fase 2)**: T002 depende de T001 e bloqueia a migração de testes que usam PostgreSQL compartilhado.
- **História 1 (Fase 3)**: T003–T006 dependem de T001–T002; T007 depende da taxonomia adotada; T008 depende de T003–T007.
- **História 2 (Fase 4)**: T009 e T010 podem começar após revisão da feature; T011 e T012 dependem do inventário atualizado.
- **Refinamento**: T013 depende da conclusão das duas histórias.

### Dependências entre Histórias

- **História 1 (P1)**: pode ser validada sem remover SBOMs; depende apenas da preparação xUnit e da coleção de integração.
- **História 2 (P1)**: pode ser validada sem migrar os testes; não altera código de produção.

### Oportunidades de Paralelismo

- T003–T006 alteram arquivos de teste distintos e podem ocorrer em paralelo após T001–T002.
- T009 e T010 alteram artefatos distintos e podem ocorrer em paralelo.
- A História 1 e a História 2 podem ser executadas em paralelo após aprovação, evitando simultaneidade em `docs/dependencies.md`.

## Exemplo de Paralelismo: História de Usuário 1

```text
Após T001–T002:
Task: migrar AuditTests e categorizar cada método
Task: migrar PersistenceTests e categorizar cada método
Task: migrar SecurityTests e categorizar cada método
Task: migrar OperationStoreTests e categorizar cada método
```

## Estratégia de Implementação

### MVP Primeiro

1. Concluir T001–T002.
2. Executar T003–T007 e validar T008 com os 17 testes preservados e filtráveis.
3. Executar T009–T012 mantendo pins e documentação consistentes.
4. Executar T013 e revisar evidências antes de marcar tarefas como concluídas.

### Entrega Incremental

1. Entregar a História 1 como migração testável, sem alteração de runtime.
2. Entregar a História 2 como limpeza do repositório/inventário com pins preservados.
3. Executar os gates combinados e não criar commit, push ou publicação sem solicitação explícita.

## Phase 6: Convergence

- [X] T014 Remover o gate obrigatório de `imageLayerLicenseInventoryByPlatform` em `tools/scripts/validate-feature.ps1` e `tools/scripts/start-local.ps1`, conforme Constituição III; preservar a exigência de digests/plataformas fixados, os gates reais de dependências/termos, a borda nginx e a injeção externa de secrets (Constitution III, contradicts). Parser PowerShell passou; o perfil unit oficial passou; integration passou o preflight de imagem e bloqueou no Dockerfile ausente; quality também passou o gate de imagem e chegou ao Prettier.

**Evidência posterior de T014**: `validate-feature.ps1 -Profile quality` e `-Profile unit` passaram após tornar `Ensure-NuGetCache` idempotente para volume já existente. `-Profile integration` retorna `blocked` somente por `deploy/Dockerfile.api` ausente.
