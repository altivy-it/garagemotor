# GarageMotor

Gestão de veículos pessoais e familiares, abastecimentos, despesas e manutenção, com opção de operação self-hosted.

## Visão e escopo

O primeiro MVP acompanha carros e motos, compartilhamento familiar autorizado, abastecimentos, despesas, manutenção, lembretes e comprovantes. A primeira entrega planejada abrange web, self-hosted e Android; iOS, IA/OCR, chat, predição e gestão empresarial de frotas ficam fora dela.

A definição dos requisitos pertence à [spec](specs/001-garagem-pessoal/spec.md); decisões técnicas pertencem ao [plano](specs/001-garagem-pessoal/plan.md). Esses artefatos ainda exigem revisão humana antes de implementação. Não há aplicação, pacote ou release de produção publicado.

## Princípios

- Código próprio sob AGPL-3.0-only, conforme decisão humana na [constituição](.specify/memory/constitution.md); texto oficial, avisos e auditoria de distribuição ainda devem ser entregues por trabalho autorizado.
- Autogerido gratuito, SaaS e suporte self-hosted como modalidades de serviço; preços e contratos não aprovados.
- Angular 22 e design system Altivy, usando Dokpod como referência de componentes próprios, sem importar automaticamente a edição Business ou suas dependências.
- Demais componentes LTS ou estáveis mantidos, com versões fixadas, proveniência e licenças verificadas. Exceção de ferramentas Android limitada aos termos da constituição.
- Dados e secrets privados não compõem o material de código-fonte publicado; nenhuma licença de serviço é necessária para usar o núcleo aberto.

## Desenvolvimento

Spec Kit é o processo obrigatório: especificação, plano, tarefas, análise, revisão e autorização humana antes de implementação. Build e testes executam em containers; aplicação HTTP é acessada por nginx em container. Secrets permanecem fora do repositório, conforme as [instruções](.github/copilot-instructions.md).

O [repositório público](https://github.com/altivy-it/garagemotor) e o [endereço reservado para o portal](https://altivy.it/produtos/garagemotor/) não indicam disponibilidade de uma release ou publicação do portal.

**Origem**: IA assistida, baseada no escopo confirmado pelo humano. **Revisor humano**: CEO da Altivy e owner do GarageMotor. **Estado**: visão registrada; implementação e publicação não autorizadas.