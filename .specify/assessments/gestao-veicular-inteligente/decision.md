# Decision: Gestão veicular inteligente

- **Slug**: gestao-veicular-inteligente
- **Decided**: 2026-11-08
- **Verdict**: needs-clarification
- **Artifacts reviewed**: [intake.md](intake.md), [research.md](research.md), [problem.md](problem.md), [concept.md](concept.md)
- **Origem**: IA assistida
- **Revisor humano**: CEO da Altivy e owner do projeto
- **Estado da revisão humana**: diretrizes D1–D5 do intake confirmadas pelo owner; revisão do produto, modelo comercial, licença e execução permanece pendente
- **Autorização recebida**: descoberta completa, comparação dos públicos e atualização de intake, research, concept e decision preservando o histórico, nesta conversa

## Scorecard

| Criterion | Rating | Justification |
|---|---|---|
| Problem validity | unknown | Problema articulado, mas sem entrevistas, observação de processos ou frequência medida; problem.md e research.md. |
| Evidence strength | weak | Proposta humana e confirmação de suporte do .NET 10 não sustentam demanda, diferenciação ou pagamento; fontes de produto não verificadas; research.md. |
| Value vs. inaction | unknown | Tempo, perdas, disposição a pagar e custo de servir não medidos; comprar ou manter solução atual permanece alternativa; problem.md. |
| Feasibility / appetite | unknown | Angular 22, DS e política LTS/estável definidos pelo humano; equipe, orçamento, cronograma, matriz mobile e bloqueio de execução iOS permanecem sem resolução; concept.md. |
| Strategic fit | unknown | Direção de stack, DS Altivy e self-hosted confirmada pelo owner; README raiz ausente e constituição com placeholders ainda impedem confirmar visão ratificada. Regras operacionais locais continuam obrigatórias. |
| Risk posture | weak | Riscos financeiros, privacidade, OCR, licenças, direitos do DS, cobrança nas lojas, suporte self-hosted e incompatibilidade da cadeia iOS com a regra de containers permanecem sem mitigação validada; research.md e concept.md. |

## Verdict & Rationale

**needs-clarification**. A ideia é suficientemente concreta para investigação, mas não para `go`: a força das evidências é `weak`, e o fluxo exige pelo menos `adequate`. A ausência de evidência não sustenta `kill` nem demonstra que o mercado é inviável; requer descoberta dirigida antes de comprometer recursos.

A opção B, pequenas frotas com foco em custos e manutenção, permanece **hipótese preferencial**, não decisão aceita. A e B devem ser comparadas antes da escolha, como solicitado pelo humano. O valor candidato é reduzir esforço e aumentar confiabilidade; IA não é requisito de diferenciação até demonstrar benefício medido.

Na revisão de 08/11/2026, o humano determinou Angular 22, DS Altivy e LTS ou estável para os demais componentes, além de self-hosted como diferencial. Essas diretrizes estão registradas, mas não elevam força de evidência comercial. A recomendação de licença é MIT com receita por serviços, condicionada à revisão jurídica e adoção humana; não houve troca ou criação de licença.

Nenhuma implementação, branch, commit, instalação de dependência, alteração de constituição, spec, plano ou tarefa foi autorizada por este veredito. Não há hooks de avaliação configurados em `.specify/extensions.yml`; os hooks de outros fluxos não se aplicam à descoberta.

## If needs-clarification

| Questão bloqueante | Evidência ou resposta necessária | Etapa a retomar |
|---|---|---|
| B1 — Dor e acesso comercial | Identificar pessoas/famílias e pequenos frotistas acessíveis; observar ocorrências concretas e alternativas atuais, usando os critérios candidatos revisados do problema | research |
| B2 — Público, mercado e comprador | Escolher público somente após comparação; registrar país, segmento, porte, comprador e responsável pelos registros | define |
| B3 — Vantagem e preço | Confirmar identidade/URL do concorrente; comparar o processo escolhido; testar preço explícito e distinguir interesse verbal de compromisso/pagamento | research |
| B4 — Apetite e plataformas | Informar equipe, orçamento, prazo e sequência de web/Android/iOS/self-hosted; revisar o apetite considerando distribuição e suporte, sem presumir entrega simultânea | shape |
| B5 — Dados, risco e medição | Definir acesso autorizado a registros/amostras, baseline, critérios de sucesso, responsabilidades de privacidade e limites de recomendações | research e define |
| B6 — Alinhamento e restrições | Confirmar visão do GarageMotor, direitos de reutilização do DS e adoção da licença; verificar patches e compatibilidade de Angular 22 obrigatório, demais versões LTS/estáveis e licenças exatas | research e shape; constituição apenas por fluxo próprio e autorização |
| B7 — Operação e comercialização mobile/self-hosted | Resolver cadeia iOS/macOS versus containers; validar canais e cobrança nas lojas, endpoint próprio, matriz suportada, custos de servir e contrato de suporte sem restringir direitos MIT | research e shape; exceções operacionais exigem decisão humana explícita |

- **Blocking questions**: B1–B7 acima; decisões D1–D5 resolvem parte da direção técnica, não as evidências e condições restantes.
- **Revisit stage**: `research` primeiro; depois `define`, `shape` e `decide` conforme a evidência obtida.
- **Critério para reavaliar**: problema e evidências ao menos `adequate`, uma opção recomendada confirmada e incógnitas impeditivas resolvidas. Metas de amostra e piloto são candidatas, não critério estatístico de mercado nem aprovação automática.
- **Possível mudança de direção**: se A demonstrar valor melhor, revisar o conceito; se alternativas existentes atenderem suficientemente ou o custo superar o benefício, considerar C e `kill` para construção neste momento.

## Limite de encaminhamento

Não há handoff para `/speckit-specify`, porque o veredito não é `go`. Após nova pesquisa e revisão humana, repetir a avaliação; somente um `go` fundamentado encaminha ao SDD. Mesmo nesse caso, implementar exige spec, plano, tarefas, análise e autorização humana explícita no fluxo do repositório.

## Validações e limitações

- Cada artefato foi submetido a checagem estrutural focada em container; a validação final cobre integridade dos links locais, headings únicos e consistência do veredito com evidências fracas.
- Consultas públicas foram limitadas às fontes e à política registradas na pesquisa; não houve entrevistas, piloto comercial ou avaliação de OCR.
- Não foram executados build, testes da aplicação ou Playwright: esta rodada altera apenas artefatos de descoberta, sem implementação ou preview.
- Origem IA assistida e revisor humano estão registrados; nenhuma revisão pendente foi apresentada como aprovação.

## Próxima etapa

`/speckit-assess-research slug=gestao-veicular-inteligente`

Entrada sugerida: contatos nos dois públicos, exemplos autorizados do processo, URL oficial do concorrente, interesse real em self-hosted e resposta à proposta MIT + serviços. Informar também equipe, orçamento e condições de execução iOS. Dados pessoais e secrets não devem ser incluídos em artefatos do repositório.

## Histórico de revisão de 08/11/2026

A primeira rodada tinha seis bloqueios, Angular como candidato e self-hosted fora do recorte inicial. Após solicitação e autorização explícitas do owner, o intake recebeu complemento humano, a pesquisa recebeu fontes locais e Apple, o conceito passou a incluir distribuição mobile e três ofertas comerciais e esta decisão recebeu o bloqueio B7. O veredito histórico e atual é `needs-clarification`; nenhuma aprovação comercial ou técnica além das diretrizes humanas foi inferida.

### Recomendações submetidas à revisão

- **Código**: MIT, sem teto legal de veículos, trava online ou expiração de direitos por fim de suporte. Serviço comercial e marca têm instrumentos separados; confirmar titularidade e avisos.
- **SaaS**: hipótese de R$ 299/mês até 10 veículos e R$ 15/mês por adicional, sem preço publicado.
- **Self-hosted autogerido**: gratuito, sem SLA; infraestrutura e operação assumidas pelo cliente.
- **Self-hosted assistido**: hipótese de R$ 4.500 de implantação e R$ 24.000/ano por escopo de suporte delimitado, não por direito de executar o código.
- **Mobile**: cliente oficial para SaaS ou instância própria; Play/App Store como canais públicos candidatos, TestFlight para beta e canais empresariais quando elegíveis. Capacitor, plugins, custos e requisitos exatos continuam sujeitos a verificação.
- **DS**: referência Altivy confirmada; fonte da verdade e lacunas de componentes registradas na pesquisa. Não há frontend Angular implementado ou autorização para alterar o Altivy.

O conceito contém simulações de contribuição antes de tributos, aquisição e despesas fixas; não demonstra lucro, validação de preço ou viabilidade. Aprovar preços ou licença não autoriza publicação, e escolher stack não autoriza implementação fora do SDD.