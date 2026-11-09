# Pesquisa Técnica: Garagem pessoal e familiar

**Feature**: [spec.md](spec.md). **Data**: 2026-11-08.
**Status**: Fase 0 concluída para desenho; histórico de bloqueios preservado abaixo. Instalação, implementação e publicação permanecem não autorizadas.
**Origem**: IA assistida, incluindo pesquisa delegada somente leitura.
**Revisor humano**: CEO da Altivy e owner do GarageMotor.

## Decisões confirmadas

### Público e distribuição

- **Decisão**: pessoas/famílias; web, self-hosted e Android primeiro; iOS posterior.
- **Justificativa**: esclarecimento humano e [handoff](../../.specify/assessments/gestao-veicular-inteligente/decision.md).
- **Alternativas consideradas**: pequena frota inicial e lançamento simultâneo com iOS; não selecionadas pelo humano.

### Frontend e integração mobile

- **Decisão**: Angular 22 obrigatório; Angular + Capacitor sem obrigação de componentes Ionic; DS Altivy com Dokpod como referência Angular.
- **Justificativa**: escolha humana de 2026-11-08, registrada nas premissas da spec.
- **Alternativas consideradas**: Ionic Angular como base de UI; não obrigatório. Não substituir DS por biblioteca genérica.
- **Limite**: decisão de direção não verifica versões, compatibilidade ou licenças exatas.

### Licenciamento e serviços

- **Decisão**: AGPL-3.0-only para código próprio; autogerido, SaaS e suporte self-hosted são modalidades de serviço.
- **Justificativa**: decisão humana e constituição 1.0.0. Licença permite comercialização cumprindo seus termos.
- **Alternativas consideradas**: MIT anterior é histórico; Business proprietária do Dokpod não é decisão para GarageMotor.
- **Limite**: dependências seguem regra permissiva e verificada. Não assumir que AGPL raiz concede todos os direitos de assets/fontes ou aprova copyleft adicional.

## Achados da pesquisa local delegada

O agente Explore foi acionado pela Fase 0, somente leitura e sem consultas Web, instalação ou acesso a secrets. Consultou arquivos concretos do Dokpod e retornou:

- SDK .NET 10.0.400 na configuração local; backend usa várias dependências com versões centralizadas, mas suas licenças não estão inventariadas completamente.
- Frontend declara Angular 22.2.3, Node `>=24.22.1 <25` e TypeScript `~6.0.2`; não foi encontrado lockfile npm. Essas declarações não comprovam resolução, suporte ou licença da versão exata.
- DS próprio contém button, card, charts, checkbox, data-grid, drawer, empty-state, field, format, icon, key-value, kpi, progress, search, select, status, tabs e toast, com tokens/styles separados.
- ADR próprio do Dokpod registra derivação Altivy, vidro limitado e WCAG 2.2 AA. Isso informa a referência, não transfere aprovação de dependências ou ADRs ao GarageMotor.
- Outfit/JetBrains Mono têm avisos OFL locais; `axe-core` é MPL-2.0 e somente de teste no Dokpod. Esse aceite não vale automaticamente aqui, onde a política de dependências permanece restritiva.
- Dokpod está em prévia, sem release estável. Seus componentes não devem ser adotados como biblioteca externa estável sem avaliação de direitos, proveniência, manutenção e testes.

## Candidato backend confirmado parcialmente

- **Decisão candidata**: .NET 10 LTS, SDK 10.0.403 e runtime 10.0.14.
- **Justificativa verificável**: metadata oficial Microsoft respondeu HTTP 200, com canal `10.0`, tipo `lts`, fase `active` e fim de suporte `2028-11-14`.
- **Alternativas consideradas**: SDK local Dokpod 10.0.400; a referência local não substitui atualização da linha suportada.
- **Limite**: metadata de suporte não constitui auditoria de licenças de todo o backend ou de imagens. Nenhum pacote foi instalado.

## Histórico dos bloqueios da Fase 0

| ID | Decisão pendente | Evidência faltante | Consequência |
|---|---|---|---|
| F0-1 | Versões fixadas e compatíveis de Angular 22, Node/TypeScript, Capacitor e ferramentas de teste | Metadata oficial e requisitos de compatibilidade não puderam ser obtidos | Não instalar, fixar versões por palpite ou produzir matriz mobile como validada |
| F0-2 | Licenças exatas e direitos de DS, driver e ferramentas | Licenças de candidatos Npgsql/frontend não verificadas; componentes Dokpod exigem proveniência própria | Regra local bloqueia adoção; não importar dependências/avisos automaticamente |
| F0-3 | Banco, armazenamento e particionamento | Versão PostgreSQL estável suportada não confirmada; driver e decisão de datastore pendentes | Não produzir schema, migrations ou contrato definitivo antes da pesquisa/ADR correspondente |

Os bloqueios são técnicos, não exigência de provar receita antes de especificar. A necessidade declarada pelo owner sustenta a spec; ela não permite omitir verificação constitucional no plano.

## Fontes e resultados

Hosts não previstos na allowlist da descoberta foram autorizados explicitamente pelo humano nesta rodada. As conexões foram fixadas a IP público validado, com validação de peer e TLS. Não foram seguidos redirecionamentos ou instruções remotas. Falha de conexão não informa conteúdo nem licença da fonte.

| Fonte sanitizada | Host | Resultado |
|---|---|---|
| https://registry.npmjs.org/@angular%2fcore/22.2.3 | registry.npmjs.org | Conexão encerrada; também falhou na checagem pelo host |
| https://registry.npmjs.org/@angular%2fcompiler-cli/22.2.3 | registry.npmjs.org | Conexão encerrada |
| https://registry.npmjs.org/@capacitor%2fcore/latest | registry.npmjs.org | Conexão encerrada; também falhou na checagem pelo host |
| https://registry.npmjs.org/@capacitor%2fandroid/latest | registry.npmjs.org | Conexão encerrada |
| https://registry.npmjs.org/typescript/6.0.2 | registry.npmjs.org | Conexão encerrada |
| https://registry.npmjs.org/@playwright%2ftest/latest | registry.npmjs.org | Conexão encerrada |
| https://www.postgresql.org/support/versioning/ | www.postgresql.org | Timeout de TLS |
| https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/10.0/releases.json | dotnetcli.blob.core.windows.net | HTTP 200; metadata .NET consultada |
| https://api.github.com/repos/npgsql/npgsql/contents/LICENSE?ref=v10.0.0 | api.github.com | Conexão encerrada; também falhou na checagem pelo host |

O endpoint `latest` foi apenas tentativa de descoberta de versão, não tag flutuante aprovada para instalação. Fonte Microsoft é mutável: metadata consultada em 2026-11-08. Registrar versões e hashes reais na conclusão da pesquisa.

## Verificações propostas na primeira rodada

Conseguir acesso verificável às fontes aprovadas ou fornecer inventário confiável de versão/licença com proveniência. Resolver os três bloqueios, rever o gate constitucional e somente então executar a Fase 1 de `/speckit-plan`. Não há contratos, tarefas executáveis ou análise SDD completa enquanto a pesquisa estiver incompleta.

## Fechamento da Fase 0 — 2026-11-08

As consultas diretas anteriores continuam registradas como falhas. Nesta retomada, a CLI GitHub consultou a API oficial via HTTPS, sem ler/exibir credenciais, executar conteúdo remoto ou instalar pacotes. Não se afirma que essa via realizou o pinning de IP das consultas anteriores. Conteúdo remoto foi tratado como dado.

As duas pesquisas delegadas foram somente leitura. Seus resultados foram conferidos pelo agente principal nas tags oficiais; recomendações sem evidência exata não foram usadas para liberar dependências. A primeira tentativa Angular sem prefixo `v` retornou 404; a referência correta foi confirmada pelas releases oficiais.

### Versões selecionadas para o desenho

| Componente | Versão fixa | Licença/fundamento verificado | Compatibilidade e decisão |
|---|---|---|---|
| .NET SDK/runtime/ASP.NET Core | 10.0.403 / 10.0.14 | Metadata Microsoft da linha LTS; runtime e ASP.NET Core MIT na tag v10.0.14 | Suporte até 2028-11-14; inventário completo das imagens continua gate antes de uso |
| Angular framework | 22.2.5 | MIT em angular/angular, v22.2.5 | Release não prerelease de 2026-11-06; major obrigatória do owner |
| Angular CLI/build | 22.2.6 | MIT em angular/angular-cli, v22.2.6 | Linha 22; source usa placeholders de publicação, não versão 0.0.0 real |
| TypeScript | 6.0.3 | Apache-2.0 em microsoft/TypeScript, v6.0.3 | Satisfaz >=6.0 <6.1 de compiler-cli/build; TypeScript 7 foi rejeitado |
| Node | 24.22.2 | Licença primária MIT em nodejs/node, v24.22.2 | ^24.15.0 aceito por Angular; LTS em manutenção até 2028-04-30 |
| Capacitor core/cli/android | 8.6.0 | MIT em ionic-team/capacitor, tag 8.6.0 | CLI >=22 Node; Android exige core ^8.6.0; mesmos patches fixados |
| Playwright | 1.65.0 | Apache-2.0 em microsoft/playwright, v1.65.0 | Node >=20; apenas ferramentas de teste, não runtime do produto |
| Npgsql | 10.0.3 | Licença PostgreSQL permissiva em npgsql/npgsql, v10.0.3 | Target net10.0; consultas parametrizadas; sem adicionar EF por conveniência |
| PostgreSQL | 18.6 | Licença PostgreSQL em postgres/postgres, REL_18_6 | Release estável documentada de 2026-08-13; sem alegar uma linha LTS oficial |
| MSTest SDK/framework | 4.5.2 | MIT em microsoft/testfx, v4.5.2 | Runner backend selecionado; sem adotar xUnit adicional |

Versões transitivas, avisos incorporados, hashes de pacotes/imagens e lockfiles devem ser inventariados antes de instalação/execução, com falha fechada para licença ausente ou incompatível. O desenho não afirma que um install, build ou teste já comprovou a matriz.

### Evidências imutáveis de licença e manifestos

Referências oficiais consultadas por `repos/<owner>/<repo>/contents/<path>?ref=<tag>`:

- Angular e CLI: LICENSE blob `5ec8e26b9eec0aa26838c4bfd32a260108524ad5`; compiler-cli manifest `a0083832e81b694480ee9fff040e95de4c092b53`, build manifest `1b0aeb21bf230cbe09999a60b79f55ac1abe7b03`.
- Capacitor: LICENSE blob `c3e903bdded1959466a54a74dd3e9361481f8144`; core `43d3a909dc5c51b5bd224974056470609f61f5bb`, cli `cb3ff1050b22c2b300593109cf9d5041a06bc4f6`, android `e628a3fc710e63b357a5348d31e007b57da0eba1`.
- TypeScript LICENSE.txt: `8746124b277914d0f0fd9cf4aef2ed3b587143d9`; Node LICENSE: `9cc3315dd388a004c225620b30c5806bba3151d5`.
- Playwright LICENSE: `df112373eb2e23e459bf93ec412be1764dc5a38b`; Npgsql LICENSE: `c551cb7b0c921b082c2424f213e12fb6b84a864c`; PostgreSQL COPYRIGHT: `0a397648dcd3c2177acc58bd7daecd11ad64be62`.
- MSTest LICENSE: `d859446c6968eaeff058d8c5de28e607ffb4c717`; runtime/ASP.NET Core LICENSE: `984713a49622a96da110443c15477613bc12656b`.

Tags são referências de versão; blobs vinculam o texto conferido. Não confundir blob de licença com hash do pacote distribuído, que deve ser verificado separadamente antes de uso.

### Android e emenda constitucional

- **Decisão**: JDK Temurin 21.0.13+9, Gradle 8.14.3, Android Gradle Plugin 8.13.0, compile/target API 36 e build-tools 36.0.0. Capacitor aceita min API 24; a matriz inicial de qualificação será API 35 e 36, sem promessa de funcionamento em toda versão antiga.
- **Fundamento**: tag Capacitor 8.6.0, build.gradle blob `6ece3eda05e25df4b597be28f28c64f896397f50` e wrapper blob `7705927e949fdfbe2bfa90ef9d397e6057ccf104`; release estável Temurin publicada em 2026-10-22.
- **Autorização humana**: o owner aprovou exceção restrita para ferramentas oficiais Android somente em build/testes em containers; formalizada na [constituição 1.1.0](../../.specify/memory/constitution.md). Não abrange libs do app, execução no host, iOS ou publicação.
- **Gate de execução**: termos de SDK/JDK, origem, checksum e imagens fixadas precisam de verificação antes de uso; aceites contratuais permanecem humanos. Não fornecer script que aceite contratos automaticamente.
- **Alternativas rejeitadas**: Capacitor 9 beta, build Android no host e incluir iOS nesta entrega.

### DS e fontes

- **Decisão**: owner confirmou os direitos necessários da Altivy para reutilização de seu DS; preservar avisos e direitos de terceiros. Implementar somente primitivas necessárias, sem copiar módulos Business ou importar dependências do Dokpod.
- **Fundamento**: confirmação humana nesta retomada; referências locais de tokens/componentes e avisos OFL de Outfit. AGPL do código próprio não relicencia fontes/assets.
- **Alternativas rejeitadas**: supor direitos por pertencer à mesma empresa ou adotar bibliotecas UI/Ionic compulsoriamente.

### Persistência e estrutura

- **Decisão**: um backend modular e um PostgreSQL, arquivos privados em volume local; nenhuma dependência de S3/MinIO, broker, cache distribuído ou serviço externo para o núcleo.
- **Justificativa**: acesso transacional e partições mensais para histórico/auditoria; volume local é suficiente ao recorte, evita nova biblioteca/storage. Novo datastore exige ADR proposto e decisão humana antes de implementação.
- **Partições**: preparar três meses futuros e doze anteriores, manter default para datas fora da janela, índices por filho e migração segura do default; retenção financeira depende de exclusão da garagem, não de corte global por idade. Auditoria tem 90 dias; drop somente de partições expiradas compatíveis com a política e sem vínculos financeiros.
- **Segurança**: contas locais com APIs Identity/PasswordHasher do framework, sessões opacas revogáveis e apenas hashes persistidos; nada de algoritmo criptográfico próprio. Isolamento por garagem, idempotência transacional e concorrência otimista em todas as mutações.
- **Alternativas rejeitadas**: EF, Keycloak, microserviços e storage distribuído sem necessidade demonstrada. Recuperação pode usar códigos offline ou SMTP configurado, nunca serviço SaaS obrigatório.

## Gates remanescentes de execução, não indefinições de desenho

Proxy nginx 1.30.6 da linha estável, licença BSD-2-Clause conferida em `nginx/nginx`, ref `release-1.30.6`, LICENSE blob `4366554533723cee5e155e317b0eb014867c1f0a`. Prettier 3.9.10 estável MIT, LICENSE blob `5767e34d30ab78796220f9492726dd2f677d4166` e package.json blob `9646b6d624fb8bbf1be2e068fa06528b45a6fe4e`, engine Node >=22 satisfeito por 24.22.2. Não confundir nginx mainline 1.31 com a linha estável escolhida.

O gate da Fase 0 fica liberado para produzir desenho. Antes de implementar: revisão humana da spec/plano/tarefas, aceite do ADR de datastore, auditoria de pacotes e imagens, avisos de terceiros, aceite humano de termos Android quando aplicável e threat model. Medidas de desempenho, backup, acessibilidade e Android permanecem planejadas, não executadas. Retomar pesquisa se a resolução real de pacotes contradizer a matriz.

## Revisão após esclarecimentos — 2026-11-09

Pesquisa delegada somente leitura confrontou as quatro decisões da spec com os artefatos existentes. Somente recomendações compatíveis com a spec e a constituição foram incorporadas; qualquer sugestão de permitir retorno de dados excluídos por serem anteriores ao pedido foi rejeitada. Nenhuma dependência nova, consulta Web, troca de stack ou implementação nesta revisão.

### Decisão — idempotência de domínio

- **Escolha**: todas as dez criações de FR-013 recebem chave UUID, contexto/ator e hash de operação+conteúdo; criar garagem usa escopo do ator antes de existir a garagem e vincula o recibo na mesma transação. Replay reautoriza acesso e preserva resultado público, sem códigos/tokens em recibos.
- **Justificativa**: Q1 da Session2026-11-09 resolveu a diferença de escopo; teste inclui garagens, veículos, convites, planos e exportações, não só financeiro.
- **Alternativas rejeitadas**: apenas financeiro; deduplicação por conteúdo sem contexto/ator; lookup do recibo sem revalidar acesso.

### Decisão — recuperação de conta

- **Escolha**: APIs do framework para RNG128bits/base64url22caracteres/hash/tempo constante; SMTP15min, dez códigos offline12meses, lote revogável, cinco falhas em janela móvel15min e 100 falhas/origem15min como parâmetro operacional definido nesta revisão. Nenhuma criptografia própria.
- **Justificativa**: Q2 e FR-001; teto adicional por origem reduz abuso sem a restrição de cinco compartilhada entre usuários em NAT. Valores de validade/revogação são humanos; teto por origem é decisão técnica revisável sem remover o limite por conta.
- **Alternativas rejeitadas**: código offline sem prazo, valores plaintext persistidos, reset de falhas em nova solicitação e bloqueio permanente da conta.

### Decisão — exclusão e restauração

- **Escolha**: catálogo local de backups e autoridade operacional mínima na persistência já proposta, separados do snapshot de domínio que será restaurado; deadline=requestedUtc+30dias para todas as cópias gerenciadas pertinentes. Estado atual autenticado ou artefato independente cuja procedência/integridade/cobertura/atualidade seja comprovável pelo operador; promoção coordenada pela sequência.
- **Justificativa**: Q3, FR-023/FR-024 e SC-006; hash de arquivo não comprova atualidade, e histórico incompleto não demonstra ausência de pedido. Sem evidência atual válida, falhar fechado. Operador self-hosted é autoridade confiável e não exige SaaS.
- **Alternativas rejeitadas**: usar somente a tabela antiga do dump, permitir backup anterior reintroduzir garagem excluída, reiniciar prazo pela criação de cópia e garantir eliminação de cópias particulares externas.
- **Limites**: não adicionar storage distribuído; cópias independentes de controle precisam de procedimento operacional e teste real. Não confundir esse desenho com prova de conformidade jurídica ou execução de eliminação.

### Decisão — respostas e teste de desempenho

- **Escolha**: schemas fechados de envelope/atributos para os 12 tipos públicos, nullable explícito e mapeamento por operationId; lote offline separado. Workload determinístico de 10.000 despesas/20garagens/60veículos/12meses, dez usuários, warmup2min/medição10min, 80%histórico/20%totais e P95<=2s por tipo, sem erros inesperados.
- **Justificativa**: schemas eliminam atributos arbitrários; Q4 define critério repetível, sem média global ocultando endpoint lento. Tests de contrato e carga serão implementados após revisão de tarefas.
- **Alternativas rejeitadas**: DTO de atributos livre, cenário de carga escolhido a cada execução e usar somente média/P95 agregado.

### Decisão — preparação de manifests e lockfiles

- **Escolha**: inventário inicial/pins, manifests mínimos sem execução de dependências, resolução isolada de metadados/lockfiles, auditoria transitiva e só depois restore/install/build autorizado. Lockfiles NuGet por csproj.
- **Justificativa**: corrigir a dependência entre T003 e T005 sem reduzir a auditoria ou executar targets/scripts de pacotes antes do gate.
- **Alternativas rejeitadas**: exigir lock final sem manifests existentes ou liberar scripts de dependências para resolver essa ordem.

Fase0 concluída para revisar o desenho, sem pendências de escolha de stack. Plano/modelo/contratos/quickstart e tasks.md foram sincronizados; a análise não destrutiva `/speckit-analyze` continua pendente. Gate de implementação permanece humano.