# Modelo Conceitual: Testes categorizados e inventário de imagens

**Feature**: [spec.md](spec.md)

Esta manutenção não adiciona dados de domínio, schema PostgreSQL ou API. Os modelos abaixo descrevem somente metadata de testes e o inventário de imagens existente.

## Caso de Teste Backend

| Campo conceitual | Regra |
|---|---|
| Identificador | Nome/classe/método descoberto pelo runner xUnit. |
| Categoria | Obrigatória; `Unit` para testes isolados ou `Integration` para testes que usam serviços reais. |
| Execução | Executável individualmente e por filtro de categoria; integração conserva seus pré-requisitos de container/nginx/PostgreSQL. |
| Paralelismo | Testes de integração que compartilham estado ficam na mesma coleção serializada; unitários independentes podem manter paralelismo seguro. |

O conjunto de referência tem 17 métodos: 15 Integration e 2 Unit. A migração não pode reduzir a descoberta sem justificativa aprovada.

## Referência de Imagem

| Campo conceitual | Regra |
|---|---|
| Componente e tag | Identificam a imagem usada para build, testes, proxy ou runtime. |
| Plataforma e digest | Permanecem fixados por plataforma em `deploy/images.lock.json`. |
| Origem/licença/termos | Permanecem quando necessários para rastreabilidade e obrigações de terceiros. |
| SBOM versionado | Não faz parte do modelo de inventário mantido pelo repositório após esta feature. |

## Invariantes

- Todo método de teste backend descoberto tem uma categoria.
- Não se armazenam ou distribuem dados novos por causa desta manutenção.
- Remover dados SPDX derivados não remove pins de imagem nem avisos/licenças de terceiros necessários.
