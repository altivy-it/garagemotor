# Revisão de Segurança do GarageMotor

**Estado**: revisão inicial dos controles propostos; validação de implementação e aprovação humana pendentes.
**Data**: 2026-11-09
**Origem**: IA assistida.
**Revisor humano**: CEO da Altivy e owner do GarageMotor.
**Escopo**: [spec](../specs/001-garagem-pessoal/spec.md), [plano](../specs/001-garagem-pessoal/plan.md), [modelo de dados](../specs/001-garagem-pessoal/data-model.md), [contrato HTTP](../specs/001-garagem-pessoal/contracts/api.md) e [ADR de persistência](adr/2026-0001-persistencia-garagem.md).

Esta revisão registra ameaças, controles requeridos e evidências a obter durante a implementação. Não declara segurança validada, conformidade jurídica, teste executado ou aceite humano dos controles. Nenhuma credencial, dado pessoal ou segredo foi usado como evidência.

## Superfícies e Controles

| Superfície | Risco | Controle proposto | Evidência necessária |
|---|---|---|---|
| Isolamento entre garagens | Acesso direto ou consulta lateral a dados de outra garagem | Membership em cada operação, contexto transacional, RLS como defesa adicional e role runtime sem BYPASSRLS | Testes de integração PostgreSQL com duas garagens, papéis distintos e conexões reutilizadas |
| Sessão e recuperação | Enumeração, roubo/reuso de token ou abuso de recuperação | Sessão opaca com hash persistido, revogação imediata, respostas genéricas, tokens/códigos de uso único e limites concorrentes | Integração real para origem/CSRF, expiração, revogação, recuperação e concorrência |
| Idempotência e finanças | Repetição de requisição cria despesa/recarga duplicada | Idempotency-Key UUID, OperationReceipt transacional, replay reautorizado e If-Match para mutações | Testes de repetição/conflito em falha de rede, concorrência e rollover de partição |
| Recarga elétrica | Tarifa futura altera custo histórico, origem incorreta da despesa ou eficiência enganosa | Snapshot da tarifa vigente por sessão, uma despesa vinculada, Wh inteiro e cálculo somente cheia-a-cheia | Testes de troca de tarifa, recarga pública/doméstica, custo único, odômetro e sessões parciais |
| Busca | Injeção, fuga de dados ou indexação de identificadores/valores/documentos | SQL parametrizado, escopo de garagem, auditoria owner-only e campos permitidos de FR-031 | Testes de integração e contrato para autorização, campos excluídos, cursor e resposta fechada |
| Arquivos | Travessia de caminho, conteúdo em garagem errada ou download não autorizado | Chave opaca gerada pelo servidor, staging privado, limites, autorização por acesso, attachment/nosniff | Testes com filesystem/volume real, falhas parciais, conteúdo e acesso cruzado |
| Backup, restore e exclusão | Reintroduzir dados excluídos ou afetar outras garagens | Backup consistente, DeletionAuthority independente, revalidação antes de promoção e preservação de deadline | Testes de restore isolado, autoridade ausente/stale, cópias múltiplas e prazo de exclusão |
| Rede/Android | TLS inválido, troca de instância com sessão antiga ou gravação falsa | HTTPS sem bypass, sessão limpa ao trocar destino, token fora de logs/URLs e Keystore da plataforma | Testes na matriz Android API35/36 pela borda nginx |

## Gates

- O ADR de PostgreSQL foi aceito pelo owner em 2026-11-09; isso aprova a escolha de datastore, não os controles de segurança ou o rollout.
- Revisão humana das superfícies privilegiadas permanece necessária antes de promoção.
- Testes de PostgreSQL/filesystem, Playwright, Android e restore ainda não foram executados nesta etapa documental.
- Recarga pública sem cobrança é representada pelo mínimo positivo de R$ 0,01, conforme decisão humana de 2026-11-09; o sistema não admite despesa de valor zero.
- O teto de 100 falhas por origem/15 minutos continua proposta técnica configurável, não valor confirmado pelo owner.

## Resultado

Controles especificados; efetividade não verificada. Registrar evidências e decisões de segurança após implementação autorizada, sem declarar aprovação até revisão humana explícita.

## Evidência Local do Recorte Web

**Atualização**: 2026-11-11, IA assistida; revisor humano indicado acima. A continuidade autorizada da T067 permitiu bootstrap/integrar a stack local, não aprovou os controles de segurança para produção. O resultado inicial acima permanece histórico.

| Superfície local | Ameaça | Controle implementado e evidência | Limite |
|---|---|---|---|
| Proxy e TLS | Headers forjados ou acesso sem validação do certificado | API confia em um único IP de gateway, um salto; apenas nginx publica a borda. Discovery, frontend e API responderam 200 por TLS com cadeia/hostname validados | Confiança/renovação do certificado no host e qualificação de produção pendentes |
| Sessão web | Roubo de bearer, CSRF ou logout sem proteção | Duas jornadas reais verificaram ausência de token no JSON web/localStorage, cookie Secure/HttpOnly/Strict, DELETE sem CSRF recusado e logout com CSRF 204 | Concorrência, recuperação completa e restauração do frontend após reload não revalidadas |
| Provisionamento | Credenciais administrativas expostas ou sobrescrita de estado prévio | Criação exclusiva externa 0600/0700; SQL por stdin em processo protegido; roles próprias sem superuser/BYPASSRLS; recusas para destinos existentes | Operação única privilegiada requer revisão humana; falha parcial exige recuperação manual sem apagar dados |
| Key ring | Chaves em claro ou perdidas na recriação | Volume privado no UID 1654, chaves cifradas por PKCS12 externo; ciclo parar/iniciar preservou o volume | Rotação, backup/restore e validade do certificado local de um ano precisam de procedimento validado |
| Runner | Senhas/códigos vazarem por artefatos | Credenciais fictícias geradas no runner sem arquivo privado montado; trace/vídeo/capturas desativados e página limpa antes de diagnóstico final | Manter essas restrições em novos testes; não habilitar debug/trace em sessão real |

Perfis oficiais `quality` e `web-e2e` passaram; este último inclui quatro contratos estáticos e duas jornadas reais. Isso não substitui threat model/revisão humana para promoção, testes completos de isolamento/restore ou Android. Instruções operacionais e limites em [instalação local](install.md).
