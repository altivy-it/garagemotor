# Especificação da Funcionalidade: Design system e identidade GarageMotor

**Branch da Funcionalidade**: `003-garagemotor-design-system`

**Criada em**: 2026-11-11

**Status**: Rascunho

**Entrada**: Analisar a identidade visual disponível em Altivy, avaliar primitives Angular reutilizáveis do Dokpod e estabelecer um design system GarageMotor usado em web e Android, com logo e ícones originais e seletor de tema Claro/Escuro/Sistema sempre acessível.

**Origem**: solicitação humana. **Revisor humano**: CEO da Altivy e owner do GarageMotor.

## Ajuste Visual Solicitado em 2026-11-11

O owner solicitou azul como cor geral do tema no lugar do verde e arquivos alternativos de marca para escolha, com o traço interno do G inspirado no triângulo invertido do A Altivy. Esta solicitação autoriza a alteração de paleta e a exploração do motivo triangular, não a adoção automática de uma nova geometria de marca. O azul de referência é `#2563EB`; superfícies, foco e variantes escuras devem preservar contraste AA. Verde pode permanecer exclusivamente como cor semântica de sucesso, sem dominar o tema. Preservar os assets anteriores e criar propostas separadas; não incorporar o logotipo Altivy completo nem confundir os produtos. O uso desse motivo nas propostas é a exceção expressamente solicitada ao limite de FR-010, sem ampliar direitos sobre outros ativos.

## Cenários de Usuário e Testes *(obrigatório)*

### História de Usuário 1 - Usar GarageMotor no tema escolhido (Prioridade: P1)

Uma pessoa consegue escolher como visualizar o GarageMotor e mantém essa escolha ao voltar ao aplicativo, no navegador ou no Android.

**Motivo da prioridade**: tema e legibilidade afetam toda interação; a preferência deve estar disponível independentemente da plataforma ou da etapa de autenticação.

**Teste independente**: abrir a aplicação sem preferência explícita, alternar entre Claro, Escuro e Sistema, atualizar/reabrir e validar o tema efetivo em navegador e Android.

**Cenários de aceitação**:

1. **Dado** um usuário em qualquer tela principal web ou Android, **Quando** abre o controle de tema, **Então** pode escolher Claro, Escuro ou Sistema e a escolha fica visível e acessível por teclado.
2. **Dado** que Sistema está selecionado, **Quando** a preferência de aparência do dispositivo muda, **Então** a aplicação acompanha o novo tema sem recarregar e sem alterar a seleção Sistema.
3. **Dado** que Claro ou Escuro foi selecionado, **Quando** o usuário atualiza ou reabre a aplicação, **Então** a preferência explícita continua aplicada até ser alterada.

### História de Usuário 2 - Reconhecer a identidade GarageMotor (Prioridade: P1)

Uma pessoa reconhece o GarageMotor pela mesma identidade visual na web, no Android, na tela de autenticação e no ícone instalado, sem confundi-lo com a marca Altivy ou Dokpod.

**Motivo da prioridade**: a identidade própria torna o produto reconhecível e acompanha todas as superfícies do sistema.

**Teste independente**: revisar logo, símbolo, ícone de aplicativo e telas principais nos temas claro e escuro, em desktop e viewport móvel; confirmar que os ativos não reutilizam logotipo ou marca de outro produto.

**Cenários de aceitação**:

1. **Dado** um usuário abrindo o GarageMotor, **Quando** vê a autenticação, navegação ou ícone instalado, **Então** cada superfície apresenta a identidade original do GarageMotor e uma versão legível para o tema atual.
2. **Dado** os ativos de marca entregues, **Quando** são comparados às marcas Altivy e Dokpod, **Então** o nome e o símbolo identificam exclusivamente o GarageMotor e não reutilizam seus logotipos.

### História de Usuário 3 - Manter experiências consistentes (Prioridade: P1)

Uma pessoa usa controles previsíveis e acessíveis nas jornadas existentes, enquanto a equipe consegue compor telas web e Android com primitives visuais compartilhadas.

**Motivo da prioridade**: consistência reduz erros de interação, suporta acessibilidade e evita que cada funcionalidade crie sua própria variante de controle.

**Teste independente**: validar a tela de autenticação com primitives comuns nos dois temas e plataformas; conferir teclado, foco, nomes acessíveis e estados de carregamento, vazio, sucesso e erro.

**Cenários de aceitação**:

1. **Dado** um controle interativo usado pelo produto, **Quando** está normal, focado, pressionado, desabilitado ou carregando, **Então** usa os mesmos padrões de aparência, feedback e acessibilidade do design system.
2. **Dado** um estado informativo, de erro, alerta ou sucesso, **Quando** aparece na interface, **Então** seu significado não depende apenas da cor e permanece legível nos temas claro e escuro.
3. **Dado** uma tela web ou Android em viewport estreito, **Quando** conteúdo, validações ou controles mudam, **Então** textos e ações não se sobrepõem nem ficam inacessíveis.

### Casos de Borda

- Se o dispositivo não expuser preferência de tema, Sistema deve usar o tema padrão definido pelo produto e continuar selecionado.
- Mudanças de tema não podem apagar a seleção, o foco atual nem o estado do formulário.
- Preferências de tema não podem carregar credenciais ou tokens e devem permanecer isoladas por instância GarageMotor.
- Logo e ícones devem continuar distinguíveis em fundos claros, escuros, superfícies opacas e fundos de alto contraste.
- Movimento reduzido e ausência de fontes externas devem manter todas as jornadas funcionais.

## Requisitos *(obrigatório)*

### Requisitos Funcionais

- **FR-001**: O GarageMotor DEVE oferecer as opções de aparência Claro, Escuro e Sistema na web e no Android.
- **FR-002**: O seletor de tema DEVE permanecer acessível nas jornadas principais, inclusive antes da autenticação e após a entrada, por mouse, toque e teclado.
- **FR-003**: Em Sistema, a aparência efetiva DEVE acompanhar mudanças da preferência de tema do dispositivo; escolhas explícitas de Claro ou Escuro DEVEM persistir entre recargas e reinicializações até nova alteração.
- **FR-004**: A preferência de tema DEVE ser isolada por instalação/instância e NÃO DEVE conter credenciais, tokens ou dados de domínio.
- **FR-005**: O GarageMotor DEVE possuir logotipo, símbolo e ícones de aplicação originais, coerentes entre web e Android, com variações que mantenham legibilidade nos temas claro e escuro.
- **FR-006**: O produto DEVE aplicar uma linguagem visual compartilhada e primitives reutilizáveis às telas web e Android, incluindo controles de ação, formulário, seleção, navegação, superfícies, status, feedback e estados vazios/carregamento.
- **FR-007**: Ícones sem rótulo visível DEVEM ter nome acessível e indicação compreensível; status e resultados DEVEM ter texto ou ícone além da cor.
- **FR-008**: As jornadas principais DEVEM oferecer contraste WCAG 2.2 AA, foco visível, navegação por teclado, adaptação a viewport móvel e redução de movimento.
- **FR-009**: Fontes e ativos do design system DEVEM funcionar sem depender de CDN ou recurso remoto em runtime; licenças, proveniência e avisos necessários DEVEM ser preservados.
- **FR-010**: O GarageMotor NÃO DEVE copiar o logotipo, símbolo ou ativos de marca do Altivy ou Dokpod; código de componentes Dokpod só pode ser reutilizado após confirmar sua licença/proveniência e compatibilidade, mantendo avisos aplicáveis e sem incorporar a edição Business ou dependências não aprovadas.

### Entidades Principais

- **Tema selecionado**: preferência de aparência Claro, Escuro ou Sistema, independente de sessão de autenticação e dos dados da garagem.
- **Ativo de identidade GarageMotor**: logotipo, símbolo ou ícone original com finalidade e apresentação adequada aos temas claro/escuro.
- **Primitive visual**: padrão compartilhado de controle ou estado que mantém semântica, aparência e acessibilidade consistentes nas superfícies web/Android.

## Critérios de Sucesso *(obrigatório)*

### Resultados Mensuráveis

- **SC-001**: 100% das jornadas principais web e Android exibem um seletor acessível com Claro, Escuro e Sistema.
- **SC-002**: Em 100% dos testes de tema, Sistema acompanha a preferência do dispositivo e as escolhas explícitas persistem após recarga/reinício sem substituir uma preferência explícita por uma mudança do sistema.
- **SC-003**: 100% das superfícies principais e ícones instaláveis usam ativos originais GarageMotor, sem logotipos Altivy/Dokpod.
- **SC-004**: 100% dos controles e estados abrangidos pela biblioteca compartilhada passam verificações de nomes acessíveis, foco/teclado e contraste AA nos dois temas.
- **SC-005**: As telas validadas em web e Android funcionam em viewport desktop e móvel sem sobreposição, perda de texto ou ação inacessível.
- **SC-006**: Build de produção e smoke test dos ativos confirmam que fonte, logo e ícones são carregados localmente e não exigem conexão externa em runtime.

## Premissas

- “Sistema” acompanha a preferência de aparência do dispositivo; se ela não existir, aplica o tema padrão definido durante o planejamento.
- O conjunto visual Altivy é referência de tokens, tipografia e princípios, não autorização para usar o logotipo Altivy como marca do GarageMotor.
- O tema dark-only e os componentes do Dokpod são referência e candidatos à adaptação, não uma dependência obrigatória nem uma promessa de compatibilidade com o tema claro.
- A tela de autenticação existente será a primeira superfície da aplicação migrada para o design system; as demais jornadas o adotarão conforme suas tarefas de feature.
- A seleção de tema é uma preferência local de interface e não sincroniza entre instâncias ou usuários.
