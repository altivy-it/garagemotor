# Problem Definition: Gestão veicular inteligente

- **Slug**: gestao-veicular-inteligente
- **Created**: 2026-11-08
- **Inputs used**: [intake.md](intake.md), [research.md](research.md) e respostas humanas desta conversa
- **Origem**: IA assistida
- **Revisor humano**: CEO da Altivy e owner do projeto
- **Estado da revisão**: recorte pessoal/familiar confirmado pelo owner em 2026-11-08; critérios e implementação aguardam revisão própria

## Problem Statement

Pessoas e famílias precisam acompanhar custos e compromissos de seus veículos em uma experiência brasileira e portátil, podendo manter os próprios dados sob seu controle em instalação self-hosted. Essa necessidade foi declarada pelo owner, que confirmou o recorte para especificação; recorrência da dor, adoção e superioridade às alternativas não foram medidas [intake.md, D6–D8; research.md, Evidência primária].

## Affected Users & Stakeholders

| Grupo candidato | Dificuldade a investigar | Quem pode decidir/pagar | Incógnita principal |
|---|---|---|---|
| Pessoa proprietária/condutora | Esforço para acompanhar gastos e obrigações de um veículo | A própria pessoa | O benefício justifica mudança de hábito e assinatura? |
| Família com veículos compartilhados | Falta de visão comum e atribuição de despesas | Responsável pelo orçamento familiar | Quem registra, consulta e permanece responsável? |
| Pequena frota empresarial | Dados dispersos entre condutores, administrativo e manutenção | Dono ou gestor com orçamento | Qual decisão fica prejudicada e quanto custa o problema? |

Todos os perfis e dificuldades acima são hipóteses, não personas validadas. [NEEDS CLARIFICATION: tamanho de frota, atividade, localização e evidência de cada dificuldade.]

- **Stakeholder proponente**: CEO da Altivy e owner do GarageMotor; autoriza esta descoberta, não implementação.
- **Stakeholders a confirmar**: responsável financeiro, gestor operacional, condutores, pessoas titulares dos dados e eventual responsável por privacidade. [ASSUMPTION]
- **Comprador e usuário**: podem ser a mesma pessoa no uso pessoal e pessoas diferentes numa frota; confirmar responsabilidade e incentivo de cada um. [ASSUMPTION]

## Goals

Resultados candidatos derivados da proposta, sujeitos à validação humana e pesquisa:

- Reduzir tempo e esforço para conhecer despesas e obrigações de cada veículo.
- Aumentar completude, correção e compreensão das informações usadas em decisões financeiras.
- Reduzir obrigações ou manutenções previsíveis esquecidas, sem prometer prevenir falhas mecânicas.
- Identificar um público inicial com dor recorrente e benefício percebido suficiente para pagar.
- Confirmar que o valor entregue excede o custo de adoção, operação e suporte.

## Non-Goals

Limites propostos para esta descoberta, não exclusões de produto já aprovadas:

- Não provar a priori que existe oportunidade maior que a de concorrentes.
- Não resolver simultaneamente todos os públicos, portes de frota e categorias de veículo.
- Não substituir decisões de mecânicos, validação fiscal ou aconselhamento jurídico.
- Não prometer redução de sinistros, desgaste ou custo antes de observar resultados.
- Não decidir nesta etapa tecnologia, arquitetura, planos comerciais ou implantação.
- Não tratar a lista original como requisitos aprovados nem iniciar desenvolvimento.

## Success Metrics

Não há linha de base medida. Os indicadores abaixo respondem aos objetivos do intake; números são critérios candidatos de descoberta/piloto, não metas contratadas ou resultados existentes. [NEEDS CLARIFICATION: revisor validar amostra, metas e horizonte.]

| Indicador | Linha de base | Critério candidato | Como avaliar |
|---|---|---|---|
| Dor recorrente por segmento | Desconhecida | Pelo menos 3 de 5 entrevistados de cada segmento descrevem ocorrência concreta no último mês | Relato específico acompanhado de processo ou evidência autorizada; amostra exploratória, não estatística |
| Esforço para apurar gastos | Desconhecida | Redução de pelo menos 30% no tempo mediano do processo escolhido | Comparar a mesma atividade antes/depois, com dados equivalentes |
| Correção de despesas | Desconhecida | Nenhum valor financeiro incorreto aceito na amostra avaliada | Conferência autorizada com documentos e explicitação de campos ausentes |
| Adoção recorrente | Desconhecida | Pelo menos 2 participantes do segmento escolhido usam o processo por 4 semanas | Observar continuidade e registrar motivos de abandono |
| Disposição a pagar | Desconhecida | Pelo menos 2 compradores concordam com piloto de preço explícito | Distinguir intenção verbal, compromisso e pagamento efetivo; não registrar receita antecipadamente |
| Benefício versus custo | Desconhecida | Custo estimado de servir o piloto inferior à receita proposta, com suporte incluído | Medir esforço operacional e custos; margem-alvo a definir |

Uma amostra pequena serve para descobrir riscos e selecionar novas perguntas, não para demonstrar mercado, retenção de longo prazo ou viabilidade econômica geral.

## Cost of Inaction

Hipótese: quem já enfrenta registros dispersos continuaria gastando tempo para consolidá-los e poderia manter despesas ou obrigações pouco visíveis. Não há valor financeiro, tempo ou incidência medidos; não se atribui perda concreta sem evidência. [ASSUMPTION]

Não construir preserva recursos da Altivy e permite continuar com soluções existentes. Se essas soluções forem suficientes ou a disposição a pagar for baixa, não agir ou comprar pode ser a melhor alternativa [ASSUMPTION; research.md, Evidence Against the Idea].

## Open Questions

1. [NEEDS CLARIFICATION: qual dor recorrente é observada, em qual segmento, e quem pode ser recrutado para compará-los?]
2. [NEEDS CLARIFICATION: qual país, atividade e porte orientam o primeiro público e quem paga?]
3. [NEEDS CLARIFICATION: quais alternativas atuais e concorrente oficial permitem medir vantagem real?]
4. [NEEDS CLARIFICATION: qual orçamento, equipe, apetite de tempo e necessidade imediata de plataformas existem?]
5. [NEEDS CLARIFICATION: quais dados autorizados e responsabilidades de privacidade permitem medir qualidade e custo?]
6. [NEEDS CLARIFICATION: o revisor aceita os critérios candidatos de sucesso ou quais devem ser ajustados?]

## Próxima etapa

`/speckit-assess-shape slug=gestao-veicular-inteligente`

## Recorte confirmado e histórico — 08/11/2026

Os perfis, objetivos comerciais e métricas comparativas acima preservam a primeira rodada da descoberta. Para o encaminhamento atual, o humano escolheu **pessoas e famílias**; pequenas frotas deixam de compor o MVP. A justificativa é autonomia sobre dados, portabilidade e experiência brasileira, não mercado ou pagamento comprovados.

Objetivos atuais: conhecer despesas e compromissos por veículo; reduzir esforço de acompanhamento; manter dados acessíveis e recuperáveis sob controle do usuário; compartilhar responsabilidades familiares sem acesso indevido; preservar a utilidade do núcleo sem assinatura de serviços opcionais.

O núcleo confirmado é veículos, abastecimentos, despesas, manutenção, lembretes e comprovantes. Web, self-hosted e Android pertencem à primeira entrega; iOS, OCR, chat e predição não pertencem a ela. Preços e SLA não estão aprovados. A licença é AGPL-3.0-only conforme a constituição.

Critérios de sucesso da primeira feature devem medir conclusão desses fluxos, correção financeira, isolamento entre famílias, persistência/recuperação e possibilidade de uso independente de SaaS. As linhas de base comerciais permanecem desconhecidas; entrevistas e piloto pago não devem ser apresentados como validação executada.

Questões a levar ao planejamento: capacidade inicial testada, papéis de compartilhamento, retenção, proteção de arquivos, compatibilidade mobile e direitos do DS. Definir premissas conservadoras e gates na especificação e no plano. Orçamento, aquisição e preço permanecem questões de negócio, sem autorização de publicação ou promessa de data.