# Contribuindo com o GarageMotor

## Antes de começar

1. Leia o [README](../README.md) e os documentos da área afetada.
2. Procure os artefatos Spec Kit, tarefa/remediação, Issue e ADR relacionados.
3. Confirme autorização humana explícita e limites da tarefa/remediação antes de executar; Issues são intake, não autorização.
4. Nunca inclua secrets em nenhuma parte do código ou configuração, inclusive
   workflows, testes, documentação, exemplos, scripts e connection strings.
   Secrets locais permanecem exclusivamente no arquivo externo
   `$env:APPDATA\Microsoft\UserSecrets\GarageMotor\.env`, sem leitura por automações, e secrets de CI
   permanecem nos Environments do GitHub.

## Licenciamento de contribuições

O código é distribuído sob `MIT`; consulte o arquivo [LICENSE](../LICENSE).

## Fluxo

Os formulários usam as labels declaradas em [labels.json](labels.json).
Gerar arquivos não cria labels nem habilita controles no GitHub. O mantenedor
deve executar `groundwork labels` com GitHub CLI autenticado para diagnosticar
as ausentes e, se aprovado, `groundwork labels --sync --dry-run` seguido de
`groundwork labels --sync`. Labels existentes não são renomeadas, removidas
nem têm cores e descrições sobrescritas.

1. Para features e manutenção, use o processo Spec Kit de especificação, esclarecimento, planejamento, tarefas e análise. Requisitos, desenho e execução pertencem a `specs/<feature>/spec.md`, `plan.md` e `tasks.md`.
2. Para defeitos, use a extensão oficial Bug Fixing: `/speckit-bug-assess`, remediação explicitamente autorizada via `/speckit-bug-fix` e verificação via `/speckit-bug-test`. Sem artefatos ou extensão, pare; não substitua por fluxo local.
3. Execute somente tarefas explicitamente autorizadas via `/speckit-implement`, respeitando dependências e bloqueios de `/speckit-analyze`. A criação de branch ou a Issue não autoriza execução.
4. Atualize testes, contratos, documentação e telemetria necessários à tarefa; valide o menor recorte primeiro e registre evidências no artefato e PR.
5. Use `/speckit-converge` para pendências, sem ampliar escopo por conta própria. ADR auxiliar exige justificativa de `/speckit-plan` e permanece `proposed` até decisão humana.
6. Use os [Conventional Commits](COMMIT_CONVENTIONS.md) somente quando houver solicitação de commit. Commit, push, merge, publicação e deploy exigem autorização separada.

## Definition of Done

- escopo e autorização rastreáveis, com evidências no artefato de origem;
- critérios de aceite demonstrados;
- código formatado, analisado e compilado;
- testes reais proporcionais ao risco;
- contratos e clientes gerados sincronizados;
- segurança, acessibilidade, concorrência e desempenho considerados;
- logs e métricas não expõem conteúdo sensível;
- rollout, rollback e compatibilidade documentados;
- nenhum achado crítico ou alto aberto sem aceitação formal.

## Pull requests

Mantenha PRs coesos e pequenos o bastante para revisão responsável. Mudanças incompatíveis exigem issue, ADR quando aplicável, estratégia de migração e período de compatibilidade. Não misture refactor amplo com mudança funcional.
