# Regras do Contrato HTTP e Mobile

**Origem**: IA assistida. **Revisor humano**: CEO da Altivy e owner do GarageMotor.
**Estado**: contrato proposto da Fase 1, sem endpoints implementados.
**Contrato de máquina**: [openapi.json](openapi.json). **Tipos e limites**: [modelo de dados](../data-model.md).

## Protocolo e respostas

Base `/api/v1`; JSON camelCase, UUID, datas ISO, instantes UTC, dinheiro em centavos BRL e volume em mililitros. Indicadores decimais são strings com ponto, apresentados em pt-BR pelo cliente. API não aceita formato de moeda localizado como número.

`Resource` tem id, version, kind e attributes. Campos attributes são os campos públicos camelCase da entidade correspondente do modelo: recursos vehicle/expense/fuel/maintenance/plan/commitment incluem dados de entrada e estado/vínculos públicos; garage inclui nome/fuso/estado; member inclui identidade pública/papel/estado; invite inclui destinatário/papel/expiração/estado; attachment inclui nome original, tipo, tamanho, hash e vínculo; export inclui estado/expiração e nunca caminho real. Reminder é projeção de referência, título, limite, última leitura e estado, não entidade de predição. Nenhum DTO contém hash de senha, token de sessão, código de recuperação, storageKey ou credencial de serviço.

Coleções usam items/nextCursor e limite padrão 50, máximo 100. Ordenação é determinística por data/id; cursor opaco é validado e não permite navegar para outra garagem. Filtros temporais incluem as duas datas; `from > to` é erro de entrada. GET nunca exige If-Match; PUT substitui o conjunto de campos editáveis da entidade e exige versão.

Falhas usam application/problem+json: 400 entrada, 401 sessão, 403 papel insuficiente dentro da própria garagem, 404 recurso inexistente ou de outra garagem, 409 versão/idempotência/conflito, 413 limite, 415 tipo, 422 regra de domínio, 429 rate limit e 503 manutenção. Resposta nunca expõe SQL, filesystem, stacktrace ou existência de contas em recuperação. TraceId é correlacionável sem conteúdo sensível.

## Identidade, web e Android

Contas locais, PasswordHasher/Identity e rate limiting do framework; sem algoritmo criptográfico próprio. Cadastro/solicitação de recuperação devolvem resposta genérica. Recuperação exige código de uso único; acesso offline usa códigos já entregues ao usuário após reautenticação, não bypass administrativo público. Falha e sucesso da confirmação têm mensagem genérica; senha só muda após validação.

Web usa cookie `__Host-gm-session`, Secure, HttpOnly, Path=/ e SameSite restritivo; token de sessão não é devolvido ao JavaScript web. Antiforgery do framework em mutações por cookie com X-CSRF-Token, origem validada e CORS restrito; endpoint csrf fornece o requestToken correspondente. Persistência de chaves do framework usa provider padrão com proteção por credencial externa e material protegido em volume privado, nunca segredo plaintext no repositório.

Android usa sessão opaca bearer, sem token em URL, logs ou localStorage. Persistência, se necessária, usa Android Keystore/APIs da plataforma, sem novo plugin não auditado; JS mantém somente estado necessário em memória. Expiração de sessão em 12 horas e revogação imediata. A API verifica membership a cada operação e reautoriza replay idempotente.

Escolher instância própria altera somente destino do cliente, não faz o servidor buscar URL arbitrária. Exigir HTTPS, identidade e major compatível em `/instance`; certificados inválidos/incompatibilidade são erro sem bypass. LAN exige TLS confiável e eventual VPN. Trocar instância encerra sessão anterior, limpa estado de dados e solicita autenticação nova. Conexão perdida não gera sucesso falso; repetir a mesma chave/payload após reconectar.

## Autorização e mutações

Owner gerencia membros, propriedade, exclusão e exportações; editor cria/corrige dados financeiros, planos e comprovantes; reader apenas consulta. Nenhuma garagem atravessa essas fronteiras. Estados active/archived/deleting são verificados antes de escrita.

Despesas, abastecimentos, conclusão de compromissos e uploads usam Idempotency-Key UUID. Chave/corpo idênticos retornam a operação anterior, após reautorizar o ator. Chave reutilizada com conteúdo diferente retorna 409. Recibos duram enquanto existir a garagem; partições não justificam perder idempotência após rollover.

ETag é versão inteira entre aspas; If-Match ausente na mutação exigida é 400, divergente é 409. Anular despesa/abastecimento é tombstone financeiro, não remoção física. Registro, vínculo de despesa e auditoria alteram juntos. Custo de manutenção/abastecimento não entra duas vezes nos totais.

## Comprovantes e exportação

Multipart: arquivo PDF/JPEG/PNG de até 10 MiB, até cinco por despesa/manutenção. Validar tipo/conteúdo/tamanho e pai da garagem antes de confirmar; staging não é público. Em arquivos ligados a despesas particionadas, resolver id e data no contexto da garagem; nunca confiar em path enviado pelo cliente.

Download exige sessão e vínculo autorizado em cada acesso, attachment, nosniff e nome codificado pelo framework. Verificação de tipo não é promessa de antivírus ou segurança ao abrir arquivo no dispositivo. Auditoria/logs não registram conteúdo. Remoção marca deleting e coleta após checar referências.

Exportação familiar é job interno e inclui dados e arquivos autorizados da garagem com manifesto/checksums; não inclui identidade secreta, credenciais, chaves, códigos ou tokens. Expira em 24 horas; download exige owner ainda ativo. Backup operacional não é endpoint remoto que executa shell: ocorre por ferramenta de administração em containers, conforme quickstart.

## Disponibilidade e compatibilidade

Durante backup/restore/manutenção, bloquear novas escritas com 503 e Retry-After, drenar gravações em curso e não confirmar operação incompleta. Restauração exige destino vazio. Dados e direitos do código não são bloqueados por término de serviço de suporte.

Mudanças de API/schema seguem expand-contract; cliente incompatível não escreve. Android API 35/36 e navegadores Chromium/Firefox/WebKit serão a matriz inicial testada, sem promessa de toda versão antiga. Sem escrita offline, push, IA ou iOS nesta feature.

## Verificação do contrato

Testar isolamento e papéis, origem/CSRF, enumeração, sessão/códigos, idempotência no rollover, conflitos, precisão, arquivo malicioso, download após revogação e restauração. Validar schemas/DTOs por operationId e integração real. Validação de sintaxe e referências OpenAPI nesta fase não substitui teste de implementação.