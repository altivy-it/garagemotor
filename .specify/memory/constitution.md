# Constituição do GarageMotor

<!-- Relatório temporário de impacto; remover antes do commit.
- Versão: 1.4.0 -> 2.0.0 (MAJOR, remoção do gate constitucional de auditoria por camada).
- Princípio III ajustado: imagens continuam fixadas para reprodutibilidade, sem exigir
	aprovação individual de digest/plataforma nem SBOM/auditoria por camada para uso local.
- Seções adicionadas: nenhuma.
- Seções removidas: nenhuma.
- Acompanhamento: alinhar os scripts e o inventário de imagens à regra revisada via Spec Kit.
-->

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

Por decisão humana explícita de 2026-11-08, OpenJDK e ferramentas oficiais Android
podem compor exclusivamente o ambiente de build e testes em containers, mesmo com
licenças ou termos distintos de licença permissiva. Essa exceção não alcança bibliotecas
do app, plugins, runtime distribuído ou serviços comerciais. Antes do uso, devem ser
verificados versão estável mantida, origem, licença/termos, integridade e imagem fixada;
aceites contratuais necessários permanecem humanos, nunca automáticos por agente.
Essas ferramentas não devem ser distribuídas com o app. A exceção permite a cadeia
Android sem enfraquecer a regra do código e não autoriza execução fora de containers,
instalação, publicação ou a cadeia iOS/macOS.

Por decisão humana explícita de 2026-11-09, ficam excepcionalmente aprovados para
download e uso somente como ferramentas de build/teste, em containers, os seguintes
artefatos já fixados nos lockfiles da feature `001-garagem-pessoal`:

- `lightningcss@1.33.0`, licença declarada MPL-2.0, integridade npm
	`sha512-WkUDrojuJs0xkgGf2udWxa3yGBRxPtxUkB79i6aCZLRgc7PM8fZe9TosfPDcvEpQZbuFASnHYmRLBLUbmLOIIA==`.
- `caniuse-lite@1.0.30001823`, licença declarada CC-BY-4.0, integridade npm
	`sha512-TQ5s/djDjNk1axreUW+4rULH+BmdCMLiJkhaXnO5wqivG7tITkSUj4ItIdzqdZHKaXcHAV8dhH3JxOM7/7bwqw==`.
- `Microsoft.Testing.Extensions.CodeCoverage@18.12.0`, `contentHash`
	`BST/sWYpKk9OXO7Yxktcg6FWORRsHU0EdCbMwZ7d3OqJsMKlrNHIZ30g0Ib4k5T5acJYXRBmg9mNrTU/N6cy1g==`.
	A licença exata continua não verificada; a aprovação não afirma compatibilidade
	nem substitui revisão jurídica.
- `lightningcss-linux-x64-gnu@1.33.0`, variante opcional para Linux x64 glibc,
	licença declarada MPL-2.0, integridade npm
	`sha512-ar+Ju7LmcN0Jo4FpL4hpFybwNG9/3A/Br5KW2n2jyODg3MEZXaDYADdemoNS+BDNfMgKvylJLj4S5tyRActuAg==`.

A exceção cobre apenas os identificadores, versões e integridades acima, no uso
de ferramentas de build/teste desta feature. Não permite incorporar esses pacotes
ao runtime, distribuí-los com o aplicativo, aceitar contratos automaticamente ou
adotar outras versões/dependências. Avisos e obrigações aplicáveis, inclusive
atribuição, devem ser preservados. Todas as demais dependências continuam sujeitas
à regra de licença permissiva, gratuita e verificada. A exceção não cria precedente
para outra feature. As demais variantes de plataforma de `lightningcss` presentes
no lockfile não estão incluídas nesta aprovação.

Imagens de container usadas para desenvolvimento, build, testes, banco, proxy ou
runtime podem conter camadas com licenças distintas das dependências incorporadas
ao código da aplicação. Para uso local e self-hosted, não é necessária aprovação
constitucional individual por imagem, digest ou plataforma, nem SBOM ou auditoria
de licença camada a camada como pré-requisito. As referências devem continuar
fixadas em tag/digest no lock aplicável para reprodutibilidade, e mudanças devem
seguir o fluxo normal de revisão e autorização da feature.

O uso de imagens respeita as licenças e os termos publicados por seus fornecedores;
isso não altera a política de licenças das bibliotecas incorporadas ao produto nem
concede direitos adicionais sobre conteúdo de terceiros. Antes de redistribuir ou
publicar uma imagem upstream ou derivada, a tarefa deve verificar as licenças,
preservar avisos exigidos e obter aceites contratuais humanos quando aplicáveis.
SBOMs podem ser geradas quando forem úteis para segurança, manutenção ou distribuição,
mas não são um gate constitucional geral para uso local/self-hosted.

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

Emenda `1.1.0`, autorizada pelo mesmo humano em 2026-11-08: "Sim, formalizar a exceção
restrita na constituição antes de fechar o plano", para ferramentas de build/testes
Android. O incremento MINOR registra a ampliação; não altera a licença do código próprio
ou a autorização necessária para implementar e publicar.

Emenda `2.0.0`, autorizada pelo owner em 2026-11-10, simplifica a governança de imagens:
remove aprovação constitucional individual por digest/plataforma e o gate obrigatório
de SBOM/auditoria de licença por camada para uso local/self-hosted. O incremento MAJOR
registra a remoção de controles incompatíveis com a regra anterior; mantém pins para
reprodutibilidade e obrigações de terceiros em caso de distribuição.

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
jurídica já foram entregues. A regra de dependências permanece, exceto pelas aprovações
pontuais e delimitadas das emendas 1.2.0 e 1.3.0 e pela regra de imagens da emenda 2.0.0;
a exceção de imagens 1.4.0 é histórica e foi substituída.

**Versão**: 2.0.0 | **Ratificada em**: 2026-11-08 | **Última alteração**: 2026-11-10

## Next Actions

- Retomar `/speckit-implement` após revisão e autorização das tarefas para alinhar
	`tools/scripts/validate-feature.ps1`, `tools/scripts/start-local.ps1` e
	`docs/dependencies.md` à remoção do gate obrigatório de auditoria de imagens.
- Antes de qualquer publicação, verificar se os artefatos finais incorporam conteúdo
	ou dados sujeitos a MPL-2.0 ou CC-BY-4.0 e cumprir as obrigações correspondentes.
