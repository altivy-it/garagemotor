# Plano de Implementação: Design system e identidade GarageMotor

**Branch**: `003-garagemotor-design-system` | **Data**: 2026-11-11 | **Spec**: [spec.md](spec.md)

**Entrada**: Especificação da funcionalidade em `specs/003-garagemotor-design-system/spec.md`

**Estado de execução em 2026-11-11**: autorizado pelo owner nesta conversa. Após o bloqueio inicial, o owner confirmou expressamente que Dokpod é código da mesma empresa Altivy e autorizou copiar seus componentes para GarageMotor. Retomar o reuso somente de código próprio Altivy com proveniência conferida, preservando autoria e avisos; essa autorização não libera dependências externas copyleft, código de terceiros ou marcas de outro produto.

**Nota**: Este template é preenchido pelo comando `/speckit-plan`; sua definição descreve o fluxo de execução.

> Redija este plano e todos os artefatos derivados em português brasileiro (pt-BR). Preserve nomes de código, caminhos, comandos e identificadores técnicos.

## Resumo

Estabelecer um design system GarageMotor para a aplicação Angular compartilhada entre navegador e Android via Capacitor: tokens semânticos claro/escuro, preferência Claro/Escuro/Sistema persistida localmente, marca e ícones originais, e primitives acessíveis. Usar Outfit sob SIL OFL 1.1 self-hosted e adotar os princípios e a escala do design system Altivy, com paleta e identidade próprias do GarageMotor. Reutilizar seletivamente primitives independentes do Dokpod sob AGPL-3.0, preservando avisos e adaptando-as aos tokens e testes de ambos os temas; não importar seu tema, marca ou shell de produto.

## Contexto Tecnico

<!--
  AÇÃO NECESSÁRIA: Preencha esta seção com detalhes técnicos confirmados para a funcionalidade.
  Use as configurações, dependências e convenções existentes no repositório como evidência.
-->

**Linguagem/Versão**: TypeScript 6.0.3, Angular 22.2.5, Capacitor 8.6.0, Node.js 24.22.2

**Dependências Principais**: Angular, Forms, Capacitor e Playwright já fixados em `frontend/web/package-lock.json`; sem nova dependência de runtime/build. Outfit é um ativo licenciado SIL OFL 1.1, não um pacote de runtime.

**Persistência**: preferência não sensível em `localStorage`, com chave versionada e escopo da origem/instância; sem API, banco ou sincronização entre usuários.

**Testes**: perfil `quality` para format/build/análise estática; perfil `web-e2e` com Playwright para fluxo HTTP por nginx em container, incluindo tema, primitives e contraste computável. O frontend ainda não declara runner unitário próprio; não introduzir outro sem auditoria e necessidade demonstrada. Perfil `android` somente após existirem `frontend/web/android` e `tools/scripts/test-android.ps1` e o aceite humano da cadeia Android estar registrado.

**Plataforma Alvo**: web responsiva e shell Android Capacitor, compartilhando Angular/CSS; execução de build e testes em containers. Qualquer fluxo HTTP de teste/preview passa pela borda nginx em container.

**Tipo de Projeto**: aplicação frontend Angular standalone, sem alteração de API.

**Metas de Desempenho**: nenhuma meta quantitativa nesta feature; fonte, logo e ícones devem ser carregados localmente, sem requisições remotas em runtime.

**Restrições**: manter APIs/rotas e estado de autenticação; não guardar dados além da preferência de tema; respeitar `prefers-reduced-motion`, foco por teclado e WCAG 2.2 AA; não reutilizar logotipos ou símbolos Altivy/Dokpod; não adicionar dependências sem licença permissiva, gratuita, fixada e verificada. O kit de app Altivy não foi validado no tema claro. O perfil Android está bloqueado até seus pré-requisitos e aceite estarem presentes.

**Escala/Escopo**: fundação do design system e migração da shell/auth existente; biblioteca inicial de ações, campos, seleção, checkbox, superfícies, navegação local, status, feedback e estados vazios/carregamento. Não migrar telas de domínio ainda inexistentes nem copiar charts/data-grid/drawers do Dokpod sem necessidade aprovada.

## Verificação da Constituição

*GATE: Deve ser aprovado antes da pesquisa da Fase 0 e revisado novamente após o design da Fase 1.*

- **I. Licenciamento aberto com reciprocidade**: manter o código próprio sob AGPL-3.0-only; reutilização de código Dokpod somente sob a licença verificada no repositório (`AGPL-3.0`) e com avisos/proveniência preservados. Outfit mantém o texto SIL OFL 1.1 e checksum.
- **III. Tecnologia mantida e identidade visual Altivy**: manter Angular 22 e usar o design system Altivy como referência de tokens semânticos, tipografia, foco e acessibilidade; a identidade, paleta de marca, logotipo e ícones GarageMotor são originais. Não assumir que os componentes de app Altivy suportam claro: adaptar e validar localmente ambos os temas.
- **IV. Segurança, confidencialidade e integridade**: persistir apenas `light | dark | system`; não registrar nem transmitir preferência, credenciais ou estado de domínio. SVGs locais sem script ou referências externas.
- **V. Mudanças verificáveis e autorizadas**: gerar tarefas e analisar consistência antes de implementação; testes de frontend em containers, fluxo web via nginx, Playwright para jornadas e Android sujeito ao gate de aceite existente.
- **Restrições técnicas**: sem broker, cache, serviço, datastore ou dependência nova. Sem contratos públicos, migration ou alteração de API.

**Verificação revisada após autorização expressa**: o reuso é permitido como incorporação de código próprio Altivy, não como exceção para dependência externa copyleft. A permissão do titular está referenciada em `tasks.md`; a inspeção de proveniência e imports de cada arquivo continua obrigatória. A validação Android permanece condicionada ao aceite humano exigido pela documentação da cadeia Android; não contornar o gate.

## Estrutura do Projeto

### Documentação desta Funcionalidade

```text
specs/003-garagemotor-design-system/
├── spec.md               # Requisitos e cenários da funcionalidade
├── plan.md               # Este plano
├── research.md           # Decisões e proveniência
├── data-model.md         # Preferência local e ativos
├── quickstart.md         # Validação em containers
└── tasks.md              # Gerado por /speckit-tasks
```

### Código-Fonte (raiz do repositório)
<!--
  AÇÃO NECESSÁRIA: Substitua a arvore de exemplo abaixo pelos caminhos reais afetados.
  Liste somente os projetos e arquivos relevantes para esta funcionalidade.
-->

```text
frontend/web/
├── angular.json                         # Inclusão de assets locais no build
├── src/app/app.component.ts             # Shell global e acesso ao tema
├── src/app/core/                        # Serviço e inicialização de tema
├── src/app/design-system/               # Tokens, primitives, ícones e testes
├── src/app/features/auth/               # Primeira tela migrada
├── src/assets/brand/                    # Marca GarageMotor original
├── src/assets/fonts/                    # Outfit e licença/checksum
├── src/index.html                       # Metadados e ícone web
└── src/styles.css                       # Tokens semânticos e estilos globais
frontend/tests/                          # Testes Playwright de jornadas web
docs/dependencies.md                     # Avisos e proveniência, se código Dokpod for copiado
```

**Decisão de Estrutura**: manter o sistema visual junto ao frontend Angular existente; serviço de tema em `core`, primitives e seus testes em `design-system`, assets locais sob `src/assets`. Configurar explicitamente a cópia de `src/assets` no build Angular, pois `angular.json` atual não declara assets. Não criar contratos porque nenhuma interface externa é alterada. O shell Android ainda depende da entrega T067 da feature 001; o design responsivo e o serviço Capacitor devem permanecer compartilháveis sem criar um segundo sistema visual.

## Desenho da Solução

**Ajuste humano autorizado em 2026-11-11**: substituir a paleta verde de interface por azul Altivy `#2563EB`, com neutros e variantes claras/escuras ajustados para contraste. Manter aliases dos tokens antigos para compatibilidade dos consumidores existentes. Verde fica restrito ao sucesso semântico. Preservar os cinco assets anteriores e criar versões azuis da geometria atual, além de propostas separadas com o motivo triangular invertido solicitado; propostas não são aprovação de marca. Esta decisão posterior substitui as referências a preservar a paleta verde neste plano.

- Definir tokens semânticos `light` e `dark` para superfícies, texto, bordas, acento, estados e foco, preservando a paleta GarageMotor já existente e usando Altivy como referência de sistema, não copiando sua marca.
- Implementar tema selecionado `light | dark | system`, padrão `system`; quando o dispositivo não expuser preferência, usar `light`. Resolver preferência antes da primeira renderização, observar mudanças de `matchMedia`, persistir somente valores válidos e aplicar `data-theme`/`color-scheme` na raiz.
- Manter primitives standalone e compatíveis com toque/teclado. Seletivamente adaptar button, field, select, checkbox, card, tabs, status, toast e empty/loading state do Dokpod; substituir o componente de ícone por um `gm-icon` com paths GarageMotor originais. Não portar CSS global, shell, charts, grids ou módulos de produto.
- Criar logo, símbolo, favicon/ícone de app e ícones SVG originais do GarageMotor, sem script, fonte ou referência externa. Self-host Outfit e preservar OFL e checksum.
- Migrar shell e autenticação primeiro; testar temas, estados, acessibilidade, responsividade e ausência de assets remotos. Playwright cobre desktop e viewport móvel via nginx. Validar build Android quando seu gate estiver liberado.

## Fases de Implementação

1. **Fundação**: auditar e registrar proveniência/licenças, configurar assets/fontes, criar tokens claros/escuros, serviço de tema e verificação de contraste.
2. **Tema**: expor seletor Claro/Escuro/Sistema no shell e autenticação, cobrir persistência e mudança de preferência do dispositivo.
3. **Identidade**: produzir e integrar wordmark, símbolo, ícone de app, favicon e ícones originais; validar ambos os temas e tamanhos.
4. **Primitives e primeira migração**: adaptar primitives selecionadas, migrar a shell/auth, manter semântica de estado e validar teclado/toque.
5. **Verificação transversal**: Playwright via nginx em desktop/móvel, quality/build e, após liberação do gate, build/smoke Android.

## Gates e Dependências

- Reutilização Dokpod exige preservar AGPL-3.0, autoria/proveniência dos arquivos e avisos necessários; excluir código sem origem/licença clara e dependências não aprovadas.
- Antes de copiar Outfit, conferir a licença e os três hashes registrados em `Altivy/docs/brand/fonts/SHA256SUMS`; a licença SIL OFL 1.1 e o arquivo fonte exato estão disponíveis no repositório Altivy.
- Perfil `web-e2e` exige os artefatos de deployment pendentes da T067 da feature 001 (`deploy/Dockerfile.api`, `deploy/Dockerfile.web`, `tools/scripts/start-local.ps1`) e `frontend/tests/playwright.config.ts`. Não contornar ausências; registrar bloqueio e executar validações isoladas disponíveis.
- Perfil `android` exige `frontend/web/android`, `tools/scripts/test-android.ps1` e aceite humano da cadeia Android conforme `docs/android-toolchain.md`.
- Todos os requisitos FR/SC da spec devem mapear para uma ou mais tarefas antes de pedir revisão/autorização de implementação.

## Registro de Complexidade

> **Preencha somente se houver violações da constitution que precisem de justificativa.**

| Violação | Motivo da Necessidade | Alternativa Mais Simples Rejeitada Porque |
|-----------|------------|-------------------------------------|
| [descrição] | [necessidade atual] | [motivo da rejeição] |
