# Correção de Bug: Respostas específicas no OpenAPI

- **Slug**: openapi-tipos-operacao
- **Corrigido em**: 2026-11-13
- **Avaliação**: [assessment.md](assessment.md)
- **Estado**: applied
- **Origem**: IA assistida; correção e verificação autorizadas pelo owner.
- **Revisor humano**: CEO da Altivy e owner do GarageMotor.

## Resumo

As respostas de domínio passaram a declarar o recurso correspondente a cada operação. A busca de registros mantém a união dos 14 tipos, e a busca de auditoria conserva seu schema próprio.

## Alterações

| Arquivo | Alteração | Notas |
|---|---|---|
| [openapi.json](../../../specs/001-garagem-pessoal/contracts/openapi.json) | Modificado | 31 respostas individuais e 11 coleções referenciam componentes tipados; status, cabeçalhos e envelopes foram preservados. |
| [openapi.spec.ts](../../../frontend/tests/contracts/openapi.spec.ts) | Testes ampliados | Validação por operação, exemplos inválidos, schemas públicos, recargas e auditoria. |

## Testes Adicionados ou Atualizados

- `all twelve domain creation and other operations declare only their own resource kind` — verifica os tipos por operação e os cabeçalhos ETag.
- `search retains the resource union while domain responses reject other kinds and private fields` — verifica a união completa, tipos trocados, propriedades privadas, tipos incorretos e paginação.
- `public resources are closed and member identifiers use UUID schemas` — verifica os 14 recursos, atributos fechados e schemas UUID de membros.
- `nullable response attributes remain required and reject incompatible types` — verifica nulabilidade e campos obrigatórios.
- `electric charge inputs discriminate home and public costs` — verifica as entradas de recarga.
- `audit search is owner-only and excludes changed values and private data` — verifica a declaração owner-only e o schema fechado da auditoria.

## Verificação Local

- Antes da correção, o comando de reprodução da [avaliação](assessment.md#reprodução) executou dois testes: um passou e o teste de tipos falhou nas respostas genéricas.
- Após a correção, os oito testes de contrato passaram em container Playwright, sem rede e com o repositório somente para leitura. Prettier e a análise TypeScript estrita do teste passaram.
- A comparação estruturada com o contrato anterior confirmou 42 referências alteradas e 24 componentes novos; os demais campos e os templates de resposta, exceto os tipos de recursos, permaneceram iguais.
- O build de produção Angular passou em workspace temporário dentro de container.
- Na raiz do repositório, com as imagens fixadas já disponíveis e sem acesso à rede, o comando `podman build --pull=never --network=none --layers --tag localhost/garagemotor-openapi-991de1bb:verify --file deploy/Dockerfile.api --target production .` reconstruiu uma imagem de teste isolada.
- O smoke offline confirmou usuário não root, runtime e arquivos necessários legíveis. Os dois contratos extraídos da imagem coincidiram byte a byte com a origem e passaram nos mesmos oito testes, incluindo resolução do `$ref` externo. A aplicação não foi iniciada nem houve teste HTTP.

## Desvios da Avaliação

Nenhum.

## Acompanhamento

- Executar `/speckit-bug-test slug=openapi-tipos-operacao`.
- A restrição do schema pode afetar clientes gerados com a união anterior; avaliar compatibilidade antes de qualquer publicação.
- A T011 continua aberta: a igualdade entre `MemberResource.id`, `attributes.userId` e o identificador da rota não foi comprovada pelos testes estruturais, e os demais critérios da tarefa ainda precisam de validação.
