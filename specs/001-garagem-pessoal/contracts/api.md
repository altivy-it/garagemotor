# Regras do Contrato HTTP e Mobile

**Origem**: IA assistida. **Revisor humano**: CEO da Altivy e owner do GarageMotor.
**Estado**: contrato proposto da Fase 1, sem endpoints implementados.
**Contrato de máquina**: [openapi.json](openapi.json). **Tipos e limites**: [modelo de dados](../data-model.md).

**Revisão**: 2026-11-09, alinhada à sessão de esclarecimento; sem endpoints implementados.

## Protocolo e respostas

Base `/api/v1`; JSON camelCase, UUID, datas ISO, instantes UTC, dinheiro em centavos BRL e volume em mililitros. Indicadores decimais são strings com ponto, apresentados em pt-BR pelo cliente. API não aceita formato de moeda localizado como número.

`Resource` tem id, version, kind e attributes fechados pelos [schemas públicos](resources.schema.json), com `oneOf` por kind e `additionalProperties: false` nos envelopes e atributos. Nenhum recurso de domínio contém hashes de senha/token/código, códigos de recuperação, storageKey ou credenciais. Campos opcionais de saída definidos como nullable são presentes com null, não objetos arbitrários. Session e OfflineRecoveryBatch são respostas excepcionais de autenticação, não Resource ou dados exportáveis; seus valores secretos não são logados/cacheados.

Mapeamento obrigatório por operação: garagens/ownership -> GarageResource; members -> MemberResource; invites/acceptInvite -> InviteResource; vehicles -> VehicleResource; expenses -> ExpenseResource; fuel-records -> FuelResource; `charging-sessions` -> ElectricChargeResource; `electricity-tariffs` -> ElectricityTariffResource; maintenance -> MaintenanceResource; plans -> PlanResource; commitments/completion -> CommitmentResource; reminders -> ReminderResource; uploads -> AttachmentResource; exports/status -> ExportResource; `searchGarageRecords` -> Resource tipado por kind; `searchGarageAudit` -> AuditSearchItem fechado e owner-only. Coleções usam o mesmo tipo nos items; download retorna binário. Testes de contrato devem rejeitar kind trocado, campo extra/privado ou tipo incorreto, mesmo que o union aceite outro tipo em operação diferente. A API deve declarar o tipo específico ao publicar sua descrição, sem relaxar os schemas do desenho.

Coleções usam items/nextCursor e limite padrão 50, máximo 100. Ordenação é determinística por data/id; cursor opaco é validado e não permite navegar para outra garagem. Filtros temporais incluem as duas datas; `from > to` é erro de entrada. GET nunca exige If-Match; PUT substitui o conjunto de campos editáveis da entidade e exige versão.

## Pesquisa textual

`GET /garages/{garageId}/search` (`searchGarageRecords`) pesquisa somente nome da garagem; label/marca/modelo de veículo; descrição de despesa/manutenção; posto de abastecimento; e título de plano/compromisso. Qualquer membro ativo pode consultar, mas resultados e cursor ficam limitados ao `garageId` e à membership revalidada. Retorna coleção paginada de `Resource`, em ordem de relevância e desempate determinístico por tipo/id.

`GET /garages/{garageId}/audit/search` (`searchGarageAudit`) exige owner ativo e consulta somente os 90 dias retidos da própria garagem. Retorna `AuditSearchItem` fechado: id, instante UTC, ação, ator pseudonimizado, alvo e nomes dos campos alterados, sem valores. A resposta não inclui payload, detalhes livres, e-mail, IP, credenciais, tokens ou conteúdo de comprovantes.

Ambas as operações exigem `q` com 1–200 caracteres, aceitam limite padrão 50/máximo 100 e cursor opaco vinculado à consulta/garagem. Busca usa full-text nativo PostgreSQL em português, sem extensão; normaliza caixa/acentos e aceita sintaxe web: termos, frases entre aspas, `OR` e exclusão por `-`. Placa, Renavam, chassi, valores, nomes/bytes de arquivos, secrets e credenciais não são indexados nem pesquisáveis. Uma consulta vazia após normalização retorna erro de entrada.

## Recargas elétricas

`GET/POST /garages/{garageId}/charging-sessions` lista e cria sessões; `GET/PUT/DELETE /garages/{garageId}/charging-sessions/{id}` consulta, corrige com `If-Match` e anula a sessão. O veículo deve estar ativo e ter combustível `electric`. Cada criação exige `Idempotency-Key`; gravação/correção/anulação da sessão e despesa vinculada é transacional. Owner/editor escrevem; reader consulta.

Entrada doméstica declara `source=home` e não envia custo: a API usa a tarifa BRL/kWh vigente na data da sessão e salva o snapshot usado. Entrada pública declara `source=public` e exige `amountMinor` positivo informado pelo usuário; quando não houve cobrança, o usuário pode informar o mínimo de 1 centavo (R$ 0,01), nunca zero. `GET/POST /garages/{garageId}/electricity-tariffs` consulta histórico e cria nova vigência com `Idempotency-Key`; apenas owner/editor podem criá-la. Uma tarifa nova nunca altera o custo de sessões anteriores.

Energia é enviada em Wh inteiro e apresentada em kWh. `fullAtEnd` marca bateria cheia no fim da sessão. km/kWh e custo/km só são completos entre dois marcos cheios consecutivos; somar as sessões posteriores ao marco anterior, incluindo as parciais até o marco final. Sem intervalo válido, retornar cobertura parcial/indisponível e não número completo.

Falhas usam application/problem+json: 400 entrada, 401 sessão, 403 papel insuficiente dentro da própria garagem, 404 recurso inexistente ou de outra garagem, 409 versão/idempotência/conflito, 413 limite, 415 tipo, 422 regra de domínio, 429 rate limit e 503 manutenção. Resposta nunca expõe SQL, filesystem, stacktrace ou existência de contas em recuperação. TraceId é correlacionável sem conteúdo sensível.

## Identidade, web e Android

Contas locais, PasswordHasher/Identity e primitivas criptográficas/rate limiting do framework; sem algoritmo próprio. Cadastro/solicitação de recuperação devolvem resposta genérica. Códigos têm pelo menos 128 bits (16 bytes aleatórios codificados em base64url, 22 caracteres), somente hash persistido e comparação de tempo constante. SMTP expira em 15 minutos; lote offline tem dez códigos únicos e expira em 12 meses. Entregar OfflineRecoveryBatch uma única vez após reautenticação, com expiresUtc explícito, Cache-Control: no-store e sem logs de conteúdo.

Renovar invalida o lote anterior; código SMTP novo invalida o SMTP anterior sem destruir códigos offline válidos. Sucesso de recuperação troca senha, consome o código e revoga todas as sessões e demais códigos na mesma transação. Expirado, consumido, revogado ou conta inexistente são recusados genericamente; senha não muda em falha. A resposta 204 confirma alteração válida; 422 recusa genericamente e 429 limita taxa, sem detalhar se a conta existe.

Contar atomicamente até cinco falhas por identidade em janela móvel de 15 minutos, inclusive sob concorrência; não criar bloqueio permanente. Teto operacional por origem: 100 falhas em 15 minutos, conforme decisão técnica documentada no modelo, com identidade/origem protegidas e sem IP/email plaintext nos logs. Não resetar falhas por uma nova solicitação de recuperação. Limites aplicam-se a conta inexistente e códigos SMTP/offline, sem bypass público ou dependência de SaaS.

Web usa cookie `__Host-gm-session`, Secure, HttpOnly, Path=/ e SameSite restritivo; token de sessão não é devolvido ao JavaScript web. Antiforgery do framework em mutações por cookie com X-CSRF-Token, origem validada e CORS restrito; endpoint csrf fornece o requestToken correspondente. Persistência de chaves do framework usa provider padrão com proteção por credencial externa e material protegido em volume privado, nunca segredo plaintext no repositório.

Android usa sessão opaca bearer, sem token em URL, logs ou localStorage. Persistência, se necessária, usa Android Keystore/APIs da plataforma, sem novo plugin não auditado; JS mantém somente estado necessário em memória. Expiração de sessão em 12 horas e revogação imediata. A API verifica membership a cada operação e reautoriza replay idempotente.

Escolher instância própria altera somente destino do cliente, não faz o servidor buscar URL arbitrária. Exigir HTTPS, identidade e major compatível em `/instance`; certificados inválidos/incompatibilidade são erro sem bypass. LAN exige TLS confiável e eventual VPN. Trocar instância encerra sessão anterior, limpa estado de dados e solicita autenticação nova. Conexão perdida não gera sucesso falso; repetir a mesma chave/payload após reconectar.

## Autorização e mutações

Owner gerencia membros, propriedade, exclusão e exportações; editor cria/corrige dados financeiros, planos e comprovantes; reader apenas consulta. Nenhuma garagem atravessa essas fronteiras. Estados active/archived/deleting são verificados antes de escrita.

Criações de garagem, veículo, convite, despesa, abastecimento, recarga elétrica, versão de tarifa elétrica, manutenção, plano, compromisso, comprovante e exportação exigem Idempotency-Key UUID; conclusão de compromisso também mantém a chave. Cadastro, sessão e lote de recuperação têm regras próprias, não esse mecanismo de domínio. Chave, ator e contexto identificam a operação; operationId e conteúdo normalizado entram no hash. Chave reutilizada com conteúdo ou operação diferente retorna 409 antes de mutar. Recibo e recurso são commitados atomicamente, e replay preserva status/id/version originais após reautorizar acesso.

Criar garagem usa contexto actor/actorId antes de existir garageId; o recibo passa a apontar para a garagem criada e membership é revalidada em todo replay. Outras criações usam contexto garage/garageId. Não recuperar resposta pelo hash sem contexto/ator. Recibos duram enquanto existir a garagem; partições não justificam perder idempotência após rollover. Recibo não armazena tokens de convite ou autenticação; entrega de convite não se duplica com repetição da criação.

ETag é versão inteira entre aspas; If-Match ausente na mutação exigida é 400, divergente é 409. Anular despesa/abastecimento/recarga é tombstone financeiro, não remoção física. Registro, vínculo de despesa e auditoria alteram juntos. Custo de manutenção/abastecimento/recarga não entra duas vezes nos totais.

## Comprovantes e exportação

Multipart: arquivo PDF/JPEG/PNG de até 10 MiB, até cinco por despesa/manutenção. Validar tipo/conteúdo/tamanho e pai da garagem antes de confirmar; staging não é público. Em arquivos ligados a despesas particionadas, resolver id e data no contexto da garagem; nunca confiar em path enviado pelo cliente.

Download exige sessão e vínculo autorizado em cada acesso, attachment, nosniff e nome codificado pelo framework. Verificação de tipo não é promessa de antivírus ou segurança ao abrir arquivo no dispositivo. Auditoria/logs não registram conteúdo. Remoção marca deleting e coleta após checar referências.

Exportação familiar é job interno e inclui dados e arquivos autorizados da garagem com manifesto/checksums; não inclui identidade secreta, credenciais, chaves, códigos, tokens ou catálogo operacional. Expira em 24 horas; download exige owner ainda ativo. Backup operacional não é endpoint que executa shell: ocorre por ferramenta de administração em containers, conforme quickstart.

Pedido de exclusão define deadline=requestedUtc+30 dias para dados ativos, comprovantes e cada backup gerenciado contendo a garagem, inclusive cópias anteriores ao pedido ou criadas durante processamento. Catálogo aplica a data-limite mais restritiva e registra remoção real; nova cópia não reinicia prazo. Operador é responsável por suas cópias; não prometer eliminação de cópias particulares externas à gestão.

## Disponibilidade e compatibilidade

Durante backup/restore/manutenção, bloquear novas escritas com 503 e Retry-After, drenar gravações em curso e não confirmar operação incompleta. Restauração exige destino vazio. Dados e direitos do código não são bloqueados por término de serviço de suporte.

Restauração exige estado atual e verificável de exclusões independente do snapshot. Consultar autoridade atual autenticada ou artefato operacional separado cuja atualidade/procedência seja verificável pelo operador; checksum de arquivo antigo não é essa evidência. Reaplicar todos os pedidos pertinentes, inclusive posteriores ao backup, remover dados/arquivos/recibos antes da promoção e impedir reativação de sessões/códigos antigos. Autoridade ausente, perdida, corrompida ou não atual bloqueia a promoção; sem serviço SaaS obrigatório. Não sobrescrever catálogo/autoridade de controle com tabelas antigas do dump.

Mudanças de API/schema seguem expand-contract; cliente incompatível não escreve. Android API 35/36 e navegadores Chromium/Firefox/WebKit serão a matriz inicial testada, sem promessa de toda versão antiga. Sem escrita offline, push, IA ou iOS nesta feature.

## Verificação do contrato

Testar isolamento e papéis, origem/CSRF, enumeração, sessão/códigos, idempotência no rollover, conflitos, precisão, arquivo malicioso, download após revogação e restauração. Validar schemas/DTOs por operationId e integração real. Validação de sintaxe e referências OpenAPI nesta fase não substitui teste de implementação.
