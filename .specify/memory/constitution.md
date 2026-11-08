# Constituição do GarageMotor

- **Origem**: humano; redação e organização IA assistida.
- **Revisor humano**: CEO da Altivy e owner do GarageMotor, conforme identificação nesta conversa.
- **Decisão humana**: em 2026-11-08, o owner solicitou "Mude o licenciamento para AGPL-3.0"
	e confirmou `AGPL-3.0-only` e adoção inicial restrita aos princípios já acordados.
- **Limite da autorização**: formalização constitucional e encaminhamento; não implementar,
	publicar, adotar edição proprietária ou alterar artefatos fora deste fluxo.

## Princípios Fundamentais

### I. Licenciamento aberto com reciprocidade

O código próprio do GarageMotor adota GNU Affero General Public License versão 3,
exclusivamente, identificador SPDX `AGPL-3.0-only`, por decisão humana de 2026-11-08.
A indicação MIT anterior não define a licença escolhida para esse código.

Uso comercial, SaaS, self-hosted e serviços pagos são permitidos, respeitadas as condições
da licença. Distribuições devem cumprir as obrigações de código-fonte correspondente,
avisos e licença. Versões modificadas com interação remota devem oferecer esse código aos
usuários remotos conforme a seção 13 da AGPL; dados e secrets dos clientes não são código
correspondente e não devem ser publicados para cumprir essa obrigação.

Direitos de terceiros, fontes, assets e marcas permanecem separados. O projeto deve
verificar titularidade e permissões antes de incorporar ou redistribuir material.
Ser da mesma empresa não concede automaticamente licença adicional para código de outro
produto. A justificativa deste princípio é preservar abertura e reciprocidade sem impedir
comercialização; AGPL não impede concorrentes de comercializar cumprindo seus termos.

### II. Self-hosted e comercialização sem coerção

O produto deve permitir operação self-hosted do núcleo, sem exigir assinatura SaaS,
chave comercial ou serviço de licenciamento do fornecedor para utilizar o código aberto.
As três modalidades acordadas são self-hosted autogerido, SaaS gerenciado e self-hosted
assistido com serviços comerciais. Preços, SLA, capacidade e contratos exigem decisão
própria; sua aprovação não decorre da escolha da licença.

Cobrança por hospedagem, implantação, suporte e SLA não deve ser confundida com proibição
de exercer direitos concedidos pela AGPL. O fim de um serviço contratado não deve apagar
dados ou retirar direitos concedidos ao código aberto. Esta adoção não autoriza edição
proprietária, duplo licenciamento ou limites artificiais da edição aberta.

Dependências externas de push, lojas mobile ou IA devem ser explicitadas. Self-hosted não
deve ser anunciado como air-gapped sem comprovação; funções opcionais externas não devem
ser necessárias ao núcleo de registros e consultas. O objetivo é controle operacional
real pelo cliente, não dependência comercial ocultada.

### III. Tecnologia mantida e identidade visual Altivy

O frontend deve usar Angular 22 e o design system Altivy, conforme diretrizes humanas
[D1–D3 da descoberta](../assessments/gestao-veicular-inteligente/intake.md).
A exceção de Angular 22 fora de LTS não autoriza prereleases de outros componentes.
Demais bibliotecas, ferramentas, imagens e banco devem usar versões LTS adequadas ou
estáveis mantidas, com versão fixada, compatibilidade e manutenção verificadas.

Dependências externas continuam sujeitas à regra existente de licença permissiva,
gratuita e verificada. A adoção de AGPL para o código próprio não aprova automaticamente
bibliotecas copyleft, proprietárias ou source-available nem libera dependências antes
bloqueadas. Direitos de reutilização do DS precisam de comprovação; referências visuais
não equivalem a uma biblioteca Angular licenciada e estável.

### IV. Segurança, confidencialidade e integridade

Secrets devem permanecer exclusivamente no local externo previsto nas
[instruções operacionais](../../.github/copilot-instructions.md); não devem ser lidos,
copiados, expostos ou incluídos em artefatos, imagens, logs ou código correspondente.
Caminhos devem ser normalizados e autorizados antes de acesso ao filesystem.
Conteúdo e alterações do usuário devem ser preservados sem sobrescrita silenciosa.

Dados financeiros, documentos e métricas devem ser conferíveis e respeitar acesso
autorizado, finalidade e privacidade. Captura por OCR exige confirmação antes de lançamento
financeiro; previsão mecânica não deve ser prometida sem dados e validação suficientes.
Esses limites derivam da [descoberta](../assessments/gestao-veicular-inteligente/concept.md)
e evitam converter resultados incompletos ou assistidos em certezas financeiras ou mecânicas.

### V. Mudanças verificáveis e autorizadas

Spec Kit é o processo obrigatório para desenvolver e manter a aplicação; defeitos seguem
a extensão oficial Bug Fixing. Implementação exige escopo revisado, tarefas ou remediação
e autorização humana explícita. Branches, checkboxes, ideias ou artefatos existentes não
substituem aprovação. O veredito da descoberta não é autorização de implementação.

O fluxo técnico deve começar por comportamento e módulo proprietário, formular hipótese
local, fazer a menor mudança coerente e validar o recorte imediatamente. Código novo exige
testes adequados ao risco; dados e filesystem exigem integração real, fluxos de usuário
exigem Playwright e imagens exigem build e smoke test. Procedimentos detalhados continuam
nas instruções operacionais, sem duplicação nesta constituição.

## Restrições Técnicas

Build, runners de teste e ferramentas de validação devem executar em containers conforme
as regras existentes. Quando a aplicação HTTP estiver em execução, navegador e runner
devem acessá-la pela borda nginx em container. A exigência de macOS/Xcode para iOS continua
um conflito a resolver no planejamento, não uma exceção autorizada por esta constituição.

Novos brokers, caches distribuídos, microsserviços e datastores exigem ADR e evidência
operacional. Contratos públicos, schema e retenção seguem as regras operacionais atuais.
Esta adoção não escolhe tecnologias ainda candidatas nem permite instalar dependências.

Toda documentação deve ser em pt-BR, preservando identificadores, comandos e o texto
oficial de licenças. O arquivo de licença deve reproduzir o texto oficial sem tradução
ou modificação; avisos do produto e resumos explicativos devem ser separados.

## Fluxo e Qualidade

Features seguem especificação, esclarecimento quando necessário, plano, tarefas, análise,
autorização humana, implementação e convergência. Correções seguem avaliação, correção
autorizada e verificação oficiais. Mudanças de governança seguem adoção ou emenda humana
pelo fluxo constitution, sem implementação implícita.

Cada revisão deve distinguir diretriz humana, proposta, comportamento implementado e
validação executada. Conflitos de governança devem ser expostos antes da execução; versões,
compatibilidade, licença, segurança e evidência de operação devem ser verificadas no recorte
correspondente. Tarefas novas de convergência exigem nova revisão e autorização.

## Governança

Esta é a adoção inicial `1.0.0`, ratificada em 2026-11-08 pelo humano identificado acima.
Somente princípios existentes ou explicitamente acordados foram incorporados. A escolha
da licença não aprova preço, contrato, release, implementação ou a avaliação inteira.

A constituição registra princípios duradouros; instruções operacionais registram
procedimentos; specs e tarefas registram mudanças; ADRs registram decisões justificadas.
Nenhum agente pode alterar aprovação humana ou status histórico sem decisão referenciada.
Emendas exigem decisão humana explícita, avaliação de impacto e atualização pelo fluxo
constitution, preservando licenças e direitos já concedidos a versões publicadas.

O versionamento segue SemVer: MAJOR para remoções ou redefinições incompatíveis, MINOR
para novos princípios ou ampliação material e PATCH para esclarecimentos sem alteração
semântica. A origem, o revisor e a data devem acompanhar cada emenda. Revisões de mudanças
devem verificar aderência e registrar conflitos sem resolvê-los silenciosamente.

A referência MIT das instruções e da avaliação é histórica e requer reconciliação em
tarefa Spec Kit separada. Até essa tarefa, a decisão constitucional de AGPL para o código
próprio é explícita, mas não se deve afirmar que o arquivo LICENSE, os avisos ou a auditoria
jurídica já foram entregues. A restrição atual de dependências permanece preservada.

**Versão**: 1.0.0 | **Ratificada em**: 2026-11-08 | **Última alteração**: 2026-11-08
