# Inventário inicial de dependências

**Feature**: [001-garagem-pessoal](../specs/001-garagem-pessoal/spec.md)
**Estado**: manifests e lockfiles resolvidos; pacote xUnit instalado em container fixado sem scripts. Build e formato passaram; os 17 testes backend passaram em containers, incluindo testes HTTP pela borda nginx.
**Origem**: IA assistida. **Revisor humano**: CEO da Altivy e owner do GarageMotor.

## Versões e licenças primárias

| Componente | Versão fixada | Licença primária | Origem verificada |
|---|---:|---|---|
| .NET SDK | 10.0.403 | MIT | Metadata e repositório oficial Microsoft |
| .NET/ASP.NET Core runtime | 10.0.14 | MIT | Tag oficial `dotnet/runtime` v10.0.14 |
| Npgsql | 10.0.3 | PostgreSQL License | Repositório/tag `npgsql/npgsql` v10.0.3 |
| xUnit.net v3 | 4.0.3 | Apache-2.0 | NuGet oficial; pacote `xunit.v3` inclui MTP v2 |
| Angular framework/compiler packages | 22.2.5 | MIT | Repositório/tag `angular/angular` v22.2.5 |
| Angular CLI/build | 22.2.6 | MIT | Repositório/tag `angular/angular-cli` v22.2.6 |
| TypeScript | 6.0.3 | Apache-2.0 | Repositório/tag `microsoft/TypeScript` v6.0.3 |
| Node.js | 24.22.2 | MIT | Repositório/tag `nodejs/node` v24.22.2 |
| RxJS | 7.8.2 | Apache-2.0 | npm registry oficial; integridade e licença registradas antes de uso |
| tslib | 2.8.1 | 0BSD | npm registry oficial; integridade e licença registradas antes de uso |
| zone.js | 0.15.1 | MIT | npm registry oficial; integridade e licença registradas antes de uso |
| Capacitor core/CLI/Android | 8.6.0 | MIT | Repositório/tag `ionic-team/capacitor` 8.6.0 |
| Playwright | 1.65.0 | Apache-2.0 | Repositório/tag `microsoft/playwright` v1.65.0 |
| Prettier | 3.9.10 | MIT | Repositório/tag `prettier/prettier` 3.9.10 |

## Origem dos pacotes

NuGet é limitado a `https://api.nuget.org/v3/index.json` por `backend/NuGet.Config`, com source mapping explícito para nuget.org. npm é declarado no `frontend/web/package.json`, com versões exatas e dependências diretas fixadas; a resolução futura deve usar o registry oficial `https://registry.npmjs.org/`.

## Gates pendentes

- Os lockfiles foram resolvidos em containers fixados; `npm ci --ignore-scripts` e NuGet `--locked-mode` foram executados sem build/testes.
- As emendas constitucionais 1.2.0/1.3.0 autorizam exclusivamente `lightningcss@1.33.0`, `lightningcss-linux-x64-gnu@1.33.0`, `caniuse-lite@1.0.30001823` e, historicamente, `Microsoft.Testing.Extensions.CodeCoverage@18.12.0` como ferramentas de build/teste. CodeCoverage foi removido do lock ao substituir MSTest por xUnit; sua aprovação constitucional permanece registrada, mas não representa uma dependência atual. Nenhuma outra dependência restritiva ou sem licença verificada está aprovada; variantes de outra plataforma de `lightningcss` no lock não estão autorizadas.
- As referências de imagens continuam fixadas por digest e plataforma em `deploy/images.lock.json`; isso não autoriza publicação ou redistribuição de imagens.
- Licença ausente, ambígua, paga, restritiva ou incompatível continua bloqueando o uso, exceto os três itens explicitamente aprovados, nas versões/integridades fixadas. Não inferir licença transitiva pela licença do pacote pai.
- Aceites de termos de ferramentas Android permanecem humanos e são requisito antes de uso; as ferramentas só podem executar em containers e não são distribuídas com a aplicação.

## Política de imagens e SBOMs

Este repositório não mantém SBOMs como requisito geral de uso local ou self-hosted. O inventário `deploy/images.lock.json` conserva tags, origem, licença declarada, notices disponíveis e digests por plataforma para reprodutibilidade; ele não representa uma auditoria completa das camadas da imagem.

A ausência de SBOM versionado não remove avisos nem obrigações de licenças de terceiros. Antes de publicar ou redistribuir uma imagem, devem ser verificadas as licenças aplicáveis e preservados os avisos exigidos. Uma SBOM pode ser gerada fora do repositório quando necessária a uma operação ou distribuição específica.

## Resultado da resolução isolada de T005 — 2026-11-09

Os lockfiles foram resolvidos em containers fixados por digest, sem secrets, scripts npm, build ou testes. A geração do lock npm usou Node 24.22.2 com npm 11.19.1 e `--package-lock-only --ignore-scripts`; depois, `npm ci --ignore-scripts --no-audit --fund=false` instalou os pacotes, e `npm ls --depth=0 --omit=optional` confirmou os 18 pacotes diretos. Os restores NuGet usaram o SDK 10.0.403, `backend/NuGet.Config` e `--locked-mode`, restaurando API e testes sem alterações nos locks. Os arquivos registram integridades SHA-512 npm e `contentHash` NuGet.

**Exceções e limites registrados:**

- `frontend/web/package-lock.json` contém 477 pacotes, todos com integridade e licença declarada. `lightningcss@1.33.0` é dependência de build de `vite@8.3.5` e declara MPL-2.0; `lightningcss-linux-x64-gnu@1.33.0` é a variante opcional baixada em Linux x64 glibc e declara MPL-2.0; `caniuse-lite@1.0.30001823` declara CC-BY-4.0. Os três estão autorizados somente como ferramentas/dados de build, nas integridades fixadas pelas constituições 1.2.0/1.3.0; preservar avisos e atribuição aplicáveis. As variantes nativas para outras plataformas no lock não foram autorizadas.
- O lock histórico de MSTest incluía `Microsoft.Testing.Extensions.CodeCoverage@18.12.0`; sua licença exata não foi confirmada e a emenda constitucional autorizava apenas download/uso como ferramenta, sem afirmar compatibilidade jurídica nem redistribuição. O pacote deixou de ser dependência após a migração para xUnit.
- As demais licenças NuGet consultadas eram PostgreSQL para Npgsql e MIT para os outros pacotes listados. Isso não substitui auditoria posterior de avisos e conteúdo de imagens; inventários de camadas dos runners permanecem pendentes.

**Estado histórico**: T005 foi concluída em 2026-11-09: manifests Angular criados, dependências baixadas somente em container e locks validados em modo travado. Àquela data, nenhum build/teste da aplicação havia sido executado. A emenda 1.2.0 autoriza somente os artefatos enumerados nela; isso não cria requisito de SBOM.

## Atualização da feature 002 — 2026-11-11

`xunit.v3` 4.0.3 e suas dependências foram resolvidos com NuGet `--locked-mode` em container. Os nuspecs oficiais declaram Apache-2.0 para xUnit e MIT para os pacotes MTP/transitivos Microsoft conferidos. O build/formato passaram; `Category=Unit` passou 2/2, `Category=Integration` passou 15/15 e a suíte completa passou 17/17 pelo runner integrado. Os avisos xUnit1051 permanecem como warnings apenas no projeto de testes.

**Perfis oficiais atualizados**: `quality` e `unit` passaram após T014 e a formatação dos quatro arquivos TypeScript. `integration` ultrapassa a validação de imagens fixadas, mas permanece bloqueado antes de ler secrets pela ausência de `deploy/Dockerfile.api`, prevista em T067 da feature `001-garagem-pessoal`.

## Auditoria de preparação da feature 003 — 2026-11-11

- **Código próprio Altivy proveniente do Dokpod**: o repositório de referência contém o texto GNU AGPL versão 3 em `LICENSE`; os arquivos de origem estão em `frontend/web/src/app/design-system/`. Após o bloqueio inicial, o CEO da Altivy e owner do GarageMotor confirmou expressamente a titularidade Altivy e autorizou a cópia dos componentes nesta conversa em 2026-11-11, conforme `specs/003-garagemotor-design-system/tasks.md`. A inspeção dos dez arquivos abaixo foi executada em container, com parser TypeScript: imports limitados a Angular e helpers internos. As adaptações preservam atribuição em `NOTICE.md` e usam apenas dependências já fixadas. Não houve incorporação de marca Dokpod, edição Business, CSS global ou código de módulos de domínio. A permissão não representa aprovação de biblioteca externa copyleft ou código de terceiros.
- **Outfit**: fonte e licença copiadas sem alteração para `frontend/web/src/assets/fonts/`. Origem: pacote `docs/brand/fonts/` do repositório Altivy; versão documentada Google Fonts v15, SIL OFL 1.1. O copyright dos Outfit Project Authors e `LICENSE-Outfit-1.1.txt` foram preservados integralmente. SHA-256 da fonte: `6c18d579fd87c3776be068b762cbc83fde3acb543d49eabd3ade842eb987e887`; da licença: `c676351bf8576b9aba743cd5eaa8c0e7ee0d51f805d720447b4df4ddb6a2e416`. Os dois hashes foram verificados com sucesso em container Node fixado pelo lock de imagens. A licença da fonte permanece separada da licença do código GarageMotor. A cópia no build Angular foi comparada byte a byte e o carregamento local da fonte passou em Playwright pelo nginx estático; sem CDN.

### Proveniência dos componentes adaptados

Origem: diretório `frontend/web/src/app/design-system/` do repositório Dokpod. Destino: diretório homônimo no GarageMotor, com prefixos `gm-*` e classes `Gm*`. Os hashes identificam os arquivos de origem inspecionados, não igualdade com as versões adaptadas.

| Arquivo de origem | SHA-256 |
|---|---|
| `button/dok-button.ts` | `42b978ed34009094463eb91a362f14cdc793bbebc44e7b81049eca3d2d55ed9c` |
| `field/dok-field.ts` | `2a29f18116a73448fa77b3615906a46da434e8b499a13bfc9dcaa3a8f77f220d` |
| `select/dok-select.ts` | `dc1c047223f03841998f91dade35f3cebe54f7e47cca606cac3932f1fe7f6818` |
| `checkbox/dok-checkbox.ts` | `9cf87d0686dc6bef7bd210510db7d8a7cfe7ec2e784f5bdc9a27b8f28d7c483c` |
| `card/dok-card.ts` | `dec45ffad1b5efe814ca4e6c09a1e93b6c857bb14def3f51e14dc08ba94c5ad7` |
| `tabs/dok-tabs.ts` | `dd2399a7d8181340b67a6d6e89f634a061ad0a713639a5d0b7d1a0930cf64b63` |
| `status/dok-status.ts` | `d33e31adbd3d4e80584facc3fd8511710916cbd58ddbbb9a666f8d22feab9c09` |
| `toast/dok-toast.ts` | `aa4be2bb3d5b70f0ac93340f6f36bcec8e426630812beb8c452fd05e1edeaec3` |
| `empty-state/dok-empty-state.ts` | `14a89b33d4913f8a2dab7076bd8fada21214eb10f49253641252b8a030149aff` |
| `tone.ts` | `0ee83cf03ee8bf739fdde7f4d7fe5d8f3e3a4f98e6f3a2cfd934739af5551cd3` |

Adaptações: ícones e CSS semânticos GarageMotor; campos preservam tipo, autocomplete e limites; seleção usa `select` nativo com CVA; checkbox mantém o estado nativo sincronizado no reset; abas mantêm foco itinerante e ignoram opções indisponíveis; toast pausa por hover/foco, limpa timers na destruição e não fecha erros automaticamente. O fixture de testes é compilado separadamente e não integra o bundle da aplicação.

### Ativos originais GarageMotor

Símbolos claro/escuro, wordmarks e app-icon em `frontend/web/src/assets/brand/` e paths em `frontend/web/src/app/design-system/icon/gm-icon.ts` foram criados originalmente por IA assistida para GarageMotor, sem material de marca de terceiros. O manifesto `SHA256SUMS` registra os SVGs. A inspeção automatizada verifica ausência de scripts, imagens, imports e referências externas. Os wordmarks SVG usam texto com fallback `sans-serif`, sem incorporar ou buscar fonte externa; a shell compõe a marca com o símbolo e Outfit local. O owner escolheu a versão "Atual em azul" em 2026-11-11, conforme decisão registrada em `specs/003-garagemotor-design-system/tasks.md`; essa escolha não equivale a busca de anterioridade ou registro de marca.

### Ajuste Azul e Propostas Triangulares — 2026-11-11

Por decisão humana explícita na mesma conversa, o azul Altivy `#2563EB` substitui o verde como cor de interface. O tema escuro usa `#93C5FD` para texto/ações de acento legíveis; verde permanece apenas no sucesso semântico. Os tokens antigos `--gm-forest`, `--gm-forest-dark` e `--gm-lime` são aliases dos novos tokens primários, evitando quebra dos componentes existentes.

Foram criados cinco SVGs azuis da geometria anterior (`symbol-blue-*`, `wordmark-blue-*`, `app-icon-blue.svg`) sem alterar os cinco assets anteriores. O preview usa apenas essa recoloração, não uma geometria nova. Quatro propostas em `frontend/web/src/assets/brand/proposals/` adaptam, sob solicitação expressa do owner, o triângulo invertido vazado de `Altivy/docs/brand/assets/dist/symbol.svg`: path `M31 52 L48 72 L65 52 Z M39 52 H57 L48 64 Z`, preenchimento `#2563EB`, regra `evenodd`, transformado por escala/translação. Não foram copiadas as pernas do A, sua base ou o logotipo completo. Atribuição Altivy preservada em `NOTICE.md`. Os novos nove SVGs também constam no manifesto de checksums.

O owner escolheu "Atual em azul": `symbol-blue-*`, `wordmark-blue-*` e `app-icon-blue.svg` fora de `proposals/`, sem o triângulo Altivy. As propostas A (G aberto) e B (G conectado) não foram escolhidas e ficam preservadas para referência histórica. A galeria local `assets/brand/proposals/` registra a seleção; não é uma landing page do produto. Não houve inclusão de dependência ou recurso remoto em runtime. Esta decisão não altera os gates de integração ou Android.
