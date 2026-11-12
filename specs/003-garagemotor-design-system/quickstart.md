# Guia de Validação: Design system e identidade GarageMotor

Este guia acompanha as tarefas revisadas e autorizadas explicitamente pelo owner. Ele não instala dependências nem inicia serviços fora dos containers previstos pelo projeto.

## Execução parcial em 2026-11-11

O owner revisou e autorizou o plano nesta conversa. A auditoria T001 inicialmente interrompeu a execução por dúvida sobre reuso Dokpod; em seguida, o owner confirmou expressamente a titularidade Altivy e autorizou copiar os componentes, conforme `tasks.md`. Na primeira etapa, isso liberou a retomada com inspeção de proveniência e imports, limitada ao código próprio Altivy; dependências externas permaneceram sujeitas à política permissiva. No encerramento daquela preparação inicial ainda não havia cópia de componentes nem alteração de comportamento da aplicação.

T002 foi validada com `sha256sum --check SHA256SUMS` em container Node fixado por digest, no diretório de fontes; fonte e licença retornaram `OK`. Naquela preparação inicial ainda não haviam sido executados build Angular, Playwright, contraste, screenshots ou Android. Os resultados das etapas posteriores estão registrados abaixo; diagnósticos de documentos não equivalem a testes de funcionamento.

### Resultados após retomada autorizada

- T003: build de produção Angular em container passou; fonte e licença no pacote foram comparadas byte a byte com os arquivos de origem.
- T004-T005: 34 pares de cores semânticas Claro/Escuro passaram no cálculo de contraste em container (4,5:1 para texto; 3:1 para bordas/foco aplicáveis). Isso não representa auditoria AA integral das futuras primitives.
- T006-T009: preferência local Claro/Escuro/Sistema e seletor global implementados. Playwright passou 16 testes novos de tema nos viewports 1280x900 e 390x844 e 3 testes existentes de acessibilidade; fonte local, persistência, mudança do sistema, fallback, armazenamento indisponível e preservação do formulário foram verificados. Quatro capturas, uma por tema/viewport, foram revisadas sem sobreposição.
- O frontend estático foi servido por nginx em container, em rede isolada; `/api/` retorna 503. Não houve backend, banco, autenticação real nem teste nativo Android nesse recorte. Preview local: `http://127.0.0.1:18083` enquanto o container `garagemotor-design-003-nginx` estiver ativo.
- A tentativa inicial de teste vermelho foi descartada: o proxy do runner direcionou a navegação a uma página externa, não ao GarageMotor. Essa tentativa não comprova falha anterior da aplicação. Os testes aprovados usaram os proxies de saída desabilitados no runner e `NO_PROXY` restrito aos hosts locais, sem registrar configurações do ambiente. A evidência de teste vermelho válido antes da implementação não foi obtida.
- Perfil oficial `quality`: `passed`, exit 0, com `-FeatureDirectory specs/003-garagemotor-design-system -Engine podman`.
- Perfil oficial `web-e2e`: `blocked`, exit 2, por ausência de `deploy/Dockerfile.api` e `deploy/Dockerfile.web` (T067 da feature 001). T010 permanece desmarcada; a execução sequencial foi interrompida nesse gate. As fases de identidade, incorporação das primitives Dokpod e Android ainda não foram executadas. A permissão de reuso está registrada; o bloqueio atual não é de titularidade.

## Continuidade Após Verificação Humana

### Resultado da continuidade autorizada

O owner confirmou o funcionamento do preview e solicitou continuidade nesta conversa em 2026-11-11. As histórias web independentes avançaram sem remover os gates integrados:

- T011-T014: cinco assets SVG originais GarageMotor, `GmIcon`, símbolo sensível ao tema e favicon implementados. O teste de marca foi escrito antes da implementação e falhou pelo símbolo ausente; depois passou. Os cinco hashes em `src/assets/brand/SHA256SUMS` passaram em container.
- T017-T025: ações, campos, seleção, checkbox, superfícies, abas, status, toast e estados vazios/carregamento adaptados do código próprio Altivy/Dokpod. Hashes de origem e atribuição em `docs/dependencies.md` e `NOTICE.md`; nenhum pacote foi adicionado.
- A autenticação usa as primitives, mantendo validação, autocomplete e recuperação. O retorno de chamadas assíncronas notifica a detecção de mudanças para liberar loading e renderizar feedback. A API real do preview retorna 503: o teste verifica erro textual, limpeza de senha e ausência de sessão bem-sucedida, sem simular backend funcional.
- `quality` oficial: `passed`, exit 0. Suíte isolada: 37 testes passaram (16 tema, 4 marca, 8 primitives, 6 acessibilidade, 3 existentes); a versão final dos seis testes de acessibilidade também passou com toque e verificação de animação zerada sob movimento reduzido.
- Capturas de marca e controles foram revisadas em desktop/móvel, com os controles também exercitados em 320 px. A revisão não equivale à aprovação humana final da proposta de marca nem a certificação completa WCAG.
- `web-e2e` oficial foi repetido e retornou `blocked`, exit 2, por `deploy/Dockerfile.api` e `deploy/Dockerfile.web`. T010/T016/T027 continuam pendentes. T015/T028 dependem do projeto nativo e do aceite Android ainda ausentes; nenhum SDK foi baixado ou contrato aceito pelo agente. T026 só tem evidência parcial, pois não houve sessão autenticada real.

### Fixture separado de componentes

O fixture fica em `frontend/tests/design-system/harness.ts`, fora de `src` e sem rota na aplicação de produção. Seus valores são fictícios e não chamam backend. O teste de autenticação acessa a aplicação real na raiz, não o fixture.

No diretório `frontend/web`, dentro do runner Node fixado em `deploy/images.lock.json`, compilar separadamente:

```sh
node node_modules/@angular/cli/bin/ng.js build --configuration development --browser ../tests/design-system/harness.ts --ts-config ../tests/design-system/tsconfig.harness.json --output-path dist/design-system-harness --base-href /__design-system__/
```

Resultado esperado: build em `dist/design-system-harness/browser`, sem alteração do bundle de produção. O nginx temporário monta `frontend/web/dist` como `/usr/share/nginx/html`; a raiz usa `/usr/share/nginx/html/garagemotor-web/browser` e o caminho `/__design-system__/` usa alias para `/usr/share/nginx/html/design-system-harness/browser/`. `/api/` continua retornando 503. Essa configuração é exclusiva da validação isolada, não substitui o deployment pendente da T067.

Com aplicação e fixture disponíveis somente pelo nginx, executar no runner Playwright fixado, no mesmo diretório web:

```sh
node node_modules/@playwright/test/cli.js test --config ../tests/playwright.config.ts design-system/ design-system.spec.ts --workers=1
```

Pré-requisitos do runner: `GARAGEMOTOR_BASE_URL=http://nginx:8080`, mesma rede do proxy e saída corporativa excluída somente para hosts locais. Não imprimir variáveis de ambiente nem secrets. Preview do produto: `http://127.0.0.1:18083`; fixture: `http://127.0.0.1:18083/__design-system__/`. O fixture não deve ser distribuído com o app.

### Pré-requisitos dos perfis oficiais

- Podman e os runners/serviços fixados nos manifests do repositório.
- Branch `003-garagemotor-design-system` e tarefas aprovadas.
- Para testes web ponta a ponta: `deploy/Dockerfile.api`, `deploy/Dockerfile.web`, `tools/scripts/start-local.ps1` e `frontend/tests/playwright.config.ts` presentes; acesso do runner e navegador somente via nginx em container.
- Para Android: `frontend/web/android`, `tools/scripts/test-android.ps1` e aceite humano da cadeia Android registrado em `docs/android-toolchain.md`. Não contornar o gate.

## Validações

Na raiz do repositório, validar formatação, build e análise estática do frontend:

```powershell
pwsh -NoProfile -File tools/scripts/validate-feature.ps1 -FeatureDirectory specs/003-garagemotor-design-system -Profile quality -Engine podman
```

Resultado esperado: perfil `quality` concluído com sucesso, incluindo build Angular e verificações de qualidade aplicáveis.

Executar os fluxos web quando os pré-requisitos do perfil estiverem satisfeitos:

```powershell
pwsh -NoProfile -File tools/scripts/validate-feature.ps1 -FeatureDirectory specs/003-garagemotor-design-system -Profile web-e2e -Engine podman
```

Resultado histórico do perfil em 2026-11-11: quatro testes de contratos e duas jornadas reais de cadastro/login/CSRF/códigos offline/logout, incluindo troca Claro/Escuro/Sistema, foco, sessão preservada e ausência de overflow em desktop/móvel. A configuração atual do runner também inclui a suíte de tema; o resultado direto posterior está registrado abaixo e não substitui a execução do perfil oficial. O serviço HTTP e o navegador usam exclusivamente a borda nginx compartilhada em container. Use `-KeepRunning` para manter API/web disponíveis após a execução.

Executar a validação Android somente após o gate estar liberado:

```powershell
pwsh -NoProfile -File tools/scripts/validate-feature.ps1 -FeatureDirectory specs/003-garagemotor-design-system -Profile android -Engine podman
```

Resultado esperado: build/smoke do app Capacitor e conferência do ícone instalado, seletor e persistência após reabertura. Se o perfil for bloqueado por aceite/artefato ausente, registrar o bloqueio; não substituir por build local fora de container.

## Verificações focadas

- O controle de tema é um botão de ícone que identifica a preferência atual no nome acessível/tooltip; o popover usa rádios nativos, setas, Escape com foco restaurado e fechamento por clique externo. O layout não impõe largura mínima global de 320 CSS px; a jornada integrada inclui 276 px após o overflow observado no preview. O resultado dessa última execução deve ser confirmado pelo perfil web-e2e antes de marcar tarefa relacionada.
- Confirmar que `gm.theme.v1` contém somente `light`, `dark` ou `system`, sem tokens, credenciais ou dados da garagem.
- Simular `prefers-color-scheme` mudando enquanto `system` está selecionado; validar alteração visual sem reload e sem gravar modo explícito.
- Validar que as seleções explícitas sobrevivem a reload/reabertura e não mudam quando o sistema operacional alterna.
- Calcular contraste de texto/controles/foco nos dois conjuntos de tokens; mínimo WCAG 2.2 AA: 4,5:1 para texto e 3:1 para componentes/foco aplicável.
- Conferir navegação por Tab/Shift+Tab/Enter/Espaço/Esc, nomes acessíveis e ausência de informação transmitida somente por cor.
- Confirmar que logo, fonte e ícones são servidos de assets locais e que não há requisição remota para recursos visuais.
- Revisar capturas em viewport desktop e móvel sem sobreposição, texto cortado ou ação inacessível.

## Ajuste Azul e Propostas de Marca

Por solicitação humana posterior, em 2026-11-11, o tema de interface usa azul `#2563EB` no claro e `#93C5FD` no escuro, com superfícies neutras. O verde restante é semântico de sucesso. As versões azuis da geometria atual foram ativadas; os cinco SVGs anteriores permanecem sem alterações.

Duas propostas de símbolo, A (G aberto) e B (G conectado), usam o motivo triangular invertido Altivy expressamente solicitado, sem o logotipo completo. São quatro arquivos separados em `frontend/web/src/assets/brand/proposals/`, com versões clara/escura. Nenhuma proposta foi escolhida ou ativada como nova geometria. Comparação no preview: `http://127.0.0.1:18083/assets/brand/proposals/`.

Validação final: `quality` oficial aprovado; 34 pares semânticos de contraste aprovados; 39 testes Playwright de aparência aprovados via nginx, incluindo marca/galeria, controle por toque, foco, persistência de tema e contraste dos controles. Os 14 SVGs do manifesto passaram em `sha256sum --check SHA256SUMS`, incluindo os cinco arquivos antigos. Capturas da aplicação e da galeria revisadas em desktop/móvel sem imagens ausentes ou overflow.

O runner inicialmente reutilizou uma transformação antiga do teste de marca. Ao diagnosticar essa divergência, repetir com `PWTEST_CACHE_DIR` apontando para um diretório novo no container; não alterar expectativas corretas para acomodar cache antigo. Os testes finais foram executados com cache novo e confirmação dos seis casos atuais de marca/galeria. API, sessão real e Android continuam com os gates já documentados; a validação visual não os substitui.

### Escolha Humana da Identidade

Em 2026-11-11, após os resultados acima, o owner declarou "Escolhido foi: Atual em azul", conforme registro em `tasks.md`. A aplicação mantém os assets `symbol-blue-*` e `app-icon-blue.svg` já ativos, com os wordmarks azuis correspondentes. A geometria não recebe o triângulo das propostas A/B. A galeria identifica a opção escolhida e preserva as duas alternativas como não escolhidas. Nenhum SVG ou checksum foi alterado por essa decisão; não há liberação de gates integrados ou Android.

## Integração Web — Atualização da T067

Em 2026-11-11, o owner autorizou separadamente retomar o recorte web da T067 da feature 001. Os Dockerfiles API/web já foram implementados e tiveram builds de desenvolvimento/runtime aprovados. Os pacotes web passaram 6 testes de marca em produção e 6 em desenvolvimento, por nginx em container; o smoke da API comprovou apenas recusa correta da falta de configuração, não funcionamento com banco. `quality` oficial passou.

O bloqueio atual de `web-e2e` não é mais ausência de Dockerfiles: o perfil retorna `blocked`, exit 2, porque o arquivo externo de configuração ainda não existe. O guard também rejeita caminhos não aprovados sem leitura de conteúdo. Nenhum secret foi lido, criado ou sobrescrito, nem banco/volume existente alterado. O preview isolado continua sem API funcional, e T010/T016/T026/T027/T028 não foram marcadas como concluídas por esses smokes. Consulte [instalação local](../../docs/install.md) para pré-condições, comandos e limites. A chave Android e o vault corporativo não são necessários para esse recorte web.

### Restrição Posterior de Infraestrutura Compartilhada

O owner autorizou a pasta/arquivos privados, mas determinou reutilizar PostgreSQL e nginx existentes, seguindo os exemplos Dokpod/architecture-knowledge-base. O Compose agora declara somente API/web em redes externas e o launcher não cria serviços substitutos. A pasta externa foi criada com modo 0700, sem `.env` ou credenciais; dados/rotas compartilhados não foram alterados. O preflight recusou iniciar porque as redes `identity-global`/`identity-client` dos exemplos não estão disponíveis no contexto Podman atual; Docker e outros contextos Podman também não foram encontrados. Antes de gerar conexões e qualificar a integração, confirmar contexto, nomes dos containers/redes e origem pública do gateway e alinhar os runners a essa topologia. Nenhuma tarefa integrada foi concluída por essa adaptação estrutural.

### Resultado Integrado Posterior

Os bloqueios históricos acima foram superados por bootstrap compartilhado e integração expressamente autorizados em 2026-11-11, conforme [instalação local](../../docs/install.md). A aplicação real está em `https://localhost:7443/garagemotor/`; o preview isolado permanece separado. Configuração externa foi criada por processo seguro e exclusivo, banco/roles próprios preparados na instância compartilhada e rota instalada após `nginx -t`. Nenhum valor privado passou pelo modelo ou pelo runner Playwright.

`quality`: `passed`, exit 0. `web-e2e`: `passed`, exit 0, seis testes (quatro contratos estáticos e duas jornadas reais, 1280x900/390x844). Dois testes unitários .NET passaram. TLS foi validado com a CA pública existente, sem bypass; cookies seguros, CSRF inclusive no logout, códigos offline e preservação de sessão/foco durante troca de tema foram verificados. O runner desativa trace/vídeo/capturas e limpa a página antes dos diagnósticos finais. A prontidão do bootstrap público é aguardada de forma limitada antes de preencher credenciais; mutações não têm retry.

T027 registra somente os dois perfis executados. T010 continua pendente de incorporar a suíte completa T006-T009 ao perfil compartilhado; T026 mantém evidência parcial, agora também com sessão real em teste complementar `frontend/tests/integrated/auth-session.spec.ts`, mas sem reexecutar os arquivos originalmente previstos contra essa stack. T015/T016/T028 dependem da entrega Android ainda ausente; o aceite humano do SDK já foi registrado, não é mais o bloqueio. Não declarar feature 003 ou T067 inteira concluídas. SMTP e restauração de sessão após reload também não foram validados.

### Retomada de T010 e T026 em 2026-11-12

A suíte de tema já estava incluída na configuração compartilhada, mas navegava para a raiz do host em vez de respeitar o subcaminho `/garagemotor/`. Em um nginx isolado, os dois casos de Sistema falharam antes da alteração: a raiz retornou 404, enquanto a aplicação no subcaminho retornou 200. Após a correção, os 16 testes de tema passaram no subcaminho com Chromium e Firefox e também na raiz com Chromium. Os seis testes de acessibilidade passaram com Chromium e Firefox no fixture separado.

O runner compartilhado passou a gravar seus resultados em diretório temporário, preservando resultados existentes no repositório; quatro testes de contrato passaram com o repositório montado somente para leitura. O teste de sessão real foi ampliado para conferir também a preservação do formulário autenticado durante a troca de tema, sem exibir seu valor.

Esses resultados usaram containers e nginx isolados, sem API ou sessão real. Na verificação inicial desta retomada, a stack compartilhada estava parada; o perfil `web-e2e` e a alteração no teste de sessão real ainda não haviam sido reexecutados.

Após autorização humana, os containers compartilhados existentes foram iniciados sem recriação e o runner compartilhado foi executado diretamente: 20 dos 23 testes passaram (16 de tema e quatro de contrato); as três jornadas de sessão falharam com HTTP 400 na renovação de códigos offline. Foram criadas três contas fictícias persistentes para o teste. O perfil oficial `web-e2e` não foi executado nesta tentativa.

O endereço do gateway divergia do proxy confiável configurado no container existente da API. Uma consulta pela borda nginx retornou o cookie CSRF sem `Secure`, compatível com a falha observada.

Com nova autorização humana, somente a API foi recriada a partir da imagem local, sem rebuild ou remoção de volumes; o proxy confiável passou a corresponder ao gateway e o cookie CSRF passou a incluir `Secure`. A execução direta do runner compartilhado passou os 23 testes: 16 de tema, quatro de contrato e três jornadas reais de sessão nos viewports 1280, 390 e 276 px. As jornadas confirmaram sessão, formulário e foco durante a troca de tema; três contas fictícias adicionais foram criadas, totalizando seis contas persistentes nas duas tentativas. T026 está concluída com essa evidência e os testes de acessibilidade isolados acima.

T010 permanece aberta: o perfil oficial não foi executado nesta retomada e seu launcher recria API e web, além de executar `npm ci`; a execução direta não substitui esse comando. T015, T016 e T028 continuam dependentes da entrega Android. Por decisão humana, os serviços iniciados para a validação foram mantidos ativos.
