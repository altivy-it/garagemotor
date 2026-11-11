# Pesquisa: Design system e identidade GarageMotor

**Branch**: `003-garagemotor-design-system`  
**Data**: 2026-11-11  
**Origem**: IA assistida a partir da solicitação humana  
**Revisor humano**: CEO da Altivy e owner do GarageMotor

## Decisões

### Tokens e linguagem visual

**Decisão**: manter a paleta própria GarageMotor e adotar o design system Altivy como referência para tokens semânticos, tipografia, foco, densidade de aplicação e comportamento claro/escuro. Implementar os tokens localmente no frontend, sem copiar logo, símbolo ou componentes do kit de landing.

**Justificativa**: a Constituição exige Angular 22 e o design system Altivy. O CSS atual de GarageMotor já define ink/paper/forest/lime/coral e raio de 8px; preservá-lo diferencia a marca. Os tokens Altivy registram os dois temas e pares de contraste, mas seu README restringe validação clara aos componentes de landing; os componentes de produto/app continuam escuros e não foram validados para claro.

**Alternativas consideradas**: importar literalmente a paleta/kit de app Altivy (rejeitada por identidade distinta e falta de validação clara); manter apenas o tema claro atual (rejeitada por FR-001).

**Fontes**: `Altivy/docs/brand/design-system/README.md`, `tokens.json`, `docs/brand/tokens/tokens.css` e `docs/brand/design-system/SOURCE.md` no repositório Altivy. O espelho informa sincronização até 2026-10-25 e distingue componentes de landing de componentes de produto.

### Tipografia

**Decisão**: usar Outfit self-hosted em `frontend/web/src/assets/fonts/`, com pesos disponíveis 400–600 e preservação de `LICENSE-Outfit-1.1.txt` e checksum.

**Justificativa**: Outfit é a tipografia do design system Altivy, tem SIL Open Font License 1.1 e reduz dependência de rede/CSP. O arquivo verificado é `Outfit-Variable-Latin.woff2`; `Altivy/docs/brand/fonts/SHA256SUMS` registra SHA-256 `6c18d579fd87c3776be068b762cbc83fde3acb543d49eabd3ade842eb987e887`. O checksum da licença registrada é `c676351bf8576b9aba743cd5eaa8c0e7ee0d51f805d720447b4df4ddb6a2e416`.

**Alternativas consideradas**: carregar fonte via Google Fonts/CDN (rejeitada por FR-009); manter fallback sem Outfit (rejeitada por coerência com a referência Altivy).

**Fontes**: `Altivy/docs/brand/licenses.md`, `fonts/LICENSE-Outfit-1.1.txt` e `fonts/SHA256SUMS`. Revalidar hashes na tarefa de cópia; não baixar de CDN.

### Primitives de interface

**Atualização na execução, 2026-11-11**: o bloqueio inicial não deve ser resolvido por inferência de compatibilidade AGPL. O owner confirmou expressamente a titularidade Altivy do Dokpod e autorizou a cópia dos componentes nesta conversa, conforme `tasks.md`. O reuso será de código próprio Altivy, preservando proveniência/autoria/avisos e excluindo código de terceiros ou dependências não aprovadas. A permissão não é inferida apenas da origem na mesma empresa nem se estende às marcas.

**Decisão**: reutilizar seletivamente os componentes standalone de button, field, select, checkbox, card, tabs, status, toast e empty/loading state do Dokpod, adaptando a CSS semântica GarageMotor e trocando `DokIcon` pelos ícones originais `gm-icon`. Não importar shell, tema dark-only, CSS global, charts, data-grid ou módulos de domínio.

**Justificativa**: a inspeção dos componentes confirma Angular standalone e dependência apenas de Angular Forms e imports internos; button, field e select têm comportamento nativo/teclado e testes em `design-system.components.spec.ts`. O seletor contém CSS literal teal e classes/tokens Altivy, portanto precisa de adaptação antes de uso no tema claro. `Dokpod/LICENSE` declara GNU AGPL versão 3, compatível com a licença escolhida para o código próprio, sujeito à preservação de autoria, avisos e proveniência.

**Alternativas consideradas**: importar todo o kit (rejeitada por acoplamento, escopo de produto e tema dark-only); adicionar uma biblioteca de componentes/ícones (rejeitada por dependência desnecessária e regra de licença); reescrever sem referência ao código (rejeitada pois duplicaria primitives Angular já testadas).

**Fontes**: `Dokpod/frontend/web/src/app/design-system/index.ts`, `button/dok-button.ts`, `field/dok-field.ts`, `select/dok-select.ts`, `status/dok-status.ts`, `empty-state/dok-empty-state.ts`, `design-system.components.spec.ts` e `Dokpod/LICENSE`. A portabilidade dos arquivos selecionados deve ser confirmada por auditoria de imports e cabeçalhos durante a tarefa; arquivos com proveniência incerta ficam excluídos.

### Tema e persistência

**Decisão**: tema selecionado versionado em `localStorage` (`light`, `dark`, `system`), padrão `system`; tema efetivo segue `prefers-color-scheme`, com fallback `light` se o dispositivo não expuser preferência. Inicializar antes da primeira renderização e observar mudanças de `MediaQueryList` sem substituir a preferência selecionada.

**Justificativa**: a aplicação é uma SPA Angular e o shell Capacitor usa a mesma origem local; `localStorage` persiste entre recargas/reaberturas sem serviço ou dado de domínio. O valor é enum restrito e não contém segredo.

**Alternativas consideradas**: backend/conta (rejeitada por desnecessidade e acoplamento entre usuários/instâncias); apenas CSS media query sem guardar preferência explícita (rejeitada por FR-003); preferência em sessionStorage (rejeitada por não sobreviver à reabertura).

### Assets de marca e ícones

**Decisão**: desenhar símbolos, wordmark, favicon/ícone de app e paths de ícones originais do GarageMotor em SVG local, com versão adaptável a superfícies claras e escuras. Não usar os logos Altivy ou Dokpod.

**Justificativa**: as marcas são distintas e seus direitos/ativos não acompanham automaticamente a licença de código. SVG permite distribuir e testar ativos sem biblioteca nova; deve ser validado contra scripts e referências remotas.

**Alternativas consideradas**: usar o símbolo Altivy ou Dokpod (rejeitada por FR-005/FR-010); ícones de biblioteca adicional (rejeitada por escopo e auditoria de dependências); Unicode/emojis (rejeitada por inconsistência visual e acessibilidade).

## Riscos e Limites

- O shell Android não está presente no branch; T067 da feature 001 ainda define o pacote local. A integração de ícone nativo e validação Android dependem desse artefato e do gate de aceite em `docs/android-toolchain.md`.
- Os perfis oficiais `web-e2e` e `android` não são executáveis até seus arquivos de deployment/teste e gate existirem. A validação não pode iniciar aplicação fora de containers nem contornar nginx.
- O frontend atual não declara Vitest nem outro runner unitário; usar o perfil Playwright já fixado para comportamento integrado e `quality` para build/estática, sem adicionar runner por conveniência.
- A reutilização Dokpod deve ser limitada aos arquivos com licença, autoria e imports verificáveis. A presença de `AGPL-3.0` no LICENSE não libera marca, imagens ou conteúdo sem licença própria.
- A ausência de biblioteca axe no projeto impede assumir um scanner existente; priorizar testes Playwright/DOM próprios e cálculo automatizado local de contraste, sem incluir dependência não auditada.
