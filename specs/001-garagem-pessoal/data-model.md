# Modelo de Dados: Garagem pessoal e familiar

**Data**: 2026-11-08. **Origem**: IA assistida. **Revisor humano**: CEO da Altivy e owner do GarageMotor.
**Estado**: desenho proposto, sem migrations ou implementação executadas.
**Base**: [spec](spec.md), [pesquisa](research.md) e [ADR de persistência](../../docs/adr/2026-0001-persistencia-garagem.md).

## Convenções e invariantes

- Identificadores UUID; versão inteira positiva incrementada em mutações. Datas de negócio são `date`, instantes técnicos UTC e fuso da garagem IANA.
- Dinheiro em centavos BRL, inteiro de 1 a 999.999.999.999; volume em mililitros inteiros de 1 a 1.000.000; odômetro em quilômetros inteiros de 0 a 9.999.999. Cálculos usam decimal, nunca float/double.
- Todo registro de domínio possui `garage_id`; relações incluem a mesma garagem. Consultas usam parâmetro e contexto autorizado, sem identificar recurso de outra garagem.
- Mutações exigem versão esperada e uma transação. POST financeiro exige chave de operação estável; correções atualizam registro e despesa vinculada atomicamente, com autoria mínima.
- A versão SQL/publicação do schema será definida por migrations expand-contract e testes reais. Não presumir que UUID seja uma restrição global em tabelas particionadas.

## Entidades e campos

### Usuário, sessão e recuperação — FR-001

| Entidade | Campos e limites | Regras |
|---|---|---|
| User | id UUID; email normalizado até 254; nome até 100; password_hash; estado active/disabled; version | Email único normalizado; PasswordHasher/Identity do framework; não expor hash ou existência de conta em recuperação |
| Session | id; user_id; token_hash; created_utc; expires_utc; revoked_utc opcional | Token opaco aleatório de pelo menos 256 bits, somente hash persistido; expira em 12 horas; revogação imediata; token nunca em logs/exportações |
| RecoveryCode | user_id; code_hash; expires_utc; consumed_utc | Código de uso único; fluxo por SMTP configurado ou códigos offline previamente entregues ao usuário; sem bypass por endpoint público |

Senhas e códigos não são implementados com algoritmo próprio. SMTP é opcional; recuperação offline deve funcionar sem SaaS e exigir posse do código. Credenciais de operação ficam no arquivo externo e não em tabelas/artefatos do repositório.

### Garagem, membro e convite — FR-002–FR-004

| Entidade | Campos e limites | Regras |
|---|---|---|
| Garage | id; name 1–100; timezone IANA default America/Sao_Paulo; state active/deleting; owner_user_id; version | Criador é proprietário; exclusão exige confirmação; uma única propriedade vigente |
| Member | garage_id; user_id; role owner/editor/reader; state active/revoked; version | Único por garagem/usuário; último owner não pode sair; transferência atômica |
| Invite | id; garage_id; email normalizado; role editor/reader; token_hash; expires_utc; state pending/accepted/revoked/expired | 72 horas; token de uso único; identidade correspondente; não permite criar segundo owner por convite |

Membership é verificada em cada operação, não apenas no login. A mesma identidade pode pertencer a garagens distintas; cabeçalho de sessão não concede acesso global.

### Veículo — FR-005

`id`, `garage_id`, `label` 1–100, `kind` car/motorcycle, `brand` 1–100, `model` 1–100, `year` 1900–ano atual+1, `fuel` gasoline/ethanol/diesel/flex/electric/other, `plate` opcional até 10, `renavam` opcional 11 dígitos, `chassis` opcional até 17 e `state` active/archived, `version`.

Arquivar preserva vínculos e histórico. Novos registros financeiros ou de uso em veículo arquivado são recusados; consultar/corrigir histórico por papel autorizado continua possível. Placa/chassi/Renavam não são obrigatórios nem usados como autorização.

### Despesa e alteração — FR-006–FR-009

`id`, `garage_id`, `vehicle_id`, `spent_on`, `category` fuel/maintenance/insurance/tax/other, `description` 1–500, `amount_minor` nos limites acima, `currency` BRL, `origin` manual/fuel/maintenance, `origin_id` opcional, `state` active/voided, `version`, `created_by`, `changed_by`.

Cada abastecimento ou manutenção com custo corresponde a uma despesa única. Despesas manuais de combustível não geram medição de consumo automaticamente. Anulação retira valor dos totais, mas preserva registro até exclusão da garagem. Alterações guardam campos alterados, autoria e instante em histórico restrito; logs operacionais não recebem valores/documentos pessoais.

Despesas são particionadas mensalmente por `spent_on`. Chave de referência é `(id, spent_on, garage_id)`; filhos carregam a data financeira quando referenciam a despesa. Alteração da data move partição e atualiza os vínculos na mesma transação, com teste de rollback.

### Abastecimento — FR-010–FR-013

`id`, `garage_id`, `vehicle_id`, `occurred_on`, `odometer_km`, `volume_ml`, `fuel` gasoline/ethanol/diesel/other, `full_tank` boolean, `station` opcional até 200, `expense_id`, `expense_date`, `historical_reason` opcional até 500, `state` active/voided, `version`.

Volume, custo e despesa são gravados atomicamente. Intervalos válidos começam/terminam em tanque cheio e usam o volume que recompõe o tanque final, somado aos parciais intermediários. Divisão por zero, leituras regressivas sem justificativa, combustível não comparável ou falta de fechamento produzem indicador indisponível/explicitamente parcial.

Inserção histórica justificada não substitui silenciosamente a última leitura; recálculo considera data, sequência estável e registros ativos. Correção/anulação exige versão e atualiza a despesa vinculada uma única vez.

### Manutenção, plano e compromisso — FR-014–FR-017

| Entidade | Campos e limites | Regras |
|---|---|---|
| Maintenance | id; garage_id; vehicle_id; kind preventive/corrective; description 1–500; completed_on; odometer_km; expense_id/date opcional; plan_id opcional; version | Custo opcional gera uma única despesa; conclusão é factual, não previsão mecânica |
| MaintenancePlan | id; garage_id; vehicle_id; title 1–200; due_on opcional; due_km opcional; interval_months 1–120 opcional; interval_km 1–1.000.000 opcional; state active/paused/completed; version | Pelo menos um limite não nulo; recorrência tem limite correspondente; data ou km, primeiro atingido |
| Commitment | id; garage_id; vehicle_id; kind insurance/licensing/inspection/other; title 1–200; due_on/km; completed_on/km opcional; recurrence opcional; state active/completed/cancelled; version | Usa as mesmas regras de limite; concluir pode criar próximo ciclo, nunca duplicar ao repetir |

Próximo vencimento parte da conclusão registrada. Proximidade: 30 dias ou 500 km. Quilometragem vem da última leitura válida com sua data; ausência de leitura não é estimada. Reminders são projeções calculadas, não tabela de predição ou push obrigatório.

### Comprovante — FR-018–FR-020

`id`, `garage_id`, `parent_kind` expense/maintenance, `parent_id`, `parent_date` quando despesa, `original_name` até 255, `media_type` PDF/JPEG/PNG, `size_bytes` 1–10.485.760, `sha256`, `storage_key` opaca gerada pelo servidor, `state` pending/ready/deleting, `version`.

Até cinco por pai, conferidos sob bloqueio transacional do pai. Arquivos recebem staging privado, validação de tipo/conteúdo e limite; somente `ready` pode ser recuperado. Nome original não compõe caminho físico. Recuperação exige membership e retorna download attachment com `nosniff`, nunca execução inline. Verificação de tipo não deve ser anunciada como antivírus.

Falha entre disco e banco preserva estado pendente para reconciliação; não confirma sucesso. Staging órfão é removido após 24 horas; arquivos prontos sem vínculo são reconciliados sem excluir arquivo ainda referenciado. Exclusão segue estado deleting e coleta controlada.

### Operação, auditoria e exportação — FR-013/FR-023/FR-024/FR-030

- **OperationReceipt**: garage_id, actor_id, operation_key UUID, request_hash, created_utc, status, resource_id e response mínima. Mesma chave/corpo devolve resultado; chave com corpo diferente retorna conflito. Manter recibos enquanto existir a garagem. Em partições, não presumir unicidade global: lock transacional pela chave e busca entre partições antes da criação, com teste real de concorrência e repetição após rollover.
- **AuditEvent**: id, garage_id opcional, actor_id pseudônimo, action, target_id, changed_fields sem conteúdo sensível, created_utc. Retenção 90 dias; partições mensais. Sem senhas, tokens, arquivos, payload integral ou valores pessoais em logs.
- **ExportJob**: id, garage_id, requesting_owner_id, state pending/running/ready/failed/expired, created_utc, expires_utc, arquivo/checksum. Worker interno na mesma aplicação, sem broker; download reautoriza owner. Exportação expira após 24 horas e não inclui hashes de senha, tokens, códigos ou credenciais operacionais.
- **GarageDeletion**: confirmação do owner, requested_utc, deadline em até 30 dias, estado requested/running/completed/failed. Remover dados ativos/arquivos; backups expiram em até 30 dias; sem apagar o histórico de outra garagem.

## Partições, integridade e retenção

Despesas, medições/eventos históricos, recibos e auditoria têm partições mensais pela data declarada da entidade. Preparar doze meses anteriores e três futuros e manter default para dados fora da janela. Job diário prepara meses futuros; rollover é idempotente e testado.

Cada partição recebe índices por garagem/veículo/data e estado. Inserir registros fora da janela no default não pode falhar ou perder dados; criar uma partição exige mover dados correspondentes do default sob bloqueio/transaction antes de anexá-la. Não prometer unicidade ou FK global impossível entre partições.

Não remover partições financeiras por idade: o histórico é preservado até exclusão da garagem. Retenção de auditoria e arquivos temporários é independente. Drop de partição depende de ausência de vínculos/compatibilidade com retenção e evidência de backup/restauração.

## Autorização, concorrência e recuperação

Npgsql usa SQL parametrizado, transações e contexto de garagem. RLS é defesa adicional nas tabelas de domínio: role runtime sem superuser/BYPASSRLS, `SET LOCAL` dentro da transação e reset por término; migrations usam role separada. Export/worker só recebe contexto autorizado do job persistido, sem selecionar arbitrariamente outra garagem.

`If-Match` contém versão inteira; conflito retorna 409 e não sobrescreve. Chave de operação deve permanecer estável na repetição após falha de rede. Auditoria, despesa e vínculo são commitados juntos.

Backup operacional é separado da exportação familiar: contém banco e volume privados, verificáveis por manifesto e checksums, sem credenciais do arquivo externo. Pausar novas escritas, drenar operações de arquivo, copiar snapshot consistente e reabrir; manutenção retorna indisponibilidade recuperável, não sucesso falso. Restaurar em destino vazio, validar manifesto antes de trocar dados e falhar fechado se destino já existir.

## Cobertura

US1: identidade/garagem/membros. US2: despesas/totais. US3: abastecimento/recibos. US4: manutenção/compromissos. US5: comprovantes. US6: exportação, recuperação e matriz operacional. As relações acima cobrem FR-001–FR-030; desempenho, Android e acessibilidade serão verificados conforme o plano e quickstart.