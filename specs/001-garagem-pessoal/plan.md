# Plano de Implementação: Garagem pessoal e familiar

**Branch**: `001-garagem-pessoal` | **Data**: 2026-11-08 | **Spec**: [spec.md](spec.md)

**Entrada**: Especificação em `specs/001-garagem-pessoal/spec.md`.

**Status**: ADR de persistência aceito e implementação autorizada pelo owner em 2026-11-09; execução das tarefas em andamento. Instalação distribuída e publicação não autorizadas.

**Origem**: IA assistida. **Revisor humano**: CEO da Altivy e owner do GarageMotor.

**Feature selecionada**: `SPECIFY_FEATURE_DIRECTORY=specs/001-garagem-pessoal`.

**Revisão**: 2026-11-09, após os esclarecimentos de busca e recarga elétrica e a decisão humana de aceitar o ADR. US7/US8 e os respectivos contratos/tarefas integram o escopo autorizado. Modelo, OpenAPI, quickstart e tarefas foram sincronizados; segurança operacional, builds e testes ainda não foram validados. Nenhuma alteração de stack.

## Resumo

Preparar o MVP pessoal/familiar para veículos, abastecimentos/recargas elétricas, despesas, manutenção, lembretes e comprovantes. Primeira entrega: web, self-hosted e Android. iOS, OCR, chat, predição, pequena frota empresarial e edição proprietária ficam fora.

O fluxo oficial resolveu os bloqueios da pesquisa por fontes/tagged manifests da API GitHub e confirmação humana de direitos do DS. As emendas constitucionais 1.1.0 e 1.2.0 autorizam, respectivamente, ferramentas Android e três dependências de build/teste estritamente fixadas; não autorizam licenças adicionais para o runtime nem execução no host. Modelo, contratos e guia de validação foram produzidos; resultados operacionais ainda não foram executados.

Os achados, fontes e bloqueios estão em [research.md](research.md).

## Contexto Tecnico

**Linguagem/Versão**: C#/.NET SDK 10.0.403, runtime/ASP.NET Core 10.0.14 LTS; Angular 22.2.5, CLI/build 22.2.6, TypeScript 6.0.3 e Node 24.22.2 LTS em manutenção. Não usar TypeScript 7, fora de >=6.0 <6.1. Versões fixadas, sem ranges flutuantes nos manifests do produto.

**Dependências Principais**: Capacitor core/cli/android 8.6.0 MIT, Npgsql 10.0.3 permissiva, MSTest SDK/framework 4.5.2 MIT, Playwright 1.65.0 Apache-2.0 e Prettier 3.9.10 MIT. UI própria Altivy/Dokpod; sem Ionic UI, ORM, mediator, axe-core ou nova biblioteca de componentes. As emendas constitucionais 1.2.0/1.3.0 cobrem somente lightningcss 1.33.0, lightningcss-linux-x64-gnu 1.33.0, caniuse-lite 1.0.30001823 e Microsoft.Testing.Extensions.CodeCoverage 18.12.0 como ferramentas de build/teste nos lockfiles, não como runtime do app; a licença exata do último permanece não verificada. A variante Linux x64 glibc é a única variante nativa MPL aprovada; outras variantes ficam fora da aprovação. Framework Identity/PasswordHasher para identidade local; criptografia própria proibida.

**Persistência**: PostgreSQL 18.6 e volume privado local, conforme [ADR aceito](../../docs/adr/2026-0001-persistencia-garagem.md). SQL parametrizado, transações, RLS como defesa adicional, partições mensais/default/índices por filho e retenção definida no [modelo](data-model.md). O aceite do datastore está registrado; migrations e testes reais continuam obrigatórios antes de declarar comportamento validado. Não há serviço S3/MinIO ou volume distribuído.

**Testes**: MSTest para domínio/API, integração real PostgreSQL/filesystem, contrato OpenAPI, idempotência/concorrência/partições, Playwright para web/componentes e validação Android. Qualidade: dotnet format/analyzers, Prettier, compilação Angular/TypeScript estrita. Sem segundo framework de unit frontend; Playwright testa primitivas em navegador e lógica compartilhada. Nenhum teste da aplicação executado nesta preparação.

**Plataforma Alvo**: API, frontend, banco, nginx 1.30.6 e runners em containers Linux. HTTP exclusivamente por nginx; Android conectado em API35/36 na matriz inicial. JDK Temurin 21.0.13+9, Gradle 8.14.3, AGP 8.13.0, compile/target API36 e build-tools 36.0.0 somente em containers. Aceite de termos e audit/checksums antes de uso. iOS excluído, sem exceção macOS.

**Esclarecimentos Android de 2026-11-11**: mínimo Android 15 (`minSdk 35`), `applicationId`/Capacitor `appId` permanente `it.altivy.garagemotor`, mantendo compile/target API36 e matriz API35/36. O owner declarou aceite de `android-sdk-license` para os artefatos SDK fixados em `docs/android-toolchain.md`; auditoria, integridade e compatibilidade da cadeia continuam pendentes de execução. A autorização de retomada web anterior não executa implicitamente tarefas Android.

**Assinatura Android**: chave, senhas e material de recuperação sob guarda persistente do vault corporativo. GitHub Actions deve obter acesso restrito somente no job autorizado de assinatura/deploy; nunca colocar o material no Git, caches, logs ou artefatos publicados. Material usado no job deve ser efêmero e descartado ao término, inclusive em falha; não replicar a chave em secrets GitHub por padrão. O owner indicou Azure Key Vault ou 1Password via `op`, sem selecionar uma operação definitiva ou uso simultâneo. Recomendação técnica para CI: Azure Key Vault com identidade federada GitHub OIDC, tokens temporários, environment protegido, revisão humana, privilégio mínimo e bloqueio em PRs/forks não confiáveis; é recomendação, não escolha humana aprovada. No caminho documentado do `op` com service account, existe uma credencial de bootstrap (`OP_SERVICE_ACCOUNT_TOKEN`), cuja proteção e rotação precisam ser desenhadas; não presumir OIDC ou ausência de segredo persistente nesse caminho. O `op` não foi instalado nem aprovado como dependência do projeto: versão, origem, licença e compatibilidade exigem verificação conforme a política vigente. Escolha do backend, identidade, referências não secretas e políticas precisam ser definidas antes de implementar o workflow. Isso é desenho de segurança, não autorização de deploy, publicação ou acesso a secrets reais. Essas opções são infraestrutura corporativa de assinatura, não dependência obrigatória de runtime do app ou da instalação self-hosted. A chave de assinatura do app é distinta das chaves do Android Keystore usadas para proteger a sessão do usuário.

**Tipo de Projeto**: monólito modular com uma API ASP.NET Core, SPA Angular compartilhada com Capacitor e jobs internos HostedService. Módulos Identity/Garages/Vehicles/Expenses/Fuel/Charging/Maintenance/Documents/Operations como pastas, não microserviços ou camada de dispatch obrigatória.

**Metas de Desempenho**: SC-005: dez usuários simultâneos, dois minutos de aquecimento excluídos e dez de medição, 80% histórico/20% totais. Conjunto fictício determinístico: 10.000 despesas em 20 garagens, três veículos por garagem e 12 meses; matriz4vCPU/8GiB/SSD. P95 <=2s por tipo, sem erros inesperados; seed, filtros e parâmetros registrados. Paginação50/máximo100 e índices por garagem/veículo/data; não extrapolar para outras instalações.

**Restrições**: AGPL-3.0-only; bibliotecas permissivas e ferramentas Android limitadas pelas constituições 1.1.0–1.3.0. As exceções 1.2.0/1.3.0 são nominais, fixas por versão/integridade e limitadas a build/teste em containers Linux; não alteram a política do runtime ou autorizam variantes de outra plataforma. Antes de usar imagens ou dependências, inventariar pacotes/transitivas, verificar licenças/avisos/hashes e falhar fechado para tudo que não esteja coberto pelas exceções. A licença do CodeCoverage segue não verificada e a aprovação não certifica compatibilidade jurídica. Não presumir que tags de fonte são hashes de pacote. Direitos do DS confirmados pelo owner, preservando terceiros. Sem IA, escrita offline, push obrigatório ou edição proprietária.

**Escala/Escopo**: pessoas/famílias, carros/motos, múltiplas garagens por identidade com papéis owner/editor/reader. Histórico financeiro até exclusão, auditoria90dias, exportações/staging24h. Dados ativos/arquivos/backups gerenciados contendo a garagem são eliminados até requestedUtc+30dias, inclusive cópias anteriores/durante o pedido. Operador responde pelas cópias sob gestão, sem garantia sobre cópias particulares externas. Matriz inicial é capacidade a testar, não trava de licenciamento.

**Busca textual**: usar full-text search nativo do PostgreSQL, sem extensão adicional. Consultas usam configuração portuguesa, normalização imutável de acentos e sintaxe web (frases entre aspas, `OR` e exclusão com `-`), com índices GIN nos campos autorizados por FR-031. Busca de registros fica no contexto de uma garagem e verifica membership em cada operação; busca de auditoria é endpoint separado e exige owner ativo. Não indexar identificadores veiculares, valores, nomes/conteúdo de arquivos ou secrets. Os índices dos pais particionados devem cobrir todas as partições mensais existentes e futuras, inclusive a partição default.

**Recargas elétricas**: sessão `ElectricCharge` separada de `FuelRecord`, vinculada somente a veículo elétrico e a uma única despesa da categoria `fuel`, com origem `electric_charge`. Medir energia como inteiro Wh e apresentar kWh; manter tarifa doméstica BRL/kWh por garagem, com vigência/histórico e snapshot na sessão. Owner/editor alteram a tarifa; reader consulta. Recarga pública guarda o total informado; se não houve cobrança, o usuário pode registrar o mínimo de R$ 0,01, nunca valor zero. Eficiência é cheia-a-cheia: distância entre dois marcos de bateria cheia dividida pelos Wh das sessões posteriores ao marco anterior até o marco final, incluindo parciais. Sem dois marcos válidos não se apresenta valor completo. Não prever integração com carregadores, CPO, operadora elétrica ou importação de telemetria.

## Verificação da Constituição

*GATE: Deve ser aprovado antes da pesquisa da Fase 0 e revisado novamente após o design da Fase 1.*

Verificação inicial e após desenho contra a [constituição 1.2.0](../../.specify/memory/constitution.md):

| Princípio | Situação | Tratamento |
|---|---|---|
| I. AGPL e reciprocidade | Conforme na preparação | Decisão humana registrada; arquivo oficial, avisos e inventário ainda precisam de trabalho autorizado, sem publicar dados/secrets. |
| II. Self-hosted | Conforme no escopo | Núcleo sem assinatura/chave; três modalidades de serviço não aprovam preços ou edição proprietária. |
| III. Tecnologia/DS | Conforme para desenho | Tags, faixas e licenças primárias verificadas; direitos DS e exceção Android confirmados. Audit de artefatos/transitivas/termos antes de uso é gate de execução, sem dispensa. |
| IV. Segurança/integridade | Conforme como requisitos, não validado | Spec cobre autorização, concorrência, evidências e recuperação; threat model e testes reais pertencem ao desenho/implementação autorizados. |
| V. Spec Kit/qualidade | Conforme com gates pendentes | Implementação autorizada pelo owner em 2026-11-09; executar somente tarefas revisadas e preservar os gates de runtime, segurança e imagens. |

**Gate após pesquisa**: F0-1/F0-2/F0-3 resolvidos para desenho, conforme pesquisa atual. Nenhuma licença presumida ou quebra da exigência de containers foi usada como exceção silenciosa.

**Gate após desenho**: ADR aceito pelo owner; execução autorizada dentro das tarefas revisadas. [Modelo](data-model.md), [OpenAPI](contracts/openapi.json), [schemas públicos](contracts/resources.schema.json), [regras HTTP/mobile](contracts/api.md) e [quickstart](quickstart.md) alinhados aos esclarecimentos. A revisão humana de segurança e as validações reais permanecem gates de promoção; checagens documentais não certificam comportamento de runtime.

## Estrutura do Projeto

### Documentação desta Funcionalidade

```text
specs/001-garagem-pessoal/
  spec.md
  plan.md
  research.md
  data-model.md
  quickstart.md
  contracts/openapi.json
  contracts/resources.schema.json
  contracts/api.md
  checklists/requirements.md
```

### Código-Fonte (raiz do repositório)
Estrutura a ser criada pelas tarefas, não código existente:

```text
backend/GarageMotor.Api/          API, módulos, SQL stores e jobs internos
backend/GarageMotor.Tests/        MSTest de domínio, API e integração
frontend/web/                    SPA Angular, DS próprio e configuração Capacitor
frontend/web/android/            projeto nativo gerado/controlado
frontend/tests/                  Playwright, contrato e fixtures
deploy/                          Dockerfiles, nginx e manifests de imagens auditadas
tools/scripts/                   orquestração PowerShell, qualidade e recuperação
docs/adr/                        decisões e gates humanos
```

**Decisão de Estrutura**: uma API e dois projetos .NET inicialmente, módulos por domínio e UI compartilhada; stores Npgsql sem ORM. Não copiar agentes, terminal, Keycloak, protocolos de containers ou Business do Dokpod. Sem diretórios/scripts novos em .specify para lógica própria.

## Registro de Complexidade

Exceção Android formalizada na constituição 1.1.0 e exceção nominal de ferramentas de build/teste formalizada em 1.2.0. Nenhuma delas se estende ao runtime do app, outras versões ou execução fora de containers. Partições/RLS/staging decorrem de integridade e política do projeto; evitar serviços adicionais e criptografia própria.

## Desenho e segurança

**Ambiente local compartilhado — decisão humana de 2026-11-11**: o owner autorizou criar a pasta/arquivos privados do GarageMotor, mas determinou reutilizar a instância PostgreSQL e o nginx já existentes, seguindo os exemplos Dokpod/architecture-knowledge-base. A orquestração do GarageMotor deve gerir somente API/web; não criar, recriar, parar ou atualizar a infraestrutura compartilhada nem seus volumes. O padrão observado usa redes externas `identity-global` e `identity-client`; nomes reais, contexto de engine, DNS e origem pública precisam ser confirmados no ambiente acessível. Usar aliases específicos `garagemotor-api`/`garagemotor-web`, manter papéis e dados próprios separados dentro da instância e não ler/copiar secrets de outros projetos. O provisionamento deve falhar antes de iniciar se as redes externas não estiverem disponíveis; nunca substituí-las por containers de teste ou criar infraestrutura silenciosamente. A rota do GarageMotor no gateway existente depende de confirmação da origem/certificado e deve preservar as demais rotas. Esta decisão posterior substitui o lançamento local de PostgreSQL/gateway próprios neste recorte, não a arquitetura lógica da API ou os requisitos de segurança.

**Engine e bootstrap da Identity — esclarecimento posterior de 2026-11-11**: o owner confirmou exclusivamente Podman rootless e apontou `altivy-it/architecture-knowledge-base/platforms/identity` e os scripts Dokpod. `podman info` confirmou rootless; os launchers e perfis GarageMotor rejeitam Docker/rootful. As imagens PostgreSQL/nginx corporativas já estão no armazenamento local e o arquivo externo da Identity existe. A cópia local dos scripts de plataforma ainda referencia Docker/Windows; não executá-la por alias ou shell de compatibilidade. O provider Podman Compose 1.0.6 não aceita `config --quiet`; o preflight suportado `config --services`, com saída capturada e filtrada, falhou por parsing de mounts, com sintaxe de drive Windows identificada apenas por flag booleana. Não foram emitidos valores ou caminhos privados, nem alterados dados/serviços. É necessário confirmar os caminhos Linux do bind mount de dados existente, certificados e temas antes de qualquer start ou adaptação operacional da plataforma; nunca inicializar um volume novo como substituto.

Web é servido pelo nginx, que encaminha somente `/api/v1` à API; banco/volume nunca ficam públicos. Desenvolvimento Angular também fica atrás desse proxy. Android usa assets locais do mesmo build e endpoint HTTPS validado, sem relay do fornecedor. CORS é allowlist explícita, cookies web HttpOnly/Secure, antiforgery do framework e bearer opaco Android; revogação/membership em toda operação.

Credenciais de runtime são injetadas por arquivo externo. Chaves protegidas do framework ficam em volume privado e são protegidas por credencial externa; material plaintext nunca vai para código, fonte publicada ou exportação familiar. Android usa Keystore/APIs da plataforma, não localStorage ou plugin não auditado. Operador/DBA self-hosted é parte confiável da infraestrutura; não prometer isolamento criptográfico contra ele.

Threat model mínimo para revisão humana:

| Superfície/risco | Controle proposto | Evidência exigida |
|---|---|---|
| IDOR/membership revogada | Contexto de garagem, RLS, role sem BYPASSRLS, reautorização por operação | Integração de duas garagens, conexões pooled e revogação |
| Conta/sessão/CSRF | Framework Identity/PasswordHasher/antiforgery, rate limit, token opaco com hash, cookie seguro | Enumeração, recuperação/código, origem e expiração/revogação |
| Arquivo/path/upload | Destino gerado, staging, limites, tipo/conteúdo, download attachment/nosniff | Travessia de path, excesso, falha parcial e acesso cruzado |
| Duplicação/corrupção financeira | Transação, versão, recibo persistente e lock no rollover | Escrita simultânea, repetição após falha e recálculo |
| Endpoint/Android | TLS sem bypass, major compatível, troca limpa de sessão, Keystore | Certificado inválido, instância trocada e perda de rede |
| Backup/exclusão | Snapshot coordenado, checksums, destino vazio, retenção explícita | Restore limpo, cópia corrompida, falha antes de substituir destino |

Controles adicionais após esclarecimento:

- Toda criação de domínio usa Idempotency-Key e recibo transacional; criação de garagem usa escopo do ator antes de existir garageId. Replay preserva status/id/version e reautoriza acesso; conflito de operação/conteúdo retorna409. Recibos não armazenam tokens ou respostas privadas de autenticação.
- Recuperação: 16 bytes aleatórios do framework (128bits), base64url22caracteres, apenas hash persistido e comparação de tempo constante. SMTP15min; lote offline10códigos/12meses; reautenticar para renovar, invalidar lote anterior, revogar sessões e demais códigos após sucesso. Janela móvel de cinco falhas/conta15min. Um teto adicional por origem é necessário, mas 100 falhas/origem/15min é somente proposta técnica configurável, não decisão humana nem requisito numérico da spec; revisar o valor na revisão de segurança antes da promoção. Contagem concorrente e identidade inexistente não podem permitir enumeração ou bloqueio permanente.
- BackupCatalog/DeletionAuthority no controle operacional existente, sem novo datastore/broker. Pedido fixa deadline30dias após requestedUtc; catálogo antecipa prazo de todas as cópias contendo a garagem e comprova remoção. Restauração nunca substitui autoridade atual pelo dump antigo: origem autenticada ou exportação operacional independente verificável, cobertura desde criação da cópia até promoção e coordenação de sequência. Ausência/corrupção/cobertura parcial/atualidade não demonstrada bloqueiam promoção; hash só não comprova atualidade.
- Reaplicar pedidos pertinentes antes de tornar dados restaurados públicos e invalidar sessões/códigos antigos. Garantia é para backups sob gestão informada, não cópias privadas fora do alcance do operador. Verificação não depende obrigatoriamente de SaaS.
- DTOs públicos são schemas fechados por kind em resources.schema.json; negar propriedade extra/privada, tipo errado ou kind diferente do esperado para operationId. Lote offline é resposta excepcional não cacheável, sem dados em exportação/recibos/logs.

Revisão de segurança e auditoria WCAG 2.2 AA são gates; Playwright verifica teclado/foco/estados e subsets automatizáveis, não certifica sozinho toda conformidade AA. Não adicionar axe-core nem bibliotecas de UI externas; somente lightningcss 1.33.0 e sua variante Linux x64 glibc 1.33.0 estão cobertas pela exceção de build/teste das constituições 1.2.0/1.3.0.

## Sequência para tarefas

Correção da ordem de preparação: (1) pins/inventário inicial e gates humanos; (2) manifests mínimos de projetos/package, sem executar ou instalar dependências; (3) resolução de metadados/lockfiles em container isolado, sem secrets nem scripts de pacotes; (4) auditoria transitiva, hashes/avisos e então permissão de restore/install/build nas tarefas autorizadas. Lockfiles NuGet ficam ao lado de cada csproj, não numa pasta backend sem projeto. Não exigir resolução final antes de existir o manifesto que a determina; se a ferramenta executa targets arbitrários ou não permite isolar essa etapa, interromper e revisar o procedimento, sem contornar o gate.

1. Preparação: ratificação do ADR, inventário/lockfiles/imagens/avisos, licença oficial e reconciliação MIT, fundamentos de deploy/qualidade, threat model.
2. Fundação: autenticação, contexto de garagem, stores/migrations/partições, idempotência, DS compartilhado e runners reais.
3. US1/US2/US3: garagem/veículos, custos e abastecimentos com tests primeiro no recorte.
4. US8: recargas elétricas, tarifa histórica e eficiência cheia-a-cheia, após US1–US3.
5. US4/US5: planos/lembretes e comprovantes com integridade e recuperação.
6. US6: Android, instalação própria, exportação/backup, carga, compatibilidade e smoke de imagens.
7. US7: busca textual da garagem e da auditoria, após estarem disponíveis as entidades e campos cobertos por FR-031.
8. Transversal: formato/análise/build/testes, revisão de segurança e acessibilidade, matriz e documentação. Não marcar tarefa como executada sem evidência.

US6 faz parte da entrega, não autorização de publicar app nas lojas. A validação do núcleo móvel inclui recarga elétrica US8; busca US7 depende da aplicação web/Android preparada em US6. A implementação segue as tarefas autorizadas; nenhum marco autoriza push, release ou publicação.

A revisão de tarefas deve cobrir US1–US8, incluindo FR-031/SC-009 e FR-032/SC-010, endpoints de busca separados, autorização por garagem/owner, índices GIN nas partições, sessões elétricas, tarifa histórica, idempotência e testes de eficiência cheia-a-cheia. Recargas usam a despesa existente e não ampliam a telemetria nem os campos de auditoria. Recarga pública sem cobrança é registrada pelo mínimo de R$ 0,01, conforme decisão humana, nunca como despesa zero.

## Validações e pendências

Checagens em containers: JSON/OpenAPI refs/IDs, schemas fechados, estados/decimais, cobertura e IDs das 82 tarefas, estrutura dos documentos e links. Não foram executados serviços, migrations, unit/integration, carga, Android ou Playwright. Validar o contrato completo na implementação, inclusive DTOs de resposta por kind.

As camadas de imagens estão inventariadas por digest/plataforma em `deploy/images.lock.json` e `deploy/sboms/`; builds/testes ainda dependem da conclusão das tarefas de fundação. Uso dos pacotes excepcionados limita-se ao que foi aprovado na constituição 1.2.0. O aceite humano dos termos do Android SDK foi declarado pelo owner em 2026-11-11, conforme `spec.md` e `docs/android-toolchain.md`; isso remove somente a pendência de aceite desses termos, não a auditoria, integridade, compatibilidade ou implementação ainda pendentes. O ADR de PostgreSQL foi aceito; revisão de segurança e evidências de runtime continuam pendentes. Preços, orçamento, SLA, prazo, publicação de portal, push e release são decisões separadas. A referência de origem do Dokpod não autoriza copiar materiais de terceiros sem avisos.
