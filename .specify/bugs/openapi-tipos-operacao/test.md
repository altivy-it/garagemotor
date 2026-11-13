# Verificação de Bug: Respostas específicas no OpenAPI

- **Slug**: openapi-tipos-operacao
- **Testado em**: 2026-11-13
- **Avaliação**: [assessment.md](assessment.md)
- **Correção**: [fix.md](fix.md)
- **Resultado**: verified
- **Origem**: IA assistida; verificação autorizada pelo owner.
- **Revisor humano**: CEO da Altivy e owner do GarageMotor.

## Resumo

A reprodução que antes identificava respostas genéricas passou após a correção. Os testes também passaram nos dois contratos extraídos da imagem reconstruída, sem regressões estruturais detectadas.

## Verificações Executadas

Os testes e builds executaram em containers isolados, sem rede; os testes montaram o repositório somente para leitura.

| Verificação | Comando / Ação | Resultado | Notas |
|---|---|---|---|
| Reprodução após correção | Comando e filtro da [avaliação](assessment.md#reprodução) | pass, exit 0 | Dois testes passaram; anteriormente o teste de tipos falhava. |
| Testes novos e regressão | Mesmo comando, sem `--grep` | pass, exit 0 | Oito testes de contrato passaram. |
| Contratos empacotados | Extração com `podman cp`; comparação dos dois JSON e execução da mesma suíte sobre os arquivos extraídos | pass, exit 0 | Os arquivos coincidiram com a origem; oito testes passaram. |
| Formatação | `node node_modules/prettier/bin/prettier.cjs --config .prettierrc.json --check ../tests/contracts/openapi.spec.ts` | pass, exit 0 | Executado em `frontend/web`. |
| Tipos | `node node_modules/typescript/bin/tsc --ignoreConfig --noEmit --target ES2022 --module preserve --moduleResolution bundler --strict --skipLibCheck --types node ../tests/contracts/openapi.spec.ts` | pass, exit 0 | Executado em `frontend/web`. |
| Alterações estruturais | Comparação JSON com o contrato anterior | pass | Mudaram 42 referências; 24 componentes foram adicionados sem alterar os demais campos. |
| Build e smoke da imagem | Comando de build registrado em [fix.md](fix.md#verificação-local); verificação offline de runtime e arquivos | pass, exit 0 | Imagem isolada, sem iniciar a aplicação. |
| Build web | Build Angular de produção em workspace temporário | pass, exit 0 | Nenhuma dependência foi instalada. |
| Espaços no diff | `git diff --check` | pass, exit 0 | Nenhum problema encontrado. |
| Implementação HTTP | Não executada | skipped | O defeito é no contrato proposto; os endpoints de domínio ainda não estão implementados. |

Uma tentativa de análise de tipos sem `--ignoreConfig` falhou com `TS5112`; a repetição acima passou. Uma comparação intermediária incluiu indevidamente `api.md`, que não é um dos dois arquivos publicados pela imagem; a verificação foi refeita para os JSON previstos no Dockerfile.

## Trechos de Saída

```text
Reprodução: 2 passed
Contrato de origem: 8 passed
Contrato empacotado: 8 passed
```

## Riscos Residuais

- A alteração restringe o schema anteriormente declarado; clientes gerados com a união anterior precisam de avaliação de compatibilidade antes da publicação.
- A validação não comprova comportamento HTTP, autorização em runtime ou implementação dos endpoints de domínio.
- A igualdade dos identificadores de membro e os demais critérios da T011 continuam pendentes de evidência.

## Recomendação

Considere o defeito de tipagem do contrato verificado no escopo dos artefatos avaliados. Continue a T011 sem marcá-la concluída até validar seus critérios restantes.
