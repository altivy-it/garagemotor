# Tarefas: Design system e identidade GarageMotor

**Entrada**: Documentos de design em `specs/003-garagemotor-design-system/`

> Redija todas as tarefas e descrições em português brasileiro (pt-BR), preservando identificadores, comandos e caminhos técnicos.

**Pré-requisitos**: `spec.md`, `plan.md`, `research.md`, `data-model.md` e `quickstart.md`.

**Autorização humana**: em 2026-11-11, o CEO da Altivy e owner do GarageMotor confirmou nesta conversa: "Sim, esta revisado e autorizado, siga com o plano". A autorização abrange as tarefas revisadas e a correção da ordem test-first; não autoriza publicação, deploy ou mudança da política de licenças.

**Autorização expressa de reuso em 2026-11-11**: o mesmo revisor humano esclareceu nesta conversa: "PRojeto dokpod é da mesma empresa altivy, por isso esse projeto pode copiar os componentes sem problema, esta autorizado". O reuso fica restrito ao código próprio Altivy com proveniência conferida, incorporado ao código próprio GarageMotor, preservando autoria e avisos. Essa permissão expressa resolve o bloqueio anterior de titularidade; não decorre automaticamente da licença AGPL nem da origem na mesma empresa. Não abrange código de terceiros, marcas Dokpod, edição Business ou dependências externas não aprovadas.

**Continuidade autorizada em 2026-11-11**: o owner verificou manualmente seletor e tela inicial e solicitou nesta conversa: "ok, por hora ja verifiquei que o seletor de tema e a tela inicial esta funcionando, siga com o plano". Prosseguir nas histórias independentes após a fundação; manter T010 e os demais gates indisponíveis desmarcados. A verificação humana do preview não substitui testes integrados nem autoriza a T067 da feature 001 implicitamente.

**Continuidade autorizada em 2026-11-12**: o owner aprovou as recomendações da análise nesta conversa: "Suas recomendações estão aprovadas, siga com spec". O escopo compreende os três ajustes documentais propostos e a retomada de T010 e T026; não autoriza executar T015, T016 ou T028, a T067 da feature 001, publicação ou deploy.

**Estado consolidado em 2026-11-12**: conforme o [resultado integrado posterior](quickstart.md#resultado-integrado-posterior), T010, T026 e T027 estão concluídas para as validações registradas. T015, T016 e T028 permanecem abertas por dependerem da entrega Android da feature 001; o aceite humano do SDK já foi registrado. Os registros históricos abaixo preservam bloqueios e estados conhecidos em cada momento, não o estado consolidado.

**Testes**: esta mudança altera comportamento e fluxo de usuário; incluir testes Playwright e verificações de build/acessibilidade aplicáveis. Não adicionar runner unitário frontend sem necessidade e auditoria de dependência.

## Formato

- **[P]** identifica tarefas em arquivos distintos sem dependência mútua.
- **[US1]**, **[US2]** e **[US3]** identificam as histórias correspondentes em `spec.md`.
- Todas as tarefas indicam os caminhos exatos.

## Fase 1: Preparação

**Objetivo**: confirmar proveniência, avisos e distribuição local de assets necessários às histórias.

- [X] T001 [P] Registrar em `docs/dependencies.md` a proveniência/licença AGPL-3.0 dos arquivos Dokpod selecionados e os avisos que devem acompanhar a reutilização.
- [X] T002 [P] Copiar Outfit, `LICENSE-Outfit-1.1.txt` e checksum verificado para `frontend/web/src/assets/fonts/`, validando os hashes registrados em `Altivy/docs/brand/fonts/SHA256SUMS`.
- [X] T003 Configurar em `frontend/web/angular.json` a cópia explícita de `src/assets/` com `input: "src/assets"`, `glob: "**/*"` e `output: "assets"`, e conferir que os caminhos resultantes são locais.

**Evidência de execução em 2026-11-11**: T001 registrou inicialmente a auditoria e o bloqueio; a autorização expressa de reuso de código próprio Altivy acima permite retomar a abordagem, sujeita à conferência de proveniência dos arquivos. T002 copiou somente a fonte e sua licença e verificou os dois hashes com `sha256sum --check SHA256SUMS` no runner Node fixado por digest em container; ambos retornaram `OK`. Nenhum componente Dokpod foi incorporado até esta atualização.

**Retomada após autorização expressa**: T003 passou no build de produção Angular em container, com fonte e licença comparadas byte a byte no pacote. T004-T009 foram executadas e validadas: 34 pares semânticos de contraste passaram; 16 testes de tema e 3 testes preexistentes de acessibilidade passaram pelo nginx estático em container; `quality` oficial passou. T010 continua pendente: o perfil `web-e2e` oficial retornou `blocked` (exit 2) por ausência de `deploy/Dockerfile.api` e `deploy/Dockerfile.web`, da T067 da feature 001. Não executar essa tarefa de outra feature implicitamente nem tratar o recorte estático como aprovação do ponta a ponta. Detalhes e limites em `quickstart.md`.

**Execução após verificação humana do preview**: T011-T014 e T017-T025 foram implementadas e validadas no recorte web. Marca e autenticação renderizam com identidade GarageMotor original; primitives foram adaptadas do código próprio Altivy/Dokpod com atribuição, hashes de origem e imports auditados. `quality` oficial passou; 37 testes de aparência passaram via nginx em container (16 tema, 4 marca, 8 primitives, 6 acessibilidade e 3 preexistentes). Os testes de acessibilidade também passaram com toque e movimento reduzido. O fixture Angular tem build separado e não integra o bundle de produção. Capturas desktop/móvel foram revisadas. T010/T016/T027 permanecem bloqueadas pelos Dockerfiles; T015/T028 dependem do projeto/gate Android. T026 permanece parcial: formulário, foco e movimento reduzido validados; sessão real ainda depende de backend. A feature não está concluída.

## Fase 2: Fundação

**Objetivo**: criar a base visual compartilhada sem alterar jornadas ainda.

- [X] T004 Definir tokens semânticos base, tipografia Outfit, foco e movimento reduzido em `frontend/web/src/app/design-system/tokens.css` e `frontend/web/src/styles.css`, mantendo a paleta GarageMotor e raio de controle máximo de 8px.
- [X] T005 Implementar tokens para temas Claro e Escuro em `frontend/web/src/app/design-system/tokens.css`, garantindo pares de texto com contraste mínimo 4,5:1 e controles/foco com mínimo 3:1.

**Checkpoint**: assets/fontes são locais e o CSS consegue representar ambos os temas; nenhuma história pode começar antes desta fase.

## Fase 3: História de Usuário 1 - Usar GarageMotor no tema escolhido (Prioridade: P1, MVP)

**Objetivo**: escolher Claro, Escuro ou Sistema em qualquer jornada e persistir a preferência sem afetar sessão ou formulário.

**Teste independente**: Playwright seleciona as três opções, recarrega a aplicação, muda `prefers-color-scheme` durante Sistema e confirma seleção, aparência e armazenamento em viewport desktop e móvel.

### Testes da História de Usuário 1

- [X] T006 [P] [US1] Criar a jornada Playwright de seleção/persistência/mudança do sistema em `frontend/tests/design-system/theme.spec.ts`, incluindo fallback, valor inválido e indisponibilidade do armazenamento.

### Implementação da História de Usuário 1

- [X] T007 [US1] Implementar preferência `light | dark | system`, chave `gm.theme.v1`, fallback efetivo Claro, atualização por `matchMedia` e inicialização sem flash em `frontend/web/src/app/core/theme.service.ts` e `frontend/web/src/main.ts`.
- [X] T008 [US1] Criar seletor de tema acessível por teclado/toque e com estado selecionado anunciado em `frontend/web/src/app/design-system/theme-picker.component.ts`.
- [X] T009 [US1] Integrar `ThemePicker` à shell e às jornadas autenticada/não autenticada em `frontend/web/src/app/app.component.ts` e `frontend/web/src/app/features/auth/auth.component.ts`, sem reinicializar sessão ou formulário ao trocar tema.
- [X] T010 [US1] Executar `pwsh -NoProfile -File tools/scripts/validate-feature.ps1 -FeatureDirectory specs/003-garagemotor-design-system -Profile web-e2e` para validar T006-T009 pela borda nginx em container. Em 2026-11-12, o perfil oficial passou (exit 0), sem expor logs ou secrets.

**Checkpoint**: tema e persistência podem ser demonstrados isoladamente; preferência explícita não muda quando o sistema operacional alterna.

## Fase 4: História de Usuário 2 - Reconhecer a identidade GarageMotor (Prioridade: P1)

**Objetivo**: aplicar identidade original em marca, autenticação, navegação, favicon e ícone de app, sem reutilizar marcas de referência.

**Teste independente**: Playwright confere a presença/legibilidade dos ativos em claro/escuro e desktop/móvel; inspeção confirma ausência de referências remotas ou logos Altivy/Dokpod.

### Testes da História de Usuário 2

- [X] T011 [P] [US2] Criar verificações Playwright de presença, carregamento local e variantes de tema dos ativos em `frontend/tests/design-system/brand.spec.ts`.

### Implementação da História de Usuário 2

- [X] T012 [P] [US2] Criar wordmark, símbolo GarageMotor e SVG de favicon/app-icon originais em `frontend/web/src/assets/brand/`, com versões legíveis em superfícies claras e escuras.
- [X] T013 [P] [US2] Criar componente `GmIcon` com paths SVG originais e nomes acessíveis em `frontend/web/src/app/design-system/icon/gm-icon.ts`, incluindo ícones necessários à navegação e aos controles compartilhados.
- [X] T014 [US2] Integrar wordmark, símbolo e favicon à shell, autenticação e metadados web em `frontend/web/src/app/app.component.ts`, `frontend/web/src/app/features/auth/auth.component.ts` e `frontend/web/src/index.html`.
- [ ] T015 [US2] Gerar e integrar recursos de launcher Android a partir do app-icon original em `frontend/web/android/app/src/main/res/`, somente após a T067 da feature 001 criar o projeto e o gate de aceite Android estar liberado.
- [ ] T016 [US2] Executar a jornada da T011 no perfil `web-e2e` e inspecionar os recursos Android pelo perfil `android`; se qualquer gate estiver pendente, registrar o bloqueio e não contorná-lo.

**Checkpoint**: marca web original e assets locais passam nos dois temas; a verificação de launcher Android só fecha quando o projeto nativo existir e o gate estiver autorizado.

## Fase 5: História de Usuário 3 - Manter experiências consistentes (Prioridade: P1)

**Objetivo**: oferecer primitives compartilhadas com semântica, aparência e acessibilidade previsíveis nas telas web e Android.

**Teste independente**: Playwright exercita controles por teclado/toque e leitores de estado; verifica contraste, foco, desabilitado, loading, erro, vazio e layout estreito em ambos os temas.

### Testes da História de Usuário 3

- [X] T017 [P] [US3] Criar testes Playwright de semântica, teclado, foco e estados de ação/formulário em `frontend/tests/design-system/primitives.spec.ts`.
- [X] T018 [P] [US3] Criar verificações Playwright de contraste AA, nomes acessíveis, status não dependente de cor e redução de movimento em `frontend/tests/design-system/accessibility.spec.ts`.

### Implementação da História de Usuário 3

- [X] T019 [P] [US3] Adaptar as primitives Dokpod button, field, select e checkbox para Angular standalone, tokens semânticos e `GmIcon` em `frontend/web/src/app/design-system/button/`, `field/`, `select/` e `checkbox/`, preservando avisos e removendo cores hardcoded.
- [X] T020 [P] [US3] Adaptar card e tabs do Dokpod para superfícies e navegação GarageMotor em `frontend/web/src/app/design-system/card/` e `tabs/`, sem portar shell ou CSS global Dokpod.
- [X] T021 [P] [US3] Adaptar status, toast e empty/loading state do Dokpod em `frontend/web/src/app/design-system/status/`, `toast/` e `empty-state/`, com texto/ícone além da cor.
- [X] T022 [US3] Consolidar estilos e exports públicos das primitives GarageMotor em `frontend/web/src/app/design-system/primitives.css` e `frontend/web/src/app/design-system/index.ts`, removendo classes/token names do Dokpod que vazem da API visual.
- [X] T023 [US3] Migrar campos, ações, feedback e estados da autenticação para as primitives em `frontend/web/src/app/features/auth/auth.component.ts` e `frontend/web/src/styles.css`, preservando validação e estado funcional.
- [X] T024 [US3] Validar navegação por teclado, toque, responsividade e ausência de sobreposição em `frontend/tests/design-system/primitives.spec.ts` e `frontend/tests/design-system/accessibility.spec.ts` nos viewports desktop e móvel.

**Checkpoint**: primitives da primeira superfície passam por acessibilidade e funcionam sem depender da marca ou tema Dokpod.

## Fase 6: Refinamento e aspectos transversais

**Objetivo**: fechar rastreabilidade, qualidade e compatibilidade sem ampliar o escopo.

- [X] T025 [P] Atualizar `docs/dependencies.md` e revisar `frontend/web/src/assets/fonts/` e `frontend/web/src/assets/brand/` para conferir licenças, checksums, proveniência e ausência de referências remotas.
- [X] T026 [P] Confirmar que testes e shell preservam estados de sessão/formulário, movimento reduzido e foco durante troca de tema em `frontend/tests/design-system/theme.spec.ts` e `frontend/tests/design-system/accessibility.spec.ts`. Validação em 2026-11-12: 16 testes de tema e três jornadas de sessão real no runner compartilhado, com preservação do formulário autenticado e foco; seis testes de acessibilidade passaram em Chromium e Firefox no fixture isolado, incluindo movimento reduzido.
- [X] T027 Executar `pwsh -NoProfile -File tools/scripts/validate-feature.ps1 -Profile quality` e `-Profile web-e2e`, registrar resultados por perfil e bloquear qualquer tentativa de contornar pré-requisitos em `specs/003-garagemotor-design-system/quickstart.md`. Ambos passaram em 2026-11-11 com seleção explícita desta feature; web-e2e agora cobre quatro contratos e duas jornadas reais, não a suíte completa de aparência nem Android.
- [ ] T028 Executar `pwsh -NoProfile -File tools/scripts/validate-feature.ps1 -Profile android` e registrar build/smoke somente depois que `frontend/web/android`, `tools/scripts/test-android.ps1` e aceite humano estiverem presentes; manter explícito qualquer bloqueio em `specs/003-garagemotor-design-system/quickstart.md`.

## Dependências e Ordem de Execução

### Dependências entre fases

- **Preparação**: T001 e T002 podem ser iniciadas em paralelo; T003 depende da definição dos assets locais.
- **Fundação**: depende da Preparação e bloqueia as histórias.
- **US1**: depende da Fundação; é o MVP de funcionalidade.
- **US2**: depende da Fundação; T015/T016 também dependem da entrega T067 da feature 001 e do gate Android humano.
- **US3**: depende da Fundação, de T013 (`GmIcon`) e dos tokens; sua migração de autenticação também depende da integração do seletor em US1.
- **Refinamento**: depende das histórias web concluídas; validação Android continua condicionada aos gates definidos.

### Oportunidades de paralelismo

- Após Fundação: US1 e US2 podem avançar em paralelo, desde que os testes usem arquivos distintos.
- Em US2: T012 (ativos de marca) e T013 (componente de ícones) podem avançar em paralelo.
- Em US3: escrever T017/T018 primeiro e executar os testes para observar a falha pelo comportamento ausente. Somente depois dessa evidência e da conferência de proveniência, executar T019/T020/T021 em paralelo em suas pastas distintas. T022 integra os exports depois.
- Não paralelizar tarefas que editam `app.component.ts`, `auth.component.ts` ou `styles.css` ao mesmo tempo.

## Estratégia de Implementação

### MVP primeiro

1. Concluir Preparação e Fundação.
2. Executar US1: seletor, persistência e adaptação ao sistema.
3. Validar o MVP em `web-e2e` atrás de nginx, quando o perfil estiver disponível.
4. Prosseguir para identidade e primitives somente após revisão e autorização das tarefas correspondentes.

### Entrega incremental

1. Adicionar identidade original em US2; integrar o launcher apenas com Android shell e gate liberados.
2. Adicionar primitives em US3 e migrar auth sem regressões de sessão.
3. Concluir quality, Playwright web e Android autorizado, mantendo cada bloqueio visível.

## Rastreabilidade

- **FR-001–FR-004 / SC-001–SC-002**: T004-T010, T026-T028.
- **FR-005 / SC-003 / FR-009**: T001-T003, T011-T016, T025, T028.
- **FR-006–FR-008 / SC-004–SC-005**: T004-T005, T017-T024, T027-T028.
- **FR-010**: T001, T019-T021, T025.
- **SC-006**: T002-T003, T011-T016, T025, T027-T028.

## Fase 7: Ajuste Visual Azul Solicitado Pelo Owner

**Autorização**: solicitações humanas nesta conversa em 2026-11-11 para novos arquivos de marca com o motivo triangular Altivy e para "Use o azul como cor geral do tema no lugar do verde". Não requer nova feature nem autoriza executar os gates externos pendentes.

- [X] T029 Aplicar paleta azul Claro/Escuro e aliases compatíveis em `frontend/web/src/app/design-system/tokens.css`, preservando semântica dos estados e validando contraste.
- [X] T030 Criar versões azuis e propostas separadas com triângulo invertido em `frontend/web/src/assets/brand/`, preservando os assets anteriores; integrar somente a recoloração da geometria atual em `frontend/web/src/app/app.component.ts` e `frontend/web/src/index.html` e deixar as propostas para escolha humana.
- [X] T031 Atualizar `frontend/tests/design-system/brand.spec.ts`, validar regressão de tema/contraste/responsividade em containers via nginx e registrar proveniência/checksums e resultados em `docs/dependencies.md` e `specs/003-garagemotor-design-system/quickstart.md`.

**Evidência do ajuste azul**: build Angular e `quality` oficial passaram; 34 pares semânticos passaram no cálculo de contraste; 39 testes Playwright passaram em containers via nginx, incluindo seis de marca/galeria e seis combinações de acessibilidade tema/viewport. Capturas do produto e das propostas revisadas em desktop/móvel. Os 14 checksums SVG passaram, confirmando também preservação dos cinco assets anteriores. A primeira tentativa de teste usou um transform antigo do runner; a execução foi repetida com `PWTEST_CACHE_DIR` novo e confirmou os seis testes atuais de marca. Naquele momento, as propostas triangulares aguardavam escolha humana e T010/T015/T016/T026/T027/T028 permaneciam pendentes, sem alteração dos gates externos; as atualizações posteriores estão registradas nesta seção e no estado consolidado acima.

**Decisão humana de marca em 2026-11-11**: após a validação acima, o CEO da Altivy e owner do GarageMotor declarou nesta conversa: "Escolhido foi: Atual em azul". Origem: humano; registro IA assistida. A identidade selecionada é a geometria GarageMotor anterior recolorida em azul: `symbol-blue-light.svg`, `symbol-blue-dark.svg`, `wordmark-blue-light.svg`, `wordmark-blue-dark.svg` e `app-icon-blue.svg`. Manter os assets já ativos na aplicação; não incorporar o triângulo Altivy à marca escolhida. As propostas A e B passam a alternativas não escolhidas, preservadas como histórico. Essa decisão não aprova a feature inteira, integração, Android, publicação ou deploy.
