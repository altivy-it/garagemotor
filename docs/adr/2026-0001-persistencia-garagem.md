# ADR 2026-0001: PostgreSQL e volume privado para a garagem pessoal

**Status:** proposed

**Data:** 2026-11-08
**Responsáveis:** Altivy
**Origem:** IA assistida
**Revisor humano:** CEO da Altivy e owner do GarageMotor
**Relacionado:** [spec](../../specs/001-garagem-pessoal/spec.md), [plano](../../specs/001-garagem-pessoal/plan.md) e [pesquisa](../../specs/001-garagem-pessoal/research.md)

**Revisão:** 2026-11-09, incorporando os esclarecimentos sem aceite automático do datastore.

## Contexto

O MVP familiar precisa de consistência entre despesas, medições, membros e comprovantes, isolamento entre garagens e recuperação self-hosted. Histórico e auditoria podem crescer; a constituição exige decisão explícita de datastore e particionamento antes da implementação.

## Forças de decisão

- Transações, concorrência, autorização e precisão financeira.
- Instalação própria simples, custo de operação e recuperação comprovada.
- Versão mantida, licença permissiva e compatibilidade .NET.
- Evitar storage distribuído, brokers e cache sem necessidade.

## Opções consideradas

### Opção A — PostgreSQL e volume privado

Npgsql 10.0.3 e PostgreSQL 18.6 têm fontes/licenças verificadas na pesquisa. Transações e partições mensais permitem histórico consistente. Volume local evita fornecedor de storage; exige reconciliar disco/banco e backup coordenado.

### Opção B — Banco embarcado e storage externo

Banco embarcado simplifica instalação mínima, mas não oferece o particionamento declarativo requerido e amplia risco de concorrência/qualificação. S3/MinIO adicionaria dependência, operação e licenças sem evidência de necessidade neste recorte.

## Decisão

Propor PostgreSQL 18.6, Npgsql 10.0.3 e volume privado local. Um backend modular, sem novo broker/cache/microsserviço. Usar SQL parametrizado e migrations expand-contract; não adicionar ORM por conveniência.

Este ADR não está aceito. A revisão humana deve confirmar datastore, matriz e condições abaixo antes de implementar. Volume compartilhado/múltiplas réplicas não fazem parte da matriz inicial.

## Consequências

### Positivas

- Transações para valores, fontes únicas, idempotência e membros.
- Operação própria sem licença online ou storage de terceiros.
- RLS como defesa adicional e partições/indexação conforme retenção.

### Negativas e trade-offs

- Manutenção de migrations/partições e SQL próprio.
- Consistência entre arquivo e banco exige staging, reconciliação e pausa de escrita para backup.
- Não há HA automática ou escala horizontal do volume nesta entrega.

## Segurança e privacidade

Role runtime sem superuser/BYPASSRLS, membership por operação e contexto transacional de garagem. Volume não publicado pelo nginx; download passa por autorização e usa attachment/nosniff. Secrets operacionais permanecem no arquivo externo. Operador self-hosted controla a infraestrutura; não prometer criptografia ponta a ponta ou isolamento contra seu próprio DBA.

## Dados, protocolo e compatibilidade

Despesas, histórico, recibos e auditoria particionados mensalmente; doze meses anteriores, três futuros e default seguro. Chaves/relacionamentos incluem a data da partição quando necessário. Não presumir constraint de unicidade global inexistente; idempotência combina lock transacional e busca entre partições.

Retenção financeira até exclusão da garagem; auditoria90dias. Pedido fixa requestedUtc+30dias para dados ativos, arquivos e todos os backups sob gestão contendo a garagem, inclusive anteriores ao pedido ou criados durante processamento; novo snapshot não reinicia o prazo. APIs públicas versionadas e problem+json; mudanças de schema usam expand-contract e testes reais.

BackupCatalog e DeletionAuthority integram o controle operacional da persistência proposta, sem datastore adicional. Autoridade atual não é sobrescrita pelo dump restaurado: deve demonstrar procedência, integridade e cobertura desde a data do snapshot até o ponto coordenado de promoção. Reaplicar todos os pedidos pertinentes, inclusive posteriores à cópia, antes de liberar dados; invalidar sessões/códigos antigos. Ausência de evidência, histórico incompleto ou autoridade desatualizada bloqueiam restauração. Operador mantém o registro independente, sem serviço SaaS obrigatório; cópias particulares externas não têm eliminação garantida pelo app.

## Observabilidade e operação

Métricas de latência, falhas de autorização, conflitos, jobs/partições, arquivos pendentes e restauração, sem conteúdo privado. Job diário prepara partições e reconcilia arquivos; falhas bloqueiam promoção de release. Consultas e alertas operacionais ficam restritos ao administrador autorizado.

## Validação

- Integração real com duas garagens, role runtime e conexão reaproveitada, sem vazamento após SET LOCAL.
- Precisão de valores/consumo e repetição/concorrência de toda criação de domínio, incluindo garagem com escopo de ator antes de existir garageId, sem duplicação após rollover.
- Roteamento dentro/fora da janela, índices por partição, migração do default e retenção segura.
- Falhas entre disco e banco, autoridade perdida/desatualizada e restauração isolada que reaplica pedidos posteriores ao snapshot; prazo de exclusão respeitado em todas as cópias gerenciadas, sem afetar outras garagens.
- Carga SC-005: dez usuários, 10.000 despesas/20garagens/60veículos/12meses; aquecimento2min/medição10min, 80%histórico/20%totais e P95<=2s por tipo, sem erros inesperados na matriz do plano.

Nenhuma dessas validações foi executada nesta fase de desenho.

## Rollout e rollback

Implantar somente após aceite humano e gates de tarefas/análise. Criar estruturas por expand-contract, qualificar com dados fictícios e backup. Rollback usa release compatível; nunca descartar automaticamente coluna/partição com dados. Se recuperação falhar, manter origem intacta e não promover destino parcial.

## Referências

- [Modelo de dados](../../specs/001-garagem-pessoal/data-model.md)
- [Constituição](../../.specify/memory/constitution.md)
- [Pesquisa de versões/licenças](../../specs/001-garagem-pessoal/research.md)