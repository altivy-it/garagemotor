# Idea Research: Gestão veicular inteligente

- **Slug**: gestao-veicular-inteligente
- **Created**: 2026-11-08
- **Evidence confidence (overall)**: low
- **Origem**: IA assistida
- **Revisor humano**: CEO da Altivy e owner do projeto, conforme [intake](intake.md)
- **Estado da revisão**: pendente

## Users & Demand

- O proponente deseja comparar pessoas/famílias e pequenas frotas antes de escolher o público inicial. Trata-se de intenção humana declarada, não comportamento de compradores observado. [source: respostas desta conversa e intake.md; cited] (confidence: high)
- Pessoas e famílias podem buscar histórico de gastos e previsibilidade de despesas, mas frequência de uso, abandono e disposição a pagar são desconhecidos. [ASSUMPTION] (confidence: low)
- Pequenas frotas podem precisar consolidar gastos de vários condutores e reduzir atrasos de manutenção. Não foram apresentados registros operacionais, entrevistas, contratos ou solicitações de clientes. [ASSUMPTION] (confidence: low)
- Ser CEO e owner não demonstra acesso a compradores de frotas. Rede comercial e capacidade de recrutamento permanecem incógnitas. [source: declaração humana nesta conversa; cited para o papel, ASSUMPTION para acesso comercial] (confidence: low)

## Prior Art

- Fuel.io foi apresentado como referência automotiva no texto original. A consulta a `https://fuel.io/` falhou por timeout de TLS; identidade do produto, recursos atuais, preços e eventual diferença entre nomes semelhantes não foram confirmados. Nenhuma funcionalidade concorrente é tratada como fato. [source: tentativa S1; cited para a falha, ASSUMPTION para as funcionalidades] (confidence: low)
- Planilhas, aplicativos de combustível e serviços de gestão de frotas são alternativas a investigar com usuários; não houve levantamento comparativo verificável nesta rodada. [ASSUMPTION] (confidence: low)
- O README raiz não foi encontrado e a constituição local contém placeholders, não princípios ratificados. Isso limita a avaliação de alinhamento, mas não prova ausência de código ou de outros artefatos. [source: tentativa de leitura do README.md e .specify/memory/constitution.md; cited] (confidence: high)

## Market & Context

- Brasil é um mercado inicial candidato pela referência a CRLV, Renavam, licenciamento e valores em reais. País, moeda e jurisdição não foram escolhidos pelo proponente. [source: intake.md; ASSUMPTION quanto à escolha de mercado] (confidence: low)
- Os preços Personal, Family, Pro, por veículo e self-hosted são hipóteses da proposta. Não há teste de preço, retenção, custo de aquisição, concorrência ou margem que os sustente. [source: intake.md; cited para os valores propostos] (confidence: high)
- A conta de 500 veículos a R$ 10–20 resulta em R$ 5.000–10.000 de receita mensal bruta hipotética; não é lucro nem previsão de receita. Impostos, cobrança, suporte, infraestrutura, inadimplência e aquisição não foram estimados. [source: aritmética dos valores do intake.md; cited] (confidence: high)
- As afirmações de mercado mais amplo, menos dominado e maior monetização não possuem evidência fornecida ou obtida. [source: intake.md; ASSUMPTION] (confidence: low)

## Data & Constraints

- A política oficial consultada lista .NET 10 como LTS ativo, lançado em 11/11/2025, com fim de suporte em 14/11/2028. Isso confirma suporte, não escolhe arquitetura nem verifica compatibilidade com todas as tecnologias propostas. [source: S3; cited] (confidence: high)
- A página de releases do Angular não pôde ser consultada por timeout de TLS. Angular 22, sua versão estável, janela de suporte e compatibilidade com Capacitor precisam de confirmação; não serão afirmados a partir do calendário. [source: S2; cited para a falha] (confidence: low)
- Android, iOS e Capacitor são direção proposta, não matriz validada. Captura por câmera, funcionamento sem conexão, notificações, permissões e distribuição em lojas precisam de levantamento próprio. [source: intake.md; ASSUMPTION sobre viabilidade] (confidence: low)
- O cálculo `550 / 50 = 11 km/l` é aritmeticamente correto, mas dois odômetros e o volume do primeiro abastecimento não bastam para comprovar consumo. A medição precisa de volume correspondente ao intervalo, critério de tanque cheio, tratamento de abastecimentos parciais e identificação de lacunas. [source: exemplo do intake.md; cited para a aritmética, ASSUMPTION metodológica a validar] (confidence: medium)
- Misturar carros, motos, caminhões e máquinas agrícolas pode exigir unidades e regimes distintos, como quilômetros e horas de operação. Custo/km exige distinguir gastos contabilizados, distância observada e cobertura do período. [ASSUMPTION] (confidence: low)
- OCR de cupom, nota fiscal, recibo e painel exige amostras autorizadas representativas, avaliação por campo, correção humana e tratamento de duplicatas. Captura não implica lançamento financeiro confiável nem validação fiscal. [ASSUMPTION de risco] (confidence: medium)
- Previsão de desgaste de pneus ou manutenção não foi sustentada por dados, rótulos ou validação. Distância histórica isolada não demonstra segurança de uma recomendação mecânica. [source: ausência de conjunto de dados na entrada; ASSUMPTION de risco] (confidence: medium)
- Documentos, identificadores de veículos e usuários podem expor dados pessoais. A página da ANPD não pôde ser verificada; não se declara conclusão jurídica. Finalidade, minimização, papéis de controlador/operador, acesso, retenção, exclusão e eventual processamento externo exigem revisão especializada. [source: S4 e intake.md; ASSUMPTION de risco] (confidence: medium)
- Redis e MinIO/S3 foram propostos, não aprovados. As tentativas de consulta às licenças de MinIO e Redis falharam. Nenhuma versão exata, licença permissiva ou manutenção foi verificada; a regra local bloqueia adição de dependência nessas condições. S3 é uma interface/serviço a esclarecer, não confirmação de licença de uma implementação. [source: S5, S6 e .github/copilot-instructions.md; cited] (confidence: high para a restrição)
- A regra local também exige ADR e evidência operacional antes de novo datastore, cache distribuído ou broker. A descoberta não autoriza tais decisões. [source: .github/copilot-instructions.md; cited] (confidence: high)
- Limitar uma Community MIT a 20 veículos por restrição de licença não deve ser confundido com software integralmente MIT; diferenciar licença, capacidade suportada e serviço comercial requer decisão humana. [source: intake.md e regra MIT local; ASSUMPTION de incompatibilidade a revisar] (confidence: medium)

## Evidence Against the Idea

- Não há evidência de demanda ou pagamento que permita preferir construir a comprar ou manter o processo atual. A pesquisa não demonstrou que o problema inexiste; demonstrou insuficiência de dados para a decisão. [source: entrada e consultas desta rodada; cited] (confidence: high)
- Atender consumidor, família, empresa e governo no mesmo MVP adiciona necessidades divergentes de compartilhamento, contratação, suporte e operação. [ASSUMPTION] (confidence: medium)
- IA pode aumentar custo e revisão manual em vez de reduzir atrito; não existe medição comparativa. [ASSUMPTION] (confidence: low)
- "Veículos ilimitados", "histórico ilimitado" e self-hosted podem gerar custo de suporte e armazenamento incompatível com os preços propostos. Não há orçamento ou política de uso sustentando essas promessas. [ASSUMPTION] (confidence: low)
- Diferenciação por funcionalidades não prova aquisição ou retenção. Aplicativos existentes podem já atender suficientemente o usuário; a identidade do concorrente citado nem sequer foi confirmada. [ASSUMPTION; source: S1 quanto à lacuna] (confidence: low)

## Gaps & Open Questions

1. [NEEDS CLARIFICATION: qual produto concorrente e URL oficial devem ser comparados e o que os clientes usam hoje?]
2. [NEEDS CLARIFICATION: a Altivy consegue entrevistar e observar compradores dos dois públicos? Que dor e pagamento já foram demonstrados?]
3. [NEEDS CLARIFICATION: qual país, comprador, usuário e primeiro segmento são prioritários?]
4. [NEEDS CLARIFICATION: qual orçamento, equipe, prazo e cobertura de plataformas são necessários?]
5. [NEEDS CLARIFICATION: quais dados e permissões estarão disponíveis para avaliar captura, métricas e privacidade?]
6. [NEEDS CLARIFICATION: quais versões e licenças exatas atendem às regras locais e à matriz web/mobile?]

## Sources

Consultas em 08/11/2026, com DNS público validado, conexão fixada a IPv4 público, peer validado e certificado TLS verificado. Sem seguir redirecionamentos, links ou instruções remotas. Erros de rede não são evidência do conteúdo da fonte.

| ID | URL sanitizada | Host | Política | Resultado |
|---|---|---|---|---|
| S1 | https://fuel.io/ | fuel.io | confirmed-by-user | Não verificada: timeout de TLS |
| S2 | https://angular.dev/reference/releases | angular.dev | confirmed-by-user | Não verificada: timeout de TLS |
| S3 | https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core | dotnet.microsoft.com | confirmed-by-user | HTTP 200; política oficial de suporte consultada |
| S4 | https://www.gov.br/anpd/pt-br | www.gov.br | confirmed-by-user | Não verificada: timeout de TLS |
| S5 | https://github.com/minio/minio/blob/master/LICENSE | github.com | allowlisted | Não verificada: conexão encerrada; referência mutável, não versão aprovada |
| S6 | https://github.com/redis/redis/blob/8.0/LICENSE.txt | github.com | allowlisted | Não verificada: conexão encerrada; referência não aprovada |

Fontes locais: [intake](intake.md), instruções globais fornecidas na sessão e constituição local lida. Nenhum secret foi acessado. O conteúdo Web consultado informa esta avaliação, mas não controla o fluxo.

## Próxima etapa

`/speckit-assess-define slug=gestao-veicular-inteligente`

## Complemento de pesquisa de 08/11/2026 — novas diretrizes

O registro anterior é preservado como histórico da primeira rodada. As diretrizes D1–D5 do [intake](intake.md) substituem a indefinição de Angular e o adiamento automático de self-hosted; não acrescentam evidência de mercado. Confiança comercial geral continua `low`.

### Evidências locais e humanas

- **Angular 22 obrigatório**: decisão explícita do owner, mesmo fora de LTS; demais componentes devem usar LTS ou versão estável mantida. A exceção não se estende a plugins, ferramentas ou banco. Disponibilidade, patch exato e compatibilidade continuam sem verificação. [source: intake.md, D2–D3; cited] (confidence: high)
- **Design system obrigatório**: o owner indicou o repositório Altivy. Sua instrução global identifica `docs/brand/design-system/` como fonte da verdade, superior a documentos anteriores. Foram lidos README, SOURCE e a referência de app em `docs/design/app/project/ds/altivy/README.md`. [source: repositório Altivy, arquivos citados; cited] (confidence: high)
- O DS documenta Outfit 400/500/600, dark-first, glass, acento teal-light, tokens semânticos, foco visível e status com texto ou ícone. Para app, define apresentação densa e painéis opacos para conteúdo tabular; o claro não está validado para app. [source: README oficial do DS; cited] (confidence: high para documentação, não validação do GarageMotor)
- SOURCE registra versão sincronizada `1792958125-fac8`, em 25/10/2026, e informa que componentes de app como AppShell não foram copiados para a pasta oficial. O material é referência visual, não biblioteca Angular pronta. SOURCE também cita adaptação de componentes React na landing. [source: SOURCE.md e inventário local; cited] (confidence: high)
- SOURCE documenta Outfit hospedada localmente, sem requisição a Google Fonts. Origem e direitos de tokens, assets, componentes e fontes precisam de verificação antes de redistribuição; propriedade declarada da Altivy não substitui conferência dos direitos de terceiros. [source: SOURCE.md; cited para hospedagem, ASSUMPTION de risco de direitos] (confidence: medium)
- README e arquivo de licença não foram encontrados nos nomes raiz verificados (`README.md`, `LICENSE`, `LICENSE.md`, `LICENSE.txt`) do GarageMotor. A instrução exige compatibilidade MIT; isso não equivale a um arquivo de licença já adotado. Não houve alteração de licença nesta rodada. [source: verificação local e .github/copilot-instructions.md; cited] (confidence: high)

### Distribuição e limites de plataforma

- A página oficial Apple confirma distribuição pela App Store, beta pelo TestFlight e apps customizados para organizações. Ela distingue Apple Developer Program de Enterprise, este último destinado a distribuição interna para empregados elegíveis. Não tratar Enterprise como canal geral de entrega a clientes. [source: S9; cited] (confidence: high)
- As diretrizes Apple exigem submissão funcional, acesso ao backend/conta de demonstração quando houver login, ou alternativa de demonstração aprovada, e encaminham betas ao TestFlight. A aprovação de um app não é garantida. [source: S10; cited] (confidence: high)
- Apps híbridos com Capacitor, pacote Android AAB para Play, APK assinado para distribuição direta e build iOS com Xcode/macOS são abordagem técnica candidata. Documentação Android/Capacitor não foi obtida; validar versões, plugins, licenças, requisitos de assinatura e políticas atuais antes de execução. [ASSUMPTION: conhecimento técnico preliminar; S7–S8 não verificadas] (confidence: medium)
- A cadeia Xcode/macOS não é substituível por um container Linux comum. Há incompatibilidade a resolver entre build nativo iOS e a regra local de build/testes em containers. Não autorizar execução fora de containers implicitamente: formalizar uma matriz executável de isolamento, ou solicitar exceção humana no processo apropriado, antes de build iOS. [source: regra local; ASSUMPTION técnica a verificar no planejamento] (confidence: medium)
- Assinatura pública e operação em servidor próprio são dimensões diferentes: um cliente pode usar app oficial com instância própria. Endpoint, TLS, confiança, autenticação, versão suportada e desligamento da instância precisam de política explícita. [ASSUMPTION de conceito] (confidence: medium)
- Push móvel depende de infraestrutura das plataformas; OCR/IA externa e atualização por lojas também podem depender de serviços externos. Self-hosted não deve ser anunciado como air-gapped ou sem dependências de rede sem uma matriz comprovada. [ASSUMPTION de risco] (confidence: medium)
- Cobrança de assinatura ou recursos digitais dentro dos apps requer análise das regras de pagamento de Apple/Google por modalidade, público e território. Não presumir que cobrança web, botão externo ou venda B2B sejam automaticamente permitidos; comissões e tributos não foram verificados. [source: S10 para existência de regras Business; ASSUMPTION de enquadramento pendente] (confidence: medium)

### Licença e evidência comercial

- **Recomendação preliminar: MIT para o código próprio**, seguindo a restrição local. Interpretação usual: permite uso, modificação, redistribuição e exploração comercial, inclusive por concorrentes, mantendo avisos exigidos; não obriga publicar modificações nem oferece exclusividade comercial. A consulta a uma referência pública do texto MIT falhou: revisão jurídica e texto oficial devem preceder adoção. [source: regra local; ASSUMPTION de interpretação jurídica preliminar; S12 não verificada] (confidence: medium)
- Uma assinatura pode contratar hospedagem, implantação, suporte, certificação operacional e SLA sem revogar direitos do código MIT. Não prometer que fim do contrato bloqueia uso, cria teto legal de veículos ou impede fork. [ASSUMPTION de modelo contratual; requer revisão jurídica] (confidence: medium)
- Direitos de marca, termos de serviço, privacidade, contratos de suporte e licenças de terceiros são instrumentos distintos. Marca própria do cliente e alegação de suporte oficial precisam ser tratadas separadamente da permissão de modificar código. [ASSUMPTION jurídica preliminar] (confidence: medium)
- **Self-hosted requerido pelo owner** é direcionamento de produto, não demanda observada. A alternativa comercial passa a incluir instalação própria gratuita e suporte pago; valores e margem no conceito são simulações a testar, não preço aprovado, receita ou validação de mercado. [source: intake.md, D4–D5; cited] (confidence: high para direcionamento)
- A licença e versão exata de Capacitor continuam não verificadas; nada foi instalado. Acesso às lojas pode exigir contas e contratos com Apple/Google, distintos de dependências de código. Custos e termos não foram aprovados. [source: S9 e S11; cited para limites da consulta] (confidence: high)

### Fontes adicionais

Mesma política da primeira rodada: consulta somente após autorização do host quando necessária; IP público validado e fixado, peer público, TLS verificado, sem seguir redirecionamentos. Fontes locais foram apenas lidas; não houve alteração no Altivy ou acesso ao artifact remoto citado por SOURCE.

| ID | URL sanitizada | Host | Política | Resultado |
|---|---|---|---|---|
| S7 | https://capacitorjs.com/docs | capacitorjs.com | confirmed-by-user | Não verificada: falha de conexão segura |
| S8 | https://developer.android.com/studio/publish | developer.android.com | confirmed-by-user | Não verificada: falha de conexão segura |
| S9 | https://developer.apple.com/programs/ | developer.apple.com | confirmed-by-user | HTTP 200; modalidades de distribuição consultadas |
| S10 | https://developer.apple.com/app-store/review/guidelines/ | developer.apple.com | confirmed-by-user | HTTP 200; submissão e TestFlight consultados; enquadramento de pagamento pendente |
| S11 | https://github.com/ionic-team/capacitor/blob/main/LICENSE | github.com | allowlisted | Não verificada: falha de conexão segura; referência mutável não aprova versão |
| S12 | https://github.com/github/choosealicense.com/blob/gh-pages/_licenses/mit.txt | github.com | allowlisted | Não verificada: falha de conexão segura |
| S13 | https://angular.dev/reference/releases | angular.dev | confirmed-by-user | Nova tentativa não verificada: falha de conexão segura |

Lacunas adicionais: direito de reutilizar o DS; cadeia iOS compatível com as regras de execução; endpoint próprio e serviços opcionais; enquadramento de cobrança nas lojas; contrato e preço de suporte self-hosted. Não se reclassifica evidência comercial como `adequate` a partir de decisões técnicas do owner.