# Especificação da Funcionalidade: Remoção de SBOMs e adoção do xUnit

**Branch da Funcionalidade**: `002-remove-sboms-xunit`

**Criada em**: 2026-11-10

**Status**: Rascunho

**Entrada**: Remover do repositório os SBOMs que não precisam mais ser mantidos, registrar essa política na documentação e migrar os testes do backend .NET para xUnit, categorizando todos os testes.

**Origem**: solicitação humana. **Revisor humano**: CEO da Altivy e owner do GarageMotor.

## Cenários de Usuário e Testes *(obrigatório)*

### História de Usuário 1 - Executar testes backend por categoria (Prioridade: P1)

Uma pessoa que contribui com o backend pode executar a suíte .NET, filtrar os testes por categoria e confiar que a migração de framework não perdeu cobertura ou capacidade de descoberta.

**Motivo da prioridade**: a suíte backend é um gate recorrente de segurança e integridade; uma migração incompleta pode ocultar ou deixar de executar testes.

**Teste independente**: descobrir e executar todos os testes backend no runner do projeto, conferir que cada teste tem categoria e executar filtros das categorias adotadas.

**Cenários de aceitação**:

1. **Dado** o conjunto atual de testes backend, **Quando** a suíte é descoberta e executada com xUnit, **Então** todos os testes existentes continuam descobertos e executáveis.
2. **Dado** qualquer teste backend, **Quando** sua metadata é inspecionada, **Então** o teste possui ao menos uma categoria estável e pode ser incluído por filtro.

### História de Usuário 2 - Manter o repositório sem SBOMs versionados (Prioridade: P1)

Uma pessoa que mantém o projeto pode encontrar inventário de imagens e regras de dependências sem arquivos SBOM obsoletos ou referências quebradas, entendendo que SBOMs não são mantidos no repositório como requisito geral.

**Motivo da prioridade**: reduz arquivos e manutenção que o owner decidiu não exigir, sem perder a reprodutibilidade das imagens nem os avisos de terceiros aplicáveis.

**Teste independente**: verificar o conteúdo versionado e as referências de documentação/inventário depois da remoção, confirmando que os digests fixados continuam presentes.

**Cenários de aceitação**:

1. **Dado** o inventário e a documentação do projeto, **Quando** a manutenção é concluída, **Então** não há SBOMs versionados nem referências a arquivos removidos, e as imagens continuam fixadas por digest.
2. **Dado** um colaborador consultando a política de dependências, **Quando** procura os requisitos de SBOM, **Então** a documentação em pt-BR informa que o repositório não os mantém como requisito geral e preserva as obrigações de terceiros aplicáveis à distribuição.

### Casos de Borda

- Um projeto de teste pode não ter testes ou uma categoria válida; a validação deve falhar em vez de declarar a migração completa.
- Testes anteriormente sem categoria precisam receber classificação sem perder seu propósito nem mudar indevidamente de perfil.
- Scripts e documentação podem apontar para SBOMs removidos; referências quebradas devem ser detectadas e removidas.
- A remoção de SBOMs não pode apagar pins de imagem, avisos de terceiros ou requisitos aplicáveis se imagens forem distribuídas.

## Requisitos *(obrigatório)*

### Requisitos Funcionais

- **FR-001**: O backend .NET DEVE usar xUnit como framework de testes, conforme solicitado pelo owner, em versão fixada e compatível com o runner do projeto.
- **FR-002**: Todo teste backend descoberto DEVE possuir ao menos uma categoria identificável e selecionável por filtro; nenhum teste pode ser omitido da descoberta para satisfazer a migração.
- **FR-003**: A suíte migrada DEVE preservar os testes existentes e permitir executar a suíte completa e subconjuntos por categoria no ambiente de containers definido pelo projeto.
- **FR-004**: O repositório NÃO DEVE manter arquivos SBOM versionados nem referências quebradas a esses arquivos no inventário, documentação ou scripts.
- **FR-005**: A documentação técnica em pt-BR DEVE declarar que o repositório não mantém SBOMs como requisito geral, sem afirmar que obrigações legais ou avisos de terceiros deixam de se aplicar à distribuição.
- **FR-006**: A remoção de SBOMs NÃO DEVE remover digests/plataformas fixados de imagens, informações de licença/termos de terceiros ou outros controles exigidos pela constituição.

### Entidades Principais

- **Teste backend**: caso automatizado descoberto pelo runner, com uma ou mais categorias que permitem sua seleção.
- **Inventário de imagens**: referências de imagem e seus pins de digest/plataforma, mantidos independentemente dos SBOMs.

## Critérios de Sucesso *(obrigatório)*

### Resultados Mensuráveis

- **SC-001**: 100% dos testes backend existentes antes da migração continuam descobertos e executados ou têm remoção explicitamente justificada e aprovada.
- **SC-002**: 100% dos testes backend descobertos possuem ao menos uma categoria, e cada categoria adotada pode ser filtrada no runner.
- **SC-003**: Nenhum arquivo SBOM permanece versionado e nenhum caminho removido é referenciado em documentação, inventário ou scripts.
- **SC-004**: 100% das imagens anteriormente fixadas continuam com seus pins de digest/plataforma após a limpeza do inventário.
- **SC-005**: A documentação em pt-BR explica a política de não manter SBOMs no repositório e ressalva obrigações de terceiros aplicáveis à distribuição.

## Premissas

- A política solicitada aplica-se aos artefatos SBOM versionados neste repositório; não proíbe gerar SBOM temporário fora dele quando uma necessidade operacional ou de distribuição justificar.
- A migração abrange todos os projetos e testes backend .NET existentes, sem alterar comportamento de produção.
- A licença e compatibilidade da versão exata do xUnit e de seus adaptadores devem ser verificadas durante o planejamento antes de adicionar dependências.
- O formato de categorias deve permitir classificar testes unitários, de integração e outros recortes necessários sem reduzir a cobertura ou misturar testes que exigem serviços reais.
