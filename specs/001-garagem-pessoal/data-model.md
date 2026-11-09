# Modelo de Dados: Garagem pessoal e familiar

**Data**: 2026-11-08. **Origem**: IA assistida. **Revisor humano**: CEO da Altivy e owner do GarageMotor.
**Estado**: desenho proposto, sem migrations ou implementação executadas.
**Base**: [spec](spec.md), [pesquisa](research.md) e [ADR de persistência](../../docs/adr/2026-0001-persistencia-garagem.md).

**Revisão**: 2026-11-09, alinhada às quatro respostas da sessão de esclarecimento. Tarefas anteriores precisam de revisão; não há implementação autorizada.

## Convenções e invariantes

- Identificadores UUID; versão inteira positiva incrementada em mutações. Datas de negócio são `date`, instantes técnicos UTC e fuso da garagem IANA.
- Dinheiro em centavos BRL, inteiro de 1 a 999.999.999.999; volume em mililitros inteiros de 1 a 1.000.000; odômetro em quilômetros inteiros de 0 a 9.999.999. Cálculos usam decimal, nunca float/double.
- Todo registro de domínio possui `garage_id`; relações incluem a mesma garagem. Consultas usam parâmetro e contexto autorizado, sem identificar recurso de outra garagem.
- Mutações exigem versão esperada e uma transação. Toda criação de recurso de domínio de FR-013 exige chave de operação estável; cadastro de conta e abertura de sessão têm regras próprias. Correções atualizam registro e despesa vinculada atomicamente, com autoria mínima.
- A versão SQL/publicação do schema será definida por migrations expand-contract e testes reais. Não presumir que UUID seja uma restrição global em tabelas particionadas.

## Entidades e campos

### Usuário, sessão e recuperação — FR-001

| Entidade | Campos e limites | Regras |
|---|---|---|
| User | id UUID; email normalizado até 254; nome até 100; password_hash; estado active/disabled; version | Email único normalizado; PasswordHasher/Identity do framework; não expor hash ou existência de conta em recuperação |
| Session | id; user_id; token_hash; created_utc; expires_utc; revoked_utc opcional | Token opaco aleatório de pelo menos 256 bits, somente hash persistido; expira em 12 horas; revogação imediata; token nunca em logs/exportações |
| RecoveryBatch | id; user_id; created_utc; expires_utc; revoked_utc opcional | Dez códigos offline por lote; expiração em 12 meses UTC; entregar uma única vez após reautenticação; renovar revoga lote anterior |
| RecoveryCode | id; user_id; kind smtp/offline; batch_id opcional; code_hash; created_utc; expires_utc; consumed_utc/revoked_utc opcionais | Pelo menos 128 bits aleatórios; apenas hash persistido; SMTP expira em 15 minutos, offline usa a validade do lote; uso único |
| RecoveryAttempt | account_key_hash; origin_key_hash; occurred_utc | Contar falhas por identidade normalizada, inclusive inexistente, em janela móvel de 15 minutos; cinco falhas bloqueiam novas tentativas nessa janela, sem bloqueio permanente |

Senhas e códigos não são implementados com algoritmo próprio. SMTP é opcional; recuperação offline deve funcionar sem SaaS e exigir posse do código. Credenciais de operação ficam no arquivo externo e não em tabelas/artefatos do repositório.

Gerar 16 bytes com `RandomNumberGenerator` do framework, codificar em base64url e persistir hash SHA-256, sem inventar algoritmo criptográfico. Comparação de hash usa API de tempo constante do framework. Valores secretos não reaparecem em DTOs depois da entrega inicial. Solicitação SMTP invalida o código SMTP anterior sem revogar o lote offline; uso bem-sucedido revoga todas as sessões e os demais códigos, atomicamente com a troca de senha.

Cinco falhas por conta em janela móvel de 15 minutos, contabilizadas atomicamente também sob concorrência; origem tem teto operacional de 100 falhas em 15 minutos, proposto para reduzir abuso sem bloquear famílias em NAT com um teto de cinco por IP. Não armazenar email/IP plaintext nos logs. Contadores expirados são eliminados; tabela de tentativas usa partições mensais se persistida, com mecanismo existente. Falha, código expirado/consumido ou conta inexistente retorna erro genérico, sem revelar dados. Nenhuma renovação ou sucesso pode reativar código revogado por restaurar uma sessão antiga.

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

- **OperationReceipt**: actor_id, operation_key UUID, operation_name, scope_kind actor/garage, scope_id, garage_id opcional antes da criação, request_hash, created_utc, status pending/succeeded, resource_id e response pública mínima. Scope actor/actor_id para criar garagem sem garagem existente; demais criações usam garage/garage_id. Lock e lookup usam ator, contexto e chave antes da mutação; operation_name e conteúdo normalizado entram no hash, impedindo reuso da chave em outra operação. Criar recurso e recibo na mesma transação; após criação de garagem, vincular garage_id e reautorizar membership em todo replay. Conteúdo diferente retorna 409; resultado idêntico preserva status/id/version originais. Manter recibos enquanto existir a garagem, sem armazenar tokens de convite/sessão. Em partições, lock transacional e busca entre partições antes de criar; não presumir unicidade global. Cobrir garagens, veículos, convites, despesas, abastecimentos, manutenção, planos, compromissos, comprovantes e exportações.
- **AuditEvent**: id, garage_id opcional, actor_id pseudônimo, action, target_id, changed_fields sem conteúdo sensível, created_utc. Retenção 90 dias; partições mensais. Sem senhas, tokens, arquivos, payload integral ou valores pessoais em logs.
- **ExportJob**: id, garage_id, requesting_owner_id, state pending/running/ready/failed/expired, created_utc, expires_utc, arquivo/checksum. Worker interno na mesma aplicação, sem broker; download reautoriza owner. Exportação expira após 24 horas e não inclui hashes de senha, tokens, códigos ou credenciais operacionais.
- **GarageDeletion**: garage_id, confirmação do owner, requested_utc, deadline_utc=requested_utc+30 dias, completed_utc opcional e estado requested/running/completed/failed. Prazo inclui dados ativos, arquivos e todas as cópias gerenciadas contendo a garagem, inclusive anteriores ao pedido ou criadas durante processamento; criar cópia não reinicia deadline. Não remover dados de outra garagem.
- **BackupCatalog**: backup_id, created_utc, expires_utc, manifest_hash, location_key privada, garage_ids presentes, state creating/ready/expired/deleting/deleted e effective_deadline_utc. Inventário local na persistência já proposta, sem datastore novo; cada pedido antecipa o prazo das cópias correspondentes para no máximo requested_utc+30 dias. Registrar confirmação de remoção por cópia, sem publicar localização privada.
- **DeletionAuthority**: sequência monotônica, garage_id, requested_utc, deadline_utc e estado/completed_utc, mantidos como metadados operacionais mínimos sob acesso restrito. Não substituir essa autoridade por dados do dump restaurado. Fonte atual autenticada e coordenada ou exportação operacional independente verificada pelo operador, com sequência/instante/procedência e integridade; ausência de evidência de atualidade bloqueia a restauração. Não exige conta/serviço SaaS. Metadados não entram na exportação familiar nem em logs públicos; retenção acompanha as cópias gerenciadas que ainda poderiam conter a garagem.

## Partições, integridade e retenção

Despesas, medições/eventos históricos, recibos e auditoria têm partições mensais pela data declarada da entidade. Preparar doze meses anteriores e três futuros e manter default para dados fora da janela. Job diário prepara meses futuros; rollover é idempotente e testado.

Cada partição recebe índices por garagem/veículo/data e estado. Inserir registros fora da janela no default não pode falhar ou perder dados; criar uma partição exige mover dados correspondentes do default sob bloqueio/transaction antes de anexá-la. Não prometer unicidade ou FK global impossível entre partições.

Não remover partições financeiras por idade: o histórico é preservado até exclusão da garagem. Retenção de auditoria e arquivos temporários é independente. Drop de partição depende de ausência de vínculos/compatibilidade com retenção e evidência de backup/restauração.

BackupCatalog e DeletionAuthority pertencem ao controle operacional da persistência existente e não são substituídos pelo restore do domínio. Sua manutenção e cópia independente devem ser documentadas pelo operador. Antes de entregar uma cópia gerenciada, aplicar prazo mais restritivo do catálogo/garagens; não garantir eliminação de cópias particulares externas ao controle informado. Cópia disponível fora do prazo é incidente, não extensão automática de retenção.

## Autorização, concorrência e recuperação

Npgsql usa SQL parametrizado, transações e contexto de garagem. RLS é defesa adicional nas tabelas de domínio: role runtime sem superuser/BYPASSRLS, `SET LOCAL` dentro da transação e reset por término; migrations usam role separada. Export/worker só recebe contexto autorizado do job persistido, sem selecionar arbitrariamente outra garagem.

`If-Match` contém versão inteira; conflito retorna 409 e não sobrescreve. Chave de operação deve permanecer estável na repetição após falha de rede. Auditoria, despesa e vínculo são commitados juntos.

Backup operacional é separado da exportação familiar: contém banco e volume privados, verificáveis por manifesto e checksums, sem credenciais do arquivo externo. Pausar novas escritas, drenar operações de arquivo, copiar snapshot consistente e reabrir; manutenção retorna indisponibilidade recuperável, não sucesso falso. Restaurar em destino vazio, validar manifesto antes de trocar dados e falhar fechado se destino já existir.

Restaurar primeiro em destino isolado e não público. Verificar catálogo, integridade e estado atual de exclusões independente da cópia; selecionar todos os pedidos pertinentes, inclusive posteriores ao snapshot, reaplicar removendo dados/arquivos/recibos da garagem e só então promover. Sem origem atual autenticada ou evidência operacional verificável de atualidade, não promover. Hash do manifesto comprova integridade de bytes, não atualidade da política. O operador é a autoridade confiável da infraestrutura, sem promessa de proteção contra seu próprio administrador.

Preservar logs operacionais mínimos de verificação e remoção, sem conteúdo pessoal. Restaurar códigos/sessões antigos não pode reativá-los: invalidar sessões e códigos de recuperação vindos do snapshot antes de reabrir; usuários existentes recuperam acesso conforme procedimento operacional autorizado. Criar backup durante exclusão mantém o deadline original. Testar o pedido depois do snapshot, perda/corrupção da autoridade, recusa de destino existente e remoção de todas as cópias gerenciadas até o limite.

O estado da autoridade inclui `source_instance_id`, `coverage_from_utc`, `verified_at_utc` e última sequência. Sua cobertura deve começar antes ou na criação do snapshot e alcançar o ponto coordenado de promoção. Não concluir ausência de pedidos com histórico incompleto. Capturar sob quiescência da origem ou conferir novamente a sequência imediatamente antes de promover; avanço sem reaplicação exige repetir a verificação. Artefato fornecido pelo operador não é aceito apenas por existir: deve comprovar procedência, integridade, cobertura e atualidade no procedimento operacional autorizado.

## Cobertura

US1: identidade/garagem/membros. US2: despesas/totais. US3: abastecimento/recibos. US4: manutenção/compromissos. US5: comprovantes. US6: exportação, recuperação e matriz operacional. As relações acima cobrem FR-001–FR-030; desempenho, Android e acessibilidade serão verificados conforme o plano e quickstart.