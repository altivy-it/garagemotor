# Avaliação de Bug: Respostas genéricas no OpenAPI

- **Slug**: openapi-tipos-operacao
- **Criada em**: 2026-11-13
- **Origem**: texto colado — divergência identificada por IA assistida durante a T011 da feature 001; avaliação autorizada pelo owner.
- **Revisor humano**: CEO da Altivy e owner do GarageMotor. A autorização da avaliação não aprova a correção.
- **Veredito**: valid
- **Severidade**: medium

## Relato (literal ou resumido)

O teste existente de contrato aceitava que as criações de domínio retornassem a união de 14 recursos, sem verificar se cada operação declarava apenas o tipo correspondente. A T011 exige essa associação por `operationId`.

## Sintoma

O OpenAPI declara que `createGarage` pode retornar qualquer variante de `Resource`, embora o contrato exija somente `GarageResource`. O mesmo componente genérico é usado por outras operações específicas e coleções; a união é intencional apenas onde o contrato a prevê, como `searchGarageRecords`.

## Reprodução

1. Na raiz do repositório, com Podman rootless, dependências já disponíveis e a imagem Playwright fixada no lock, execute o teste em container sem rede e com o repositório somente para leitura:

   ```sh
   podman run --rm --pull=never --network=none --userns=keep-id \
     -v "$(pwd -P):/workspace:ro" -w /workspace/frontend/web \
     mcr.microsoft.com/playwright:v1.65.0-noble@sha256:bc4fbb329a25287ed23e9216c97063646abdd1bf2da669a9895fa3e2078b2302 \
     node node_modules/@playwright/test/cli.js test \
     --config ../tests/playwright.config.ts contracts/openapi.spec.ts \
     --grep 'all twelve domain creation' --output /tmp/garagemotor-t011-red --workers=1
   ```

2. O teste atualizado falha em `createGarage`: encontra os 14 kinds em vez de somente `garage`. Antes da atualização, os quatro testes existentes do arquivo passaram, apesar da divergência.
3. A inspeção da imagem local `localhost/local/garagemotor-api:package-smoke` confirmou que as cópias dos dois contratos coincidem com os arquivos de origem. Isso verifica o conteúdo empacotado, não uma publicação HTTP nem uma resposta da aplicação.

## Caminhos de Código Suspeitos

- [`openapi.json:253`](../../../specs/001-garagem-pessoal/contracts/openapi.json#L253) — `createGarage` usa a resposta compartilhada `Item`.
- [`openapi.json:1658`](../../../specs/001-garagem-pessoal/contracts/openapi.json#L1658) — `Item` referencia o `Resource` genérico; `Collection` usa a mesma união em seus itens.
- [`resources.schema.json:1154`](../../../specs/001-garagem-pessoal/contracts/resources.schema.json#L1154) — `Resource` é um `oneOf` com 14 variantes.
- [`api.md:15`](../../../specs/001-garagem-pessoal/contracts/api.md#L15) — define o tipo por operação, coleções e a exceção de busca.
- [`openapi.spec.ts:143`](../../../frontend/tests/contracts/openapi.spec.ts#L143) — o teste atualizado verifica os kinds efetivamente permitidos.
- [`Dockerfile.api:29`](../../../deploy/Dockerfile.api#L29) — copia ambos os contratos para a imagem de produção.

## Hipótese de Causa Raiz

**Confiança: high.** As respostas reutilizáveis `Item` e `Collection` apontam para a união genérica, em vez de schemas específicos por domínio. A descrição textual menciona o `operationId`, mas não restringe o schema consumido por validadores e geradores de cliente.

## Correção Proposta

**Preferencial**: ajustar o OpenAPI para que respostas individuais e itens de coleções de cada operação de domínio referenciem o recurso específico definido em `api.md`. Preservar status, headers, estrutura das coleções e referências externas relativas; manter a união de recursos na busca de registros e o schema próprio da busca de auditoria.

Ampliar os testes de contrato para percorrer criações, consultas e mutações relevantes, comprovando que um kind de outro domínio não é aceito. Validar também a cópia efetivamente empacotada. Não alterar handlers ainda não implementados nem relaxar os schemas para fazer os testes passarem.

**Arquivos Prováveis de Alteração**:

- `specs/001-garagem-pessoal/contracts/openapi.json`
- `frontend/tests/contracts/openapi.spec.ts`
- `specs/001-garagem-pessoal/tasks.md`, apenas para registrar evidência validada da T011 quando seus demais critérios forem atendidos.

**Testes a Adicionar ou Atualizar**:

- Verificar os kinds de todas as operações específicas, inclusive os itens das coleções e as 12 criações com `Idempotency-Key`.
- Verificar que a busca de registros mantém a união e que a busca de auditoria mantém seu schema separado e a restrição owner-only documentada.
- Rejeitar exemplos com kind trocado, propriedades extras ou privadas e tipos incorretos; manter as demais verificações de nulabilidade e entrada discriminada exigidas pela T011.
- Executar a validação tanto nos contratos de origem quanto nos arquivos extraídos da imagem de produção, com o `$ref` externo resolvido.

## Riscos e Considerações

- Restringir um schema público pode afetar consumidores gerados a partir da união anterior. Avaliar compatibilidade e clientes existentes antes de publicação; a correção não autoriza release ou deploy.
- A aplicação atual registra endpoints de identidade e informações da instância; esta avaliação não demonstra que endpoints de garagem estejam implementados nem que haja vazamento ou falha de runtime.
- A imagem precisa ser reconstruída após alterar os contratos para que a validação do pacote não use uma cópia antiga. Não é necessária migração de banco para a alteração proposta.
- A T011 permanece aberta: esta reprodução não valida seus demais critérios de contrato e acessibilidade.

## Perguntas em Aberto

- Nenhuma decisão de produto foi identificada para alinhar o OpenAPI ao contrato existente. A correção exige revisão e autorização humana separadas.
