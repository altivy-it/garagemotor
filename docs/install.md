# Instalação Local — Recorte Web

**Origem**: IA assistida. **Revisor humano**: CEO da Altivy e owner do GarageMotor.  
**Escopo autorizado**: recorte web da T067 da [feature 001](../specs/001-garagem-pessoal/tasks.md), confirmado nesta conversa em 2026-11-11.  
**Estado atual**: integração web local validada em Linux amd64. API/web usam PostgreSQL e nginx da Identity compartilhada; cadastro, login, CSRF, temas, códigos offline e logout passaram em desktop/móvel por HTTPS com certificado validado. Perfis oficiais `quality` e `web-e2e` passaram. Instalação limpa completa, Android e pacote de distribuição permanecem pendentes. Não há autorização de push, release ou deploy público. As seções de diagnóstico/bootstrap abaixo preservam os resultados históricos; o estado atual está em **Integração GarageMotor Validada**.

## Pré-requisitos

- Exclusivamente Podman rootless, provider Compose e PowerShell no host para orquestração, por decisão humana de 2026-11-11. Build, testes e serviços executam em containers. Os três launchers/perfis rejeitam Docker e verificam o modo rootless; não usar sudo, rootful ou alias Docker como alternativa.
- Imagens e dependências fixadas pelos locks do repositório. O launcher injeta as referências do [lock de imagens](../deploy/images.lock.json), inclusive as bases de build.
- PostgreSQL e nginx compartilhados já disponíveis no mesmo contexto da engine. Os exemplos de Dokpod/architecture-knowledge-base usam redes externas `identity-global` para o gateway e `identity-client` para clientes do banco; confirmar os nomes reais antes de iniciar. O GarageMotor não cria essas redes nem gerencia os containers compartilhados.
- Acesso à aplicação somente pelo nginx existente; o Compose do GarageMotor não publica portas e não declara serviço de banco, gateway ou volume PostgreSQL. Os aliases dos upstreams são `garagemotor-api:8080` e `garagemotor-web:4200`, sem usar os aliases genéricos de outros produtos.
- Banco/namespace próprio do GarageMotor preparado dentro da instância existente com a [migration de identidade/operações](../backend/GarageMotor.Api/Migrations/001_identity_and_operations.sql), roles e credenciais externas. Não reutilizar tabelas, credenciais ou banco de outro produto sem decisão explícita. O startup atual da API não aplica migrations automaticamente.

## Configuração Privada

O arquivo obrigatório fica exclusivamente fora do repositório. Após autorização do owner, o provisionamento criou a pasta Linux com modo 0700 e o arquivo com modo 0600, por criação exclusiva e valores aleatórios gerados no processo:

- Linux/macOS: `~/.microsoft/usersecrets/GarageMotor/.env`.
- Windows: `$env:APPDATA\Microsoft\UserSecrets\GarageMotor\.env`.

Não criar `.env` no repositório, não usar `dotnet user-secrets` e não enviar conteúdo, senhas ou connection strings pela conversa. O arquivo foi escrito pelo processo autorizado, sem exibir seus valores e sem leitura posterior pelo agente. Qualquer provisionamento posterior deve falhar fechado se o destino já existir; não sobrescrever configuração privada existente.

| Nome de configuração consumido pelo Compose | Finalidade |
|---|---|
| `GARAGEMOTOR_PUBLIC_ORIGIN` | Origem HTTP(S) pública já configurada no gateway compartilhado; confirmar certificado/TLS, sem bypass |
| `GARAGEMOTOR_CONNECTION_STRING` | Conexão da API com a role `garagemotor_runtime` |
| `GARAGEMOTOR_MIGRATIONS_CONNECTION_STRING` | Conexão separada com a role `garagemotor_migrations` |
| `GARAGEMOTOR_DATA_PROTECTION_CERTIFICATE_BASE64` | PKCS12 externo para cifrar o key ring; não é certificado TLS nem chave Android |
| `GARAGEMOTOR_DATA_PROTECTION_CERTIFICATE_PASSWORD` | Proteção do PKCS12 externo |

As conexões usam o DNS/alias confirmado da instância PostgreSQL compartilhada, não `localhost` do container. As roles runtime/migrations foram criadas separadamente e verificadas sem superuser ou `BYPASSRLS`; a API não utiliza a credencial administrativa. O provisionador acessa as credenciais administrativas apenas dentro do processo PostgreSQL existente, enviando SQL por stdin com saída protegida. Não ler ou copiar arquivos privados de Altivy.Identity/Dokpod para montar essa configuração.

O launcher e os perfis de serviço normalizam e rejeitam caminhos diferentes do local aprovado antes de testar/usar o arquivo. O Compose recebe apenas seu caminho; o agente não deve ler, copiar ou imprimir seu conteúdo. Credenciais nunca são argumentos de build e não integram as imagens.

## Inicialização

Após configuração externa e banco preparados, na raiz do repositório:

```powershell
pwsh -NoProfile -File tools/scripts/start-local.ps1 -Engine podman -GatewayNetwork identity-global -DatabaseNetwork identity-client
```

Resultado validado: Compose constrói os estágios `development` e recria API/web nas redes existentes; não inicia nginx ou PostgreSQL e não publica portas. Acesse `https://localhost:7443/garagemotor/`. O parâmetro legado `HttpPort` não altera a porta do gateway. O launcher verifica redes e identifica o IP do nginx antes de fornecer o caminho privado ao provider; a API confia somente nesse proxy, com um salto de headers. A recriação explícita evita que Podman Compose 1.0.6 mantenha uma imagem antiga após o build.

A rota foi instalada no adaptador público da Identity após `nginx -t`, preservando o servidor e as rotas Identity/notes/Dokpod. Não sobrescrever configuração global nem recarregar o gateway sem revisão e validação. Nenhum arquivo dos repositórios de referência foi alterado.

O projeto Angular usa allowlist de hosts em `angular.json`; não passar `--allowed-hosts nginx` no CLI, pois essa versão interpreta a flag ambígua como booleano. Não liberar todos os hosts para contornar erros.

Para encerrar sem apagar os volumes:

```powershell
pwsh -NoProfile -File tools/scripts/stop-local.ps1 -Engine podman
```

A parada usa somente o projeto Compose GarageMotor, sem ler o arquivo privado, sem `--remove-orphans` e sem gerenciar as redes externas. Não remover volumes como tentativa de reparar autenticação, schema ou credenciais. O preview isolado da feature 003 não equivale a essa instalação integrada e continua sem API funcional.

## Imagens e Evidências

- [Dockerfile da API](../deploy/Dockerfile.api): estágios `development`, `publish` e `production`, restore NuGet travado e runtime UID 1654. O pacote mantém `openapi.json` e `resources.schema.json` juntos em `/app/contracts`, além de migrations e avisos.
- [Dockerfile web](../deploy/Dockerfile.web): instalação npm pelo lock e sem lifecycle scripts, desenvolvimento como `node`, build Angular e runtime estático nginx UID 101. O fixture de testes não integra o bundle de produção.
- [Configuração do servidor estático](../deploy/nginx.web.conf): porta interna 4200, fallback da SPA e caminhos temporários em `/tmp` para execução sem root.
- As quatro imagens/targets API e web de desenvolvimento/runtime foram construídas. A API, sem configuração, alcançou o guard de credenciais externas e recusou iniciar; isso não comprova integração com PostgreSQL.
- Smoke HTTP pelo proxy nginx temporário: 6 testes existentes de marca/galeria passaram no pacote web de produção e 6 no desenvolvimento. O backend não foi simulado como funcional; `/api/` retornou explicitamente 503.
- Sintaxe PowerShell e interpolação Compose passaram sem carregar configuração privada. `quality` oficial passou. Os containers temporários de smoke foram removidos; preview, banco e volumes preexistentes foram preservados.
- ARM64 não foi executado; o launcher resolve os argumentos por plataforma pelo lock. Não inferir qualificação ARM pelos testes amd64.

## Histórico dos Gates Integrados

Na raiz, depois das pré-condições privadas:

```powershell
pwsh -NoProfile -File tools/scripts/validate-feature.ps1 -FeatureDirectory specs/001-garagem-pessoal -Profile integration -Engine podman
pwsh -NoProfile -File tools/scripts/validate-feature.ps1 -FeatureDirectory specs/003-garagemotor-design-system -Profile web-e2e -Engine podman
```

Estado observado inicialmente em 2026-11-11: `web-e2e` retornou `blocked`, exit 2, por arquivo externo ausente, sem ler seu conteúdo; caminho alternativo também bloqueou. Após a restrição de reutilização compartilhada, o preflight direto do launcher recusou iniciar por ausência de `identity-global`; a listagem também não contém `identity-client`, a CLI Docker não está instalada e não há outros contextos Podman registrados. A ausência dos Dockerfiles já não é o bloqueio. Não usar os bancos/previews de teste encontrados como substitutos automáticos da infraestrutura indicada.

Cadastro/login reais, cookies/CSRF/origem pelo proxy, migrations, recuperação e sessão persistida ainda precisam de validação com banco real. Depois de confirmar o contexto e a origem pública, alinhar os runners integrados às redes/URL compartilhadas antes de executar; a configuração anterior `garagemotor_application`/`http://nginx:8080` não representa a plataforma compartilhada. Os testes do fixture isolado e de API deliberadamente indisponível não substituem essas jornadas integradas. Android, assinatura, integração de vault/GitHub e o restante da T067 permanecem fora deste recorte.

## Diagnóstico e Recuperação

Se o preflight indicar configuração ausente, verificar somente existência/permissões do caminho externo aprovado; não imprimir valores ou logs integrais. Se a API recusar conexão, conferir por processo seguro se o banco/roles/migration foram preparados, sem trocar para credenciais administrativas como atalho. Interromper serviços sem apagar volumes e encaminhar ao operador responsável quando credenciais ou estado prévio do banco não forem conhecidos. Não reemitir chaves, sobrescrever configuração ou aceitar termos adicionais automaticamente.

### Bootstrap da Plataforma Identity

O owner apontou a plataforma de `altivy-it/architecture-knowledge-base/platforms/identity` e exigiu Podman rootless. A engine foi confirmada com `podman info --format '{{.Host.Security.Rootless}}'`, resultado `true`. A cópia local documenta o gateway TLS compartilhado em `https://localhost:7443`, redes `identity-global`/`identity-client` e mounts existentes para PostgreSQL, certificados e temas. Essa origem não representa uma rota GarageMotor já configurada.

O arquivo externo `~/.microsoft/usersecrets/Altivy.Identity/.env` existe; nenhum conteúdo foi lido diretamente, copiado ou apresentado ao modelo. O Compose recebeu somente seu caminho para a própria validação por processo seguro, com toda a saída capturada; somente status, nomes de serviços esperados e flags de diagnóstico não secretas foram emitidos. As imagens corporativas PostgreSQL 18.4/nginx 1.28 já estão no armazenamento local; isso não autoriza trocar versões nem iniciar outro cluster.

O provider instalado é Podman Compose 1.0.6 e não suporta `config --quiet`; não classificar esse retorno como erro de configuração privada. O comando suportado `config --services`, com saída suprimida/filtrada, retornou exit 1. Flags de diagnóstico confirmaram parsing de mounts e presença de sintaxe de drive Windows no erro, sem mostrar caminhos ou valores. A configuração precisa apontar para os recursos Linux existentes antes de iniciar. Não converter caminhos por suposição nem criar diretório de dados vazio, certificado novo ou cluster paralelo.

É necessário confirmar apenas metadados não secretos: caminho Linux do bind mount PostgreSQL existente, arquivos TLS já utilizados e diretório de temas. Não enviar `.env`, chaves, senhas ou tokens pela conversa. Os scripts locais da plataforma ainda chamam Docker e usam `%APPDATA%`; não foram executados nem alterados. O remoto público não pôde ser lido (404) e a ferramenta Git autenticada solicitou login, portanto a versão remota atual não foi confirmada. Nenhum container, rede ou dado da Identity foi criado ou modificado nesta etapa; a conexão e o cadastro/login continuam pendentes.

### Primeiro Start Autorizado Nesta Máquina

Posteriormente, o owner esclareceu nesta conversa que esta máquina é nova e a stack nunca havia sido executada, autorizando seu primeiro bootstrap sob `~/.microsoft/usersecrets/Altivy.Identity`. Isso substitui a hipótese anterior de um volume já inicializado a localizar. Certificados e temas já estavam presentes; o diretório `postgres-data` estava ausente.

O [bootstrap rootless](../tools/scripts/start-shared-identity.ps1) prepara um Compose público em `runtime-rootless/compose.rootless.json` e um `runtime-rootless/nginx.conf` no diretório externo da Identity. Ambos contêm somente estrutura operacional, paths Linux e referências de variáveis, não credenciais resolvidas. O `.env` existente é referenciado exclusivamente pelo provider, sem leitura direta ou sobrescrita pelo script. O manifest original de plataforma e os demais repositórios não foram alterados.

O adaptador mantém o projeto `altivy-identity`, redes `identity-global`/`identity-client`/`identity-data`, imagens corporativas com digest e gateway em `https://localhost:7443`. Resolve o DNS nginx pelo gateway da rede Podman, em vez de usar o resolver Docker `127.0.0.11`. O primeiro cluster PostgreSQL recebe o schema `identity` pelo [SQL de inicialização](../deploy/identity-init.sql); esse hook só é aplicado ao diretório novo. O script recusa bootstrap novo se o destino de dados já existir ou se encontrar uma stack existente; não remove/reinicializa dados nem gera certificados.

Pins usados: PostgreSQL `sha256:10d93543368ebe4d69a473e068cda1a7a8a94e812579d6a29a1b73a851eafab2` (18.4-pgvector0.8.6-bookworm), nginx `sha256:e46f7470fc0f6d758d5b9ba5651fd1bba1236ae33d7a269faf68f535abf98298` (1.28.0-bookworm) e Keycloak `sha256:0f0aaa3aaa6129a5b4d506ebfc2698d5397b51216d147b4b925f2ab4eb47769b` (26.7.0), todos do namespace corporativo `docker.io/lzocateli`. São referências de infraestrutura compartilhada para uso local, não novas bibliotecas incorporadas à API ou autorização para redistribuir imagens.

Na raiz, para um ambiente novo autorizado e ainda sem adaptador:

```powershell
pwsh -NoProfile -File tools/scripts/start-shared-identity.ps1 -InitializeNewInstance -PrepareOnly
```

O preparo pode criar a rede compartilhada e resolver/puxar a imagem Keycloak prevista, mas não inicia serviços. Depois, iniciar explicitamente a configuração preparada:

```powershell
pwsh -NoProfile -File tools/scripts/start-shared-identity.ps1 -InitializeNewInstance -StartPrepared
```

Para validar a interpolação existente, sem iniciar serviços:

```powershell
pwsh -NoProfile -File tools/scripts/start-shared-identity.ps1 -ValidateOnly
```

`PrepareOnly`, `ValidateOnly` e `StartPrepared` são mutuamente exclusivos. Sem opção explícita de usar a configuração preparada, um segundo bootstrap é recusado. Não remover arquivos/diretórios para contornar os guards. O script não sobrescreve a configuração preparada automaticamente; ela deve ser revisada antes de alterações.

Resultado observado: `altivy-identity-postgres` e `altivy-identity-keycloak` saudáveis; `altivy-identity-gateway` ativo. Portas somente em loopback: PostgreSQL 5432 e gateway 7443. O discovery `/realms/master/.well-known/openid-configuration` retornou 200 pela borda nginx, issuer `https://localhost:7443/realms/master`. O smoke validou cadeia e hostname usando apenas `certs/rootCA.pem` existente; `tls.key` não foi montada no runner. A tentativa com apenas a leaf falhou corretamente por falta da CA e foi substituída pelo teste com a raiz pública correta; nenhum `ignoreHTTPS` ou `rejectUnauthorized: false` foi usado. A confiança global no host/navegador não foi alterada.

Os serviços compartilhados permanecem ativos para os produtos. Na etapa de bootstrap descrita acima não houve geração de tokens de usuário nem importação de realms de produtos. A integração GarageMotor subsequente está registrada abaixo. A autenticação do GarageMotor continua local, sem uso automático do login Keycloak. O preview isolado permanece separado; T067 inteira e a qualificação de produção não estão concluídas.

## Integração GarageMotor Validada

Em 2026-11-11, o [provisionador GarageMotor](../tools/scripts/initialize-local-garagemotor.ps1), com `-InitializeNewDatabase`, criou exclusivamente configuração externa, banco próprio, duas roles e migration inicial dentro do PostgreSQL compartilhado. Ele recusa configuração, banco ou roles existentes. O [registrador da rota](../tools/scripts/register-local-gateway.ps1), com `-Apply`, também foi executado e recusa um destino preparado. Não apagar arquivos ou dados para contornar os guards nem repetir esses comandos como rotina de start.

Na raiz, com a stack compartilhada, configuração externa e CA pública existentes:

```powershell
pwsh -NoProfile -File tools/scripts/validate-feature.ps1 -FeatureDirectory specs/003-garagemotor-design-system -Profile web-e2e -Engine podman -KeepRunning
```

Resultado observado: `passed`, exit 0, seis testes, sendo quatro contratos estáticos e duas jornadas reais nos viewports 1280x900 e 390x844. `web-e2e` usa `identity-global`, a URL compartilhada e somente a CA pública montada; nenhum arquivo de credenciais é entregue ao runner Playwright. Um encaminhamento TCP interno ao runner transporta TLS intacto até o nginx existente, mantendo hostname `localhost`, cadeia validada e `ignoreHTTPSErrors: false`. A confiança global do host não foi alterada. O navegador local precisa confiar na CA pública existente; nunca contornar alertas de certificado.

O runner gera contas fictícias e senhas aleatórias sem imprimi-las. Capturas, vídeo e trace ficam desativados; a página é limpa antes dos diagnósticos finais. O teste comprova cadastro 202, login 200 sem bearer no JSON web, cookie Secure/HttpOnly/SameSite=Strict, sessão preservada na troca de tema, rejeição de DELETE sem CSRF, emissão de dez códigos offline e logout 204. A exceção pública do cliente limita-se a POST; DELETE de sessão envia CSRF. Uma espera limitada permite o bootstrap público após recriação Angular, sem repetir mutações. `-KeepRunning` mantém os serviços disponíveis; sem essa opção, o perfil encerra API/web ao finalizar.

`quality` passou com formatação, análise estática e builds Angular/.NET. Dois testes unitários também passaram. Os perfis .NET `integration` e `load` ainda usam a topologia legada e não foram qualificados nesta atualização; não executá-los contra a plataforma compartilhada por suposição. Os 39 testes isolados anteriores de aparência não se tornam integração real por esta execução. SMTP/recuperação por e-mail, restauração automática da sessão após reload, matriz completa, Android, assinatura e integração vault/GitHub permanecem pendentes.

O key ring persiste no volume privado `garagemotor-api-keys`, cifrado com o certificado externo e acessível ao UID 1654. O certificado local foi emitido com validade de um ano. Backup/recuperação precisa preservar tanto o volume quanto a configuração externa; perder ou trocar o certificado sem migração impede decifrar chaves anteriores. Rotação e recuperação completa ainda exigem procedimento e validação antes de produção. O ciclo parar/iniciar foi validado sem remover volumes; isso não comprova recuperação de desastre nem restauração de sessão no frontend.
