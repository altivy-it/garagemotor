# GarageMotor - Instruções globais

Toda documentação produzida ou atualizada, inclusive artefatos do Spec Kit e
relatórios de Bug Fixing, deve estar em português brasileiro (pt-BR). Preserve
comandos, identificadores, caminhos e marcadores literais exigidos pelas ferramentas.

## Contexto obrigatório

- Runners de testes e ferramentas de build executam em containers; comandos Docker no host apenas os orquestram.
- Sempre que um teste, preview ou validação exigir a aplicação em execução, suba seus serviços em containers e acesse HTTP do navegador e do runner exclusivamente pela borda de um proxy reverso nginx também em container. Testes unitários sem aplicação HTTP em execução continuam em containers e não exigem nginx ocioso.
- Leia o [README](../README.md) antes de alterar contratos, identidade, segurança ou deployment.

## Fluxo de trabalho

1. Comece pelo comportamento, teste relacionado e módulo proprietário.
2. Declare uma hipótese local e uma validação capaz de refutá-la.
3. Faça a menor alteração coerente e valide o recorte imediatamente.
4. Valide primeiro o recorte alterado; depois execute verificações amplas proporcionais ao risco.
5. Atualize contrato, compatibilidade, migração, documentação, segurança e observabilidade quando o comportamento exigir.
6. Preserve alterações existentes e não reformate arquivos fora do escopo.

Esse fluxo técnico só pode ser executado dentro de uma tarefa Spec Kit autorizada
ou da remediação autorizada de Bug Fixing; não cria processo independente.

## Spec Kit e ADRs

- ADRs ficam em `docs/adr/AAAA-NNNN-titulo.md`, começam como `proposed` e seguem `.github/ADR_TEMPLATE.md`.
- Spec Kit é o único processo autorizado para desenvolver e manter a aplicação: features, correções, manutenção e iniciativas de produto exigem seus artefatos e tarefas.
- Features usam `specs/<feature>/spec.md`, `plan.md` e `tasks.md`. Use `/speckit-specify`, `/speckit-clarify` quando necessário, `/speckit-plan`, `/speckit-tasks`, `/speckit-analyze`, autorização humana explícita, `/speckit-implement` e `/speckit-converge`.
- Defeitos usam exclusivamente a extensão oficial Bug Fixing: `/speckit-bug-assess`, `/speckit-bug-fix` e `/speckit-bug-test`. Não crie fluxo independente de diagnóstico/correção.
- `.specify/memory/constitution.md` registra princípios duradouros; `.github/` contém procedimentos operacionais; specs contêm requisitos de cada mudança; ADRs registram decisões duradouras sem duplicar esses artefatos.
- Antes de implementar, confirme feature/diretório, tarefas ou remediação, escopo revisado e autorização explícita. Issue `Ready`, branch, checkbox ou artefato existente não autoriza execução. Selecione `SPECIFY_FEATURE_DIRECTORY` explicitamente quando houver múltiplas features. Para validação read-only, use `SPECIFY_FEATURE_NO_PERSIST=1`.
- Checkboxes em `tasks.md` registram execução validada, não aprovação humana. Tarefas novas de `/speckit-converge` exigem revisão e autorização antes de executar.
- Use `/speckit-constitution` para adoção inicial e emendas aprovadas, registrando apenas princípios existentes ou explicitamente acordados.
- Registre `Origem` como humano ou IA assistida e identifique o revisor humano.
- IA não marca ADR como `accepted`, artefato como aprovado nem altera status histórico sem decisão ou evidência humana explícita e referenciada.

### Extensões e localização

- As extensões oficiais `assess`, `bug` e `git` apoiam o mesmo processo Spec Kit. `assess` registra descoberta em `.specify/assessments/<slug>/`; seu veredito `go` encaminha para especificação, nunca autoriza implementação.
- A configuração usa `auto_execute_hooks: true` e hooks Git habilitados. A invocação de um fluxo Spec Kit autoriza os hooks nele configurados; não autoriza push, publicação, deploy ou implementação fora das tarefas aprovadas.
- Após instalar, atualizar ou reconciliar componentes, use [localize-speckit](prompts/localize-speckit.prompt.md) para auditar ou traduzir textos humanos em pt-BR sem alterar comandos, IDs, placeholders ou contratos de leitura. Sem modo explícito, somente audite.
- A infraestrutura de `.github/`, `.specify/` e `.githooks/` é gerenciada pelo `Altivy.Groundwork`; use `groundwork status` para detectar desvios e `groundwork update` para receber novas versões sem sobrescrita silenciosa.

## Commits

- Todo commit criado ou sugerido segue [Conventional Commits](COMMIT_CONVENTIONS.md) e usa os tipos e escopos aprovados pelo projeto.
- Escreva a descrição em português brasileiro, no imperativo, iniciando com minúscula e sem ponto final.
- Mudanças incompatíveis usam `!` no cabeçalho e o rodapé `BREAKING CHANGE:` com impacto e migração.
- Fora dos hooks autorizados pela invocação do fluxo Spec Kit, não faça commit sem solicitação explícita. Push, publicação e deploy sempre exigem solicitação explícita.

## Regras invariáveis

- Preserve conteúdo do usuário; conflitos nunca podem causar sobrescrita silenciosa.
- Normalize e autorize paths antes de qualquer acesso ao filesystem.
- No desenvolvimento local em Windows, armazene todo secret exclusivamente em `$env:APPDATA\Microsoft\UserSecrets\GarageMotor\.env`; em Linux/macOS, use `~/.microsoft/usersecrets/GarageMotor/.env`.
- Nunca crie arquivos `.env`, `.env.*`, `secrets.json`, cópias ou templates com valores reais dentro do repositório. Não use `dotnet user-secrets`; scripts, Compose, coleções HTTP, testes e documentação devem referenciar o arquivo externo ou variáveis de ambiente injetadas por processo seguro.
- Não leia, exiba, registre, copie ou sobrescreva o conteúdo do arquivo externo durante validações. Antes de mover ou gravar secrets, falhe fechado se o destino já existir.
- Não registre tokens, certificados privados, variáveis de ambiente, secrets ou logs integrais.
- Dependências externas exigem licença permissiva, gratuita, versão fixada e manutenção verificada.
- Nunca sugira, instale ou adicione biblioteca paga, proprietária, source-available, com licença comercial obrigatória, copyleft ou outra restrição incompatível com a distribuição MIT do GarageMotor. Licença ausente, ambígua ou não verificada bloqueia a dependência.
- O padrão arquitetural Mediator é permitido quando houver necessidade comprovada de desacoplar dispatch, handlers e pipelines. Nesses casos, use `Nuuvify.CommonPack.Mediator` com versão centralizada e licença da versão exata verificada; nunca use MediatR.
- Não adicione broker, cache distribuído, microsserviço ou novo datastore sem ADR e evidência operacional.
- APIs públicas usam OpenAPI, `application/problem+json` e versionamento explícito.
- Mudanças de schema seguem expand-contract e devem ser testadas com banco real.
- Tabelas com potencial de crescimento elevado ou retenção temporal devem usar Table Partitioning/Declarative Partitioning por faixa de data, preferencialmente partições mensais; a solução deve definir criação antecipada, rollover, partição de segurança para datas fora da janela, índices por partição, retenção e testes de roteamento antes da produção.

## Qualidade mínima

- Código novo possui testes nos níveis adequados ao risco.
- Rode formatador, análise estática, build e testes do projeto alterado.
- Para fluxos do usuário, atualize ou adicione Playwright.
- Para banco de dados e filesystem, use testes de integração reais, não mocks como evidência principal.
- Imagens possuem build e smoke test.
- Superfícies privilegiadas recebem threat model e revisão de segurança.
- Reviews priorizam integridade de dados, segurança, regressões, concorrência e lacunas de teste.

## Instruções especializadas

As regras detalhadas em `.github/instructions/` são carregadas pelo caminho alterado ou pela descrição. Não replique essas regras em código ou documentação local.
