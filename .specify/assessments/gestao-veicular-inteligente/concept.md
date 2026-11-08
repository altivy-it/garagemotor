# Concept: Gestão veicular inteligente

- **Slug**: gestao-veicular-inteligente
- **Created**: 2026-11-08
- **Recommended option**: A — garagem pessoal e familiar com controle de dados self-hosted
- **Origem**: IA assistida
- **Revisor humano**: CEO da Altivy e owner do projeto
- **Estado da revisão**: pendente; recomendação não equivale a escolha humana ou autorização
- **Base**: [problema](problem.md), [pesquisa](research.md) e [ideia original](intake.md)

**Revisão de 08/11/2026**: diretrizes D1–D5 do intake confirmadas pelo owner. Angular 22 e design system Altivy são requisitos humanos; self-hosted passa a integrar a proposta de produto. Preços, licença, canais mobile e escopo de entrega continuam propostas, sem implementação autorizada.

## Options

### Option A — Controle pessoal e familiar

- **Sketch**: pessoa ou família reúne o histórico de despesas e compromissos de seus veículos para entender quanto custa mantê-los e o que precisa de atenção. A menor experiência útil prioriza acompanhamento confiável, não um catálogo de recursos ou gestão empresarial.
- **Appetite**: `medium` (semanas), somente como hipótese de orçamento para descoberta e eventual piloto limitado. Equipe, prazo e orçamento ainda não informados; não é estimativa de entrega.
- **Trade-offs**: hipótese de menor complexidade operacional e ciclo de decisão mais curto; hipótese de menor disposição a pagar, abandono por esforço de registro e necessidade de aquisição em escala. Não há evidência confirmando nenhuma dessas vantagens.
- **Rabbit holes**: compartilhamento familiar completo, muitas categorias de veículo, gratuidade ilimitada, lançamento simultâneo em lojas, promessa de experiência offline integral e relatórios extensos.

### Option B — Controle operacional enxuto de pequenas frotas

- **Sketch**: responsável de uma pequena empresa acompanha gastos e compromissos de seus veículos, enquanto os envolvidos entregam registros que possam ser conferidos. O foco é reduzir esforço de consolidação e permitir decisões com informações rastreáveis, não substituir todo o sistema de gestão da empresa.
- **Menor solução que poderia funcionar**: uma empresa de um segmento definido acompanha veículos, abastecimentos, despesas e manutenções, consulta totais básicos e compromissos próximos, e mantém comprovantes associados quando necessários. Compartilhamento fica limitado às responsabilidades demonstradas na pesquisa; separação e acesso autorizado são indispensáveis se houver mais de uma organização.
- **Appetite**: `medium` (semanas), como orçamento candidato de validação/piloto de um único recorte. Não estima a construção de SaaS completo; quantidade de organizações, veículos e duração dependem de decisão humana.
- **Trade-offs**: hipótese de valor mais mensurável e comprador identificável; em contrapartida, exige confiança nos números, conferência, acesso correto, onboarding e suporte. Pode ter venda mais lenta que uso pessoal. Nenhum ganho comercial foi demonstrado.
- **Rabbit holes**: transportadoras de grande porte, ERP, rastreamento em tempo real, roteirização, folha de condutores, cooperativas/máquinas, operação multiempresa completa, regras corporativas avançadas e cobrança automática prematura.

### Option C — Usar soluções existentes e não construir agora

- **Sketch**: manter ou contratar uma solução disponível e observar um processo real dos públicos candidatos. A Altivy aprende quais dificuldades permanecem sem assumir custo de produto antes de demonstrar oportunidade.
- **Appetite**: `small` (dias), orçamento candidato para comparação inicial; observação longitudinal pode exigir mais tempo. Sem compromisso aprovado.
- **Trade-offs**: preserva recursos e acelera aprendizado; pode limitar diferenciação, controle e receita própria. Adequação das alternativas ainda precisa ser verificada, inclusive identificação correta de Fuel.io.
- **Rabbit holes**: customizar tanto uma ferramenta externa que a validação se torne um projeto de integração; escolher fornecedor sem verificar licença, termos e proteção de dados.

## Histórico da recomendação para pequenas frotas

Recomendar **B como hipótese preferencial de produto**, não como seleção já aprovada. O motivo é sua relação potencial com as métricas do problema: tempo de consolidação, qualidade dos gastos e identificação de comprador podem ser observados num processo operacional delimitado. Esta relação é suposição, não evidência de mercado superior [research.md, Users & Demand e Market & Context].

Primeiro comparar A e B com o mesmo padrão de evidência e registrar as soluções atuais. Se a Altivy não tiver acesso a pequenos frotistas, ou se a pesquisa não demonstrar dor e compromisso comercial, priorizar C; se A produzir evidência melhor, reavaliar a recomendação. Não iniciar construção com base nesta preferência.

Posicionamento candidato para B: **controle confiável de custos e manutenção para pequenas frotas, com menos esforço de registro**. "Com IA" só deve tornar-se promessa de produto se reduzir esforço sem prejudicar correção ou margem. O nome de trabalho permanece GarageMotor; não há decisão de rebranding.

## Delimitação do conceito

Todos os itens a seguir são propostas para avaliação, não requisitos aprovados:

| Parte da ideia | Tratamento candidato | Justificativa |
|---|---|---|
| Cadastro e tipos de veículo | Identificar veículos do segmento escolhido; dados sensíveis apenas quando necessários | Evitar coletar placa, Renavam e chassi indiscriminadamente |
| Abastecimento e consumo | Histórico conferível; métricas com indicação de cobertura e insuficiência de dados | Não apresentar consumo como preciso com dados parciais |
| Manutenção e alertas | Compromissos por data ou uso segundo regras conhecidas e responsabilidade explícita | Lembrete não é predição de desgaste nem garantia de manutenção |
| Despesas e visão financeira | Totais por veículo, categoria e período com origem rastreável | Evitar dupla contagem e custo/km sem distância coberta |
| Comprovantes/documentos | Associar evidências necessárias ao processo validado | Cofre documental completo depende de demanda e revisão de privacidade |
| Relatórios | Visão básica e saída necessária ao processo observado | Excel e PDF avançados dependem de demanda, custo e licença verificável |
| OCR de recibos e painel | Experimento posterior ao baseline, com confirmação humana antes do lançamento | Medir tempo total incluindo correção; avaliar por campo e impedir duplicatas |
| Chat financeiro | Adiado até demonstrar uso e autorização de acesso aos dados | Um total conferível pode resolver a pergunta sem chat ou IA |
| Predição de manutenção | Fora do primeiro recorte | Exige dados e validação de segurança ainda inexistentes |
| Web, Android e iOS | Angular 22 obrigatório; distribuição mobile candidata com Capacitor e canais oficiais | Confirmar plugins estáveis, assinatura, políticas de loja e cadeia iOS; não presumir offline integral |
| Self-hosted empresarial | Diferencial requerido pelo owner; incluir avaliação de instalação própria e suporte comercial | Precisa de matriz suportada, recuperação, atualização e sustentabilidade econômica; não é implementação autorizada |

## Modelo comercial candidato

- Em B, testar cobrança por organização ou por veículo com compradores reais. Valores originais em reais são referências para experimento, não tabela aceita; medir suporte, armazenamento, eventual IA, tributos e margem antes de prometer preço.
- Em A, verificar disposição a pagar antes de escolher assinatura Personal/Family/Pro. Preços em dólares não comprovam adequação ao mercado candidato brasileiro.
- Não oferecer "ilimitado" sem política de capacidade, operação e custos. Piloto deve ter limites explícitos acordados, sem converter limite de serviço em restrição incompatível com a licença MIT do código.
- Community autogerida e suporte comercial self-hosted passam à proposta abaixo, por direcionamento humano. Demanda, preço, SSO corporativo e contratos públicos continuam sem validação.

## Out of Scope (for the recommended option)

- Clone ou promessa de superar Fuel.io sem comparação verificável.
- Atender simultaneamente pessoa, família, pequena frota, grande frota e governo.
- Máquinas agrícolas, telemetria, GPS, roteirização, integrações com ERP e validação fiscal automática.
- Chat e previsão mecânica no primeiro recorte; lançamento financeiro autônomo por OCR.
- Cofre documental genérico, biblioteca de relatórios avançados e todas as integrações de identidade propostas.
- Kubernetes, LDAP/SAML, faturamento empresarial completo ou arquitetura multiempresa detalhada; self-hosted não está mais excluído, mas não implica essas capacidades.
- Comprometer .NET, Capacitor, PostgreSQL, Redis ou MinIO como arquitetura aceita nesta avaliação. Angular 22 e o DS Altivy são diretrizes humanas confirmadas; patch, compatibilidade, licenças e demais decisões estruturais pertencem ao planejamento autorizado.
- Tarefas, APIs, modelos de dados, instalação de dependências e implementação nesta descoberta.

## Assumptions to Validate

1. Pequenas frotas apresentam dor recorrente mais forte ou acesso comercial melhor que pessoas/famílias.
2. Comprador, condutor e responsável financeiro têm incentivo para manter informações úteis e corretas.
3. A comparação de alternativas demonstra uma lacuna que vale construir, em vez de comprar.
4. Tempo, qualidade e disposição a pagar satisfazem critérios revisados do [problema](problem.md).
5. Captura assistida reduz tempo total, incluindo revisão, e cabe no custo de servir; dados são autorizados.
6. Dados, armazenamento e compartilhamento podem respeitar privacidade, licenças e regras locais.
7. Orçamento, equipe, cobertura de plataformas e apetite podem ser acordados para um recorte pequeno.

## Próxima etapa

`/speckit-assess-decide slug=gestao-veicular-inteligente`

## Diretrizes de stack e design system

Esta seção registra restrições e candidatos; não desenha arquitetura, APIs, modelos ou tarefas.

| Camada | Direção atual | Condição para adoção |
|---|---|---|
| Frontend | **Angular 22 obrigatório**, mesmo fora de LTS | Fixar patch, confirmar disponibilidade e matriz; não substituir por outra major sem decisão humana |
| Design system | **Altivy obrigatório**, fonte oficial em `/home/u_zocatel/projetos/Altivy/docs/brand/design-system/` | Referência de app complementar em `docs/design/app/project/ds/altivy/`; validar direitos de reutilização e adaptação Angular |
| Backend | .NET 10 como candidato LTS, já verificado quanto ao suporte | Fixar SDK/runtime e versões compatíveis no planejamento |
| Banco | PostgreSQL como candidato, em versão estável suportada | Não presumir que existe linha LTS oficial; fixar major/patch mantidos e justificar adoção no processo local |
| Mobile | Capacitor como candidato em versão estável mantida | Licença da versão exata, plugins, suporte a Angular 22 e versões de plataformas ainda não verificados |
| Ferramentas, bibliotecas e imagens | LTS quando disponível e adequado; caso contrário, estável mantida | Versões fixadas, atualização de segurança, compatibilidade e licença permissiva verificada; nada com tag flutuante ou prerelease por conveniência |
| Cache e documentos | Sem Redis, MinIO ou outro componente aprovado nesta rodada | Escolher apenas se houver necessidade, licença compatível e decisões exigidas pelo repositório |

O DS prescreve Outfit 400/500/600, dark-first, liquid glass, tokens semânticos e teal-light. Para app, priorizar shell e conteúdo denso, painéis opacos para tabelas, foco visível, estado com texto/ícone e movimento reduzível; não transformar a aplicação em landing page. O tema claro de app exige validação própria. Fonte local evita dependência de Google Fonts, respeitados seus direitos de redistribuição.

SOURCE do Altivy informa versão sincronizada `1792958125-fac8` e ausência dos componentes de app na pasta oficial. Não há biblioteca Angular pronta confirmada: reutilização significa manter linguagem e contratos visuais, não copiar React ou instalar dependência sem licença. Nenhum arquivo do Altivy será alterado nesta descoberta. Divergências com regras locais devem ser resolvidas explicitamente no planejamento.

## Distribuição Android e iOS

**Recomendação candidata**: um cliente mobile oficial para SaaS e self-hosted, com experiência Angular empacotada por Capacitor se sua compatibilidade e licença forem verificadas. Não lançar um binário público diferente para cada empresa por padrão.

| Canal | Proposta | Limite |
|---|---|---|
| Android público | Google Play, com pacote AAB assinado | Conta, políticas, assinatura, target API e aprovação a verificar; publicação não autorizada |
| Android privado | APK assinado ou distribuição empresarial gerenciada | Cliente assume política de instalação; prever origem confiável e atualização, sem tratar sideload como equivalente ao Play |
| iOS público | App Store; TestFlight somente para beta | Exige cadeia Apple, revisão e manutenção; TestFlight não é entrega permanente de produção |
| iOS empresarial | App customizado para organizações pelos canais Apple elegíveis | Validar elegibilidade e contratação; Enterprise interno para empregados não é canal genérico para clientes externos |
| Web self-hosted | Acesso pelo navegador; experiência instalável/PWA a avaliar | Alternativa a validar, não promessa de paridade de câmera, push ou offline com app nativo |

O cliente oficial deve poder selecionar a instância SaaS ou uma instância própria identificada com clareza, mediante política de endereço confiável, TLS, autenticação e compatibilidade. Não depender de relay, licença online ou conta SaaS para acessar servidor próprio. Servidor em rede privada pode exigir VPN ou acesso corporativo controlado; não impor exposição pública como única opção.

Self-hosted significa que backend e dados podem operar sob controle do cliente. Não significa independência automática de Apple/Google: push, lojas e determinados serviços externos precisam ser documentados como dependências opcionais ou limitações. Não anunciar air-gap nem IA local sem validação; a indisponibilidade de push/OCR não deve impedir o núcleo de registros e consultas.

**Bloqueio de execução iOS**: Xcode e assinatura requerem macOS; container Linux não resolve essa cadeia. O repositório exige build e testes em containers. O planejamento deve demonstrar execução compatível ou obter autorização humana explícita para uma exceção de runner macOS devidamente isolado no fluxo aplicável. Não considerar macOS, VM ou Xcode Cloud automaticamente conformes à regra de containers. Até resolver o bloqueio, não executar build iOS fora da regra.

Serviços pagos e contratos de distribuição Apple/Google não são licenças de bibliotecas do app. Ainda exigem decisão de orçamento e verificação de termos. Compras no app, cobrança web e links externos precisam de análise por território e modalidade; nenhuma estratégia de contornar comissão ou revisão foi aprovada.

## Histórico da recomendação MIT e limites comerciais

**Recomendação: MIT para todo código próprio do GarageMotor**, coerente com a regra atual. A adoção formal de arquivo de licença, titularidade e avisos exige revisão humana e jurídica; nenhum arquivo de licença foi criado ou modificado nesta rodada.

MIT permite comercialização e self-hosted, mas também permite forks comerciais, redistribuição e modificações fechadas, observados os avisos exigidos. Não protege exclusividade de hosting e não obriga concorrentes a devolver melhorias. Não vender assinatura como se fosse obrigação legal de usar o código.

Cobrar por serviço é compatível com essa direção: operação SaaS, implantação, manutenção, suporte, SLA, treinamento e assistência de migração. Não criar limite legal de 20 veículos, trava de execução ou expiração do direito ao código para Community MIT. Limites de plano delimitam recursos contratados ou cobertura de suporte, não revogam licença do software.

Separar código/licença, marca, termos SaaS, privacidade e contrato de suporte. Distribuições modificadas não recebem automaticamente direito de usar marca ou alegar suporte oficial. Dependências, fontes, assets e DS mantêm seus próprios avisos e direitos; a licença do código principal não os relicencia.

Se o objetivo comercial exigir impedir terceiros de revender ou hospedar o código, **MIT não atende a esse objetivo**. Isso conflita com a regra vigente e precisaria de decisão humana de governança antes de outro modelo; esta avaliação não propõe nem adota licença proprietária, copyleft ou source-available incompatível.

## Comercialização self-hosted e SaaS

Três ofertas do mesmo produto, com receita por conveniência e responsabilidade operacional. Valores em reais são **hipóteses para teste no Brasil**, não preços publicados, país escolhido ou contratos aprovados.

| Oferta | Cliente candidato | O que contrata | Preço hipotético |
|---|---|---|---|
| Community self-hosted | Empresa com capacidade própria de TI | Código MIT, instalação autogerida e documentação; sem SLA ou operação Altivy | Gratuito, sem limite legal de veículos |
| SaaS gerenciado | Pequena frota sem interesse em operar infraestrutura | Aplicação operada pela Altivy, capacidade contratada, atualização, backups e suporte definido | R$ 299/mês até 10 veículos; R$ 15 por veículo adicional/mês |
| Self-hosted assistido | Empresa que exige controle de dados e quer assistência | Implantação, matriz suportada, suporte operacional e acompanhamento de atualização; infraestrutura do cliente | R$ 4.500 de implantação + R$ 24.000/ano de serviço |

A hipótese de self-hosted assistido cobre uma instância de produção e uma de homologação, numa matriz acordada, com até duas horas mensais de atendimento consultivo. Incidentes, prazo de resposta, correções de produto, horas excedentes e assistência a customizações precisam de cláusulas próprias; não confundir franquia consultiva com limite para cumprir um SLA contratado. Cobertura inicial de até 100 veículos serve para dimensionar o contrato, **não para bloquear o código**. Maior porte, requisitos especiais e operação gerenciada recebem orçamento específico.

### Premissas financeiras

- SaaS com 25 veículos: `299 + (25 - 10) × 15 = R$ 524/mês` de receita bruta hipotética. Com custo mensal hipotético de R$ 200, contribuição antes de tributos, aquisição e despesas fixas seria R$ 324, aproximadamente 61,8%; não é margem líquida nem estimativa de custo verificada.
- Self-hosted assistido: custo anual hipotético de R$ 9.000, incluindo capacidade de suporte reservada, preparação de releases suportadas e atendimento, deixaria R$ 15.000 de contribuição sobre R$ 24.000, ou 62,5%, antes de tributos, aquisição e despesas fixas. Confirmar custo/hora e capacidade real antes de prometer SLA.
- Implantação tem escopo fechado, checklist de aceite e preço separado. Infraestrutura, banco, backups executados pelo cliente, certificados, VPN, contas de loja e consumo de IA não ficam escondidos no valor anual.
- Excedentes de suporte, armazenamento SaaS e IA precisam de política explícita e preço validado, sem promessas de ilimitado. IA fica opcional e não pode ser requisito de funcionamento do núcleo self-hosted.
- Não publicar preços ou margens sem revisão humana. Validar poder de compra, custo total de propriedade e alternativas existentes; os exemplos não demonstram disposição a pagar.

### Responsabilidades de operação

- No Community, o cliente instala, protege, monitora, atualiza e recupera seus dados; a ausência de SLA não equivale a garantia de ausência de falhas.
- No SaaS, a Altivy responde pela operação contratada; retenção, recuperação, exportação e prazo de atendimento devem constar dos termos.
- No self-hosted assistido, o cliente responde pela infraestrutura e acesso; Altivy presta assistência dentro da matriz e escopo acordados. Definir responsável por backup, teste de restauração, atualização, incidentes e customizações, sem responsabilidade implícita.
- A matriz inicial deve ser enxuta: distribuição em containers e caminho de instalação simples como candidato; Kubernetes não é requisito inicial. Não prometer instalação em qualquer sistema, nuvem ou banco.
- Definir versões suportadas, janela de atualização, compatibilidade com app mobile e migração. Término do suporte pago não bloqueia uso do código MIT, mas encerra serviços contratados conforme contrato.

### Aquisição e validação comercial

Comparar pessoas/famílias e pequenas frotas conforme o problema antes de escolher o mercado. Para a hipótese B, buscar comprador com dor concreta de consolidação e, separadamente, exigência demonstrável de controle de dados; self-hosted não deve ser vendido apenas por preferência técnica da Altivy.

O caminho comercial candidato é descoberta do processo, demonstração focada, piloto com objetivo e preço explícitos, medição do benefício e proposta de SaaS ou suporte self-hosted conforme responsabilidade desejada. Pilotar não autoriza implementação fora do SDD. Evitar customizações específicas antes de confirmar padrão replicável.

Revisar com o humano os critérios do problema: dor observada, redução de esforço, correção financeira, uso recorrente e compromisso de pagamento. Medir também horas de onboarding/suporte, custo de servir, interesse efetivo em self-hosted e objeções ao modelo MIT. Um contrato de suporte validado é sinal comercial; simples aceitação do design ou stack não é.

## Histórico da mudança de direção

Antes deste complemento, self-hosted estava fora do primeiro recorte e Angular era candidato. D2 e D4 do intake, fornecidas pelo owner em 08/11/2026, substituem essas posições. A proposta original de apetite `medium` refere-se à validação limitada, não à entrega de web, dois apps e self-hosted completos. Reestimar o apetite conceitual após informar equipe, orçamento e requisitos operacionais; nenhuma data de lançamento foi assumida.

## Recommendation

**Opção A, por escolha humana de 08/11/2026**: garagem pessoal e familiar em pt-BR, com controle de custos e manutenção e possibilidade de operar os próprios dados. O owner confirmou essa necessidade e autorizou preparação de spec, plano, tarefas e análise, sem implementação. Esta escolha substitui a preferência anterior por pequenas frotas, preservada no histórico.

O menor produto útil acompanha veículos, abastecimentos, despesas, manutenção, lembretes e comprovantes de uma pessoa ou família, com acesso compartilhado autorizado, cálculos conferíveis e histórico recuperável. A primeira entrega inclui web, self-hosted e Android. Não depende de pagamento, IA ou serviço de licença online para usar o núcleo aberto.

Apetite ainda não estimado: `medium` acima era orçamento candidato de descoberta, não prazo para o novo MVP. Não comprometer data ou custo antes do plano e da revisão humana.

## Licenciamento e referência visual atuais

**AGPL-3.0-only** é a decisão humana atual, formalizada na constituição 1.0.0; a seção MIT é histórica. Manter autogerido gratuito, SaaS e suporte self-hosted como modalidades, sem aprovar preços, edição proprietária ou duplo licenciamento. Cumprir condições de distribuição e oferta de fonte das versões modificadas com interação remota. Dados dos usuários e secrets não fazem parte dessa publicação.

O DS Altivy permanece obrigatório; **Dokpod passa a ser referência de implementação Angular**, sem importar automaticamente funcionalidades empresariais, dependências ou exceções de licença. Angular 22 continua obrigatório; demais componentes devem ser LTS/estáveis mantidos. Ionic citado pelo humano precisa de enquadramento explícito sem trocar a identidade visual por um kit genérico.

## Escopo atual para o SDD

- Dentro: núcleo pessoal/familiar confirmado; pt-BR; web e Android; self-hosted; compartilhamento e acesso autorizado; indicadores básicos; evidências de despesas; recuperação e continuidade do histórico.
- Fora: pequenas frotas empresariais, grande frota, governo, máquinas agrícolas, telemetria, roteirização, IA/OCR, chat, previsão mecânica, iOS nesta entrega, Kubernetes e edição proprietária.
- A entregar por tarefas próprias: licença oficial e avisos, reconciliação das regras MIT, referência DS/Dokpod, matriz de versões e licenças, instalação, backup/recuperação e validação nos níveis exigidos.
- A revisar antes de implementar: spec, plano, tarefas, análise, capacidade suportada, riscos e premissas. Não há autorização de push ou publicação.

Suposições: unidade familiar como fronteira de compartilhamento, dados financeiros em BRL e distâncias em quilômetros; limites exatos e comportamentos de edição/exclusão devem ser testáveis na spec. Sem promessa de offline integral, push sem infraestrutura externa ou paridade total entre plataformas.