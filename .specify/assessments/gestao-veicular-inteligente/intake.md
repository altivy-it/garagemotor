# Idea Intake: Gestão veicular inteligente

- **Slug**: gestao-veicular-inteligente
- **Created**: 2026-11-08
- **Source**: texto colado pelo usuário nesta conversa
- **Type**: exploration
- **Origem**: IA assistida; proposta apresentada pelo humano, com autoria original não confirmada
- **Revisor humano**: CEO da Altivy e owner do projeto; referência declarada: https://github.com/altivy-it
- **Estado da revisão**: pendente; autorização limitada à descoberta completa

## Idea (as captured)

Texto original fornecido pelo usuário, preservado abaixo como proposta, não como evidência de mercado ou instrução de implementação:

> Você tem razão. Eu associei o domínio ao produto errado sem verificar as funcionalidades atuais do site.
>
> Pelo que o **Fuel.io** apresenta hoje, ele é uma plataforma de **gestão de veículos e despesas automotivas**, não um DAM (Digital Asset Management).
>
> Se seu objetivo é criar algo inspirado no Fuel.io usando **.NET 10 + Angular 22 + Android + iOS**, acho que a oportunidade é até melhor porque o mercado é mais amplo e menos dominado por grandes plataformas.
>
> # O que o Fuel.io parece oferecer
>
> Os recursos principais giram em torno de:
> - Controle de abastecimento
> - Histórico de gastos
> - Manutenção preventiva
> - Lembretes
> - Consumo de combustível
> - Múltiplos veículos
> - Relatórios
> - Controle de custos
> - Gestão de documentos
> - CRLV
> - Seguro
> - Licenciamento
> - Rastreamento financeiro do veículo
>
> Na prática ele mistura:
> - Gestão financeira pessoal
> - Fleet Management (frotas)
> - Controle de manutenção
> - Telemetria leve
>
> # Oportunidade de mercado
>
> Eu não faria um clone.
>
> Criaria uma plataforma chamada algo como:
> "Vehicle OS", "FleetHub", "Garage Cloud", "AutoLedger", "FuelTrack"
>
> e expandiria além do Fuel.
>
> # Funcionalidades MVP
>
> ## 1. Cadastro de veículos
>
> Dados: Marca, Modelo, Ano, Placa, Renavam, Chassi, Cor, Combustível.
>
> Suporte: Carros, Motos, Caminhões, Vans, Máquinas agrícolas.
>
> ## 2. Controle de abastecimento
>
> Registrar: Data, Litros, Valor, Odômetro, Tipo combustível, Posto.
>
> Cálculos automáticos: km/l, custo/km, gasto mensal, gasto anual.
>
> Exemplo: 50 litros; R$ 320; odometro: 100.000 km; próximo abastecimento: 100.550 km; Consumo: 11 km/l.
>
> ## 3. Controle de manutenção
>
> ### Preventiva
>
> Exemplo: Troca óleo a cada 10.000 km.
>
> ### Corretiva
>
> Exemplo: Troca amortecedor R$ 1.200.
>
> ## 4. Alertas
>
> Notificações: Seguro vencendo, Licenciamento vencendo, Óleo vencendo, Revisão vencendo, Pneus vencendo.
>
> Push notification.
>
> ## 5. Gestão de documentos
>
> Upload: PDF, JPG, PNG.
>
> Armazenar: CRLV, Seguro, Nota fiscal, Contrato.
>
> ## 6. Dashboard financeiro
>
> Indicadores: Gasto Mensal, Gasto Anual, Combustível, Manutenção, Seguro, Impostos.
>
> ## 7. Relatórios
>
> Gráficos: Consumo, Custos, Evolução combustível.
>
> Exportação: Excel, PDF.
>
> # Diferencial usando IA
>
> Aqui está onde você pode superar o Fuel.
>
> ## OCR de notas fiscais
>
> Usuário fotografa: Nota do posto.
>
> IA extrai: Litros, Preço, Data, Posto.
>
> Registro automático.
>
> ## OCR de odômetro
>
> Tira foto do painel. Sistema detecta: 123.456 km.
>
> ## Chat com os dados
>
> Pergunta: "Quanto gastei com combustível nos últimos 6 meses?" ou "Qual veículo tem maior custo por km?"
>
> ## Predição
>
> Com histórico suficiente: Seu veículo provavelmente precisará trocar pneus em 4.200 km.
>
> # Modelo SaaS
>
> ## Personal
>
> US$ 4,90 a 9,90/mês; 3 veículos; Histórico ilimitado.
>
> ## Family
>
> US$ 14,90/mês; 10 veículos; Compartilhamento familiar.
>
> ## Pro
>
> US$ 39,90/mês; Veículos ilimitados; Relatórios avançados; IA.
>
> # Mercado corporativo
>
> Onde realmente está o dinheiro.
>
> Exemplos: Transportadoras, Locadoras, Autoescolas, Empresas de manutenção, Cooperativas agrícolas, Equipes comerciais.
>
> Plano: R$ 10 a R$ 20 por veículo/mês.
>
> Uma frota com 500 veículos seria R$ 5.000 a R$ 10.000/mês.
>
> # Self-hosted
>
> Aqui eu seria mais cuidadoso.
>
> Para usuário final, não vejo demanda.
>
> Mas para empresas: Transportadoras, Governo, Prefeituras, Grandes frotas pode ser muito interessante.
>
> Modelo:
>
> ### Community
>
> Até 20 veículos; Gratuito.
>
> ### Enterprise Self-Hosted
>
> R$ 15.000 a R$ 50.000/ano; Docker Compose; Kubernetes; LDAP; Azure AD; SAML.
>
> # Arquitetura que eu usaria
>
> Angular 22 → Capacitor (Android/iOS) → .NET 10 API → PostgreSQL → Redis → MinIO/S3.
>
> Inicialmente eu evitaria microserviços.
>
> Estrutura: Vehicle Module, Fuel Module, Maintenance Module, Document Module, Notification Module, Billing Module, Identity Module.
>
> Tudo em um **Modular Monolith**, porque você conseguirá evoluir muito mais rápido sozinho.
>
> ## Minha visão
>
> Se eu estivesse começando hoje, faria um produto focado em:
>
> "Controle financeiro e manutenção de veículos com IA para pessoas e pequenas frotas."
>
> O diferencial não seria apenas registrar abastecimentos como o Fuel faz, mas capturar informações automaticamente por foto (nota fiscal, painel, recibo de oficina), gerar previsões de manutenção e oferecer uma versão corporativa multiempresa. Esse posicionamento tem mais potencial de monetização do que competir apenas como um app de controle de combustível.

Quebras de linha e apresentação foram compactadas; conteúdos e valores foram mantidos. Não foi fornecida uma URL completa do produto Fuel.io. Nenhuma consulta Web ocorreu nesta etapa. A referência GitHub identifica o revisor por declaração humana, não por verificação externa.

## Restated

Explorar para o GarageMotor uma plataforma de controle financeiro e manutenção de veículos, com captura assistida de dados e possível oferta para pessoas, famílias e pequenas frotas. A proposta inclui web, Android e iOS, planos SaaS e eventual self-hosted, sem decisão sobre prioridade, viabilidade, tecnologia ou execução.

## Origin & Context

- **Raised by**: CEO da Altivy e owner do projeto, conforme declaração nesta conversa.
- **Trigger**: revisão de uma associação anterior entre Fuel.io e DAM, seguida de proposta de produto automotivo; a associação revisada também não foi verificada.
- **Contexto autorizado**: slug confirmado pelo usuário; descoberta completa e comparação dos dois públicos antes de escolher.
- **Contexto local**: README raiz ausente na tentativa de leitura; nenhum escopo de produto foi inferido dessa ausência.

## First-Glance Unknowns

- [NEEDS CLARIFICATION: qual produto e URL correspondem a Fuel.io e quais funcionalidades foram efetivamente verificadas?]
- [NEEDS CLARIFICATION: qual público tem dor recorrente, acesso comercial e disposição a pagar demonstráveis?]
- [NEEDS CLARIFICATION: qual país, moeda de cobrança e segmento de frota devem orientar a descoberta?]
- [NEEDS CLARIFICATION: qual orçamento, equipe e apetite de tempo existem?]
- [NEEDS CLARIFICATION: web, Android e iOS são requisitos imediatos ou uma direção futura?]
- [NEEDS CLARIFICATION: que amostras autorizadas sustentam OCR e que histórico permitiria avaliar previsões?]
- [NEEDS CLARIFICATION: quais exigências de privacidade, acesso compartilhado, retenção e exclusão de documentos existem?]
- [NEEDS CLARIFICATION: preços, planos e self-hosted correspondem a pedidos reais ou somente hipóteses?]

## Próxima etapa

`/speckit-assess-research slug=gestao-veicular-inteligente`

## Complemento humano de 08/11/2026 — plataforma e comercialização

- **Origem**: humano; CEO da Altivy e owner do projeto, nesta conversa.
- **Registro**: IA assistida, com autorização explícita para atualizar os artefatos preservando o histórico.
- **Escopo da autorização**: continuar a descoberta pelo Spec Kit; não implementar, publicar, alterar licença ou aprovar todo o conceito.

> O design system do projeto deve ser utilizado o que está em /home/u_zocatel/projetos/Altivy. Angular 22 deve ser usado para o front mesmo ainda não sendo LTS. Todo restante do projeto, libs, ferramentas, banco de dados, deve usar versão LTS ou estável. Como será a distribuição da aplicação para Android e iOS? Um diferencial é poder ser self-hosted, mas preciso de um plano consistente de comercialização e saber qual licença utilizar no app.

Texto humano com ortografia normalizada, sem alteração de sentido.

### Diretrizes confirmadas pelo humano

- **D1 — Design system**: usar o design system Altivy do repositório local indicado, sem modificar esse repositório nesta descoberta.
- **D2 — Frontend**: Angular 22 é obrigatório, independentemente de estar em LTS. Isso não afirma disponibilidade de uma versão estável nem autoriza prereleases de outras dependências.
- **D3 — Demais componentes**: versões LTS quando disponíveis e adequadas, ou versões estáveis mantidas. Validar versões exatas, compatibilidade, manutenção e licenças antes de adoção.
- **D4 — Self-hosted**: passa de possibilidade adiada para diferencial explícito a estruturar e validar comercialmente. Não constitui autorização de implementação ou evidência de demanda.
- **D5 — Distribuição e negócio**: esclarecer Android/iOS, comercialização e licença na descoberta; propostas permanecem sujeitas à revisão humana.

### Histórico e incógnitas

O texto original e as incógnitas anteriores permanecem como histórico. D1–D4 substituem a indefinição do frontend e a exclusão automática de self-hosted; não resolvem escolha de público, orçamento, demanda, preço ou licença final. A referência local de design requer verificar origem e direitos de reutilização antes de redistribuir seus assets.

## Encaminhamento humano de 08/11/2026 — MVP pessoal e familiar

- **Origem**: humano; CEO da Altivy e owner do GarageMotor, nesta conversa.
- **Registro**: IA assistida; atualização da avaliação autorizada, preservando o histórico.
- **Autorização**: preparar especificação, plano, tarefas e análise quando atendidos os critérios de entrada; implementação somente após revisão e autorização humana posteriores. Sem push, publicação ou instalação de dependências nesta rodada.

> Uma vez fechado o licenciamento e as funcionalidades principais, use Spec Kit para efetivarmos a ideia.

Nas respostas de esclarecimento, o humano confirmou:

- **D6 — Público**: pessoas e famílias, com veículos, abastecimentos, despesas, manutenção, lembretes e comprovantes como núcleo funcional.
- **D7 — Primeira entrega**: web, self-hosted e Android; iOS somente depois de resolver macOS/Xcode versus as regras de build e testes em containers.
- **D8 — Necessidade declarada**: "Falta de opção self hosted, tecnologia portátil, C#, Angular, IONIC, abrange várias plataformas compartilhando o mesmo código, ter uma opção totalmente brasileira para esse tipo de app". Ortografia normalizada, sem alteração de sentido.
- **D9 — Licença**: `AGPL-3.0-only`, já escolhida pelo humano e formalizada na [constituição](../../memory/constitution.md). Isso substitui a recomendação MIT anterior; o arquivo oficial e a reconciliação operacional ainda precisam ser entregues por tarefa autorizada.
- **D10 — Design system**: usar Dokpod como referência de implementação Angular do DS Altivy, conforme pedido humano anterior nesta conversa; não importar automaticamente regras de negócio, edição Business ou dependências do Dokpod.

A necessidade D8 é intenção de produto e demanda declarada pelo stakeholder responsável, não pesquisa de mercado ou comparação exaustiva de concorrentes. Não há dados de pagamento, entrevistas ou confirmação factual de ausência de alternativas. O público de pequenas frotas permanece histórico e não pertence ao primeiro MVP autorizado para especificação.

Ionic foi citado na justificativa de portabilidade. Sua função e compatibilidade com o DS próprio devem ser esclarecidas antes de adoção; esta referência não aprova uma biblioteca de UI ou versão sem licença e manutenção verificadas. Compartilhar código não implica paridade automática de recursos em todas as plataformas.