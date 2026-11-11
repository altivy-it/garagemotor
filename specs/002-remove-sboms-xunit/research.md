# Pesquisa: Remoção de SBOMs e adoção do xUnit

**Feature**: [spec.md](spec.md)
**Data**: 2026-11-10
**Origem**: IA assistida. **Revisor humano**: CEO da Altivy e owner do GarageMotor.

## Decisões

### xUnit e Microsoft.Testing.Platform

**Decisão**: usar o pacote `xunit.v3` versão `4.0.3`, que instala o suporte `xunit.v3.mtp-v2`; manter a seleção MTP do `global.json` e os comandos `dotnet test` do projeto. Classificar cada método com o trait `Category=Unit` ou `Category=Integration`, filtrar via `--filter-trait` e preservar a não paralelização das integrações compartilhadas.

**Justificativa**: a documentação oficial do xUnit informa que xUnit v3 integra nativamente MTP e que versões de pacote 4.x usam MTP v2. A integração MTP de `dotnet test` no SDK 10 usa a configuração já existente em `global.json`. A documentação também apresenta `--filter-trait` como filtro por metadata de teste. O nuspec NuGet de `xunit.v3` 4.0.3 declara Apache-2.0, exige .NET 8 ou posterior e instala MTP v2; os nuspecs de `xunit.v3.mtp-v2`, `xunit.v3.core` e `xunit.analyzers` também declaram Apache-2.0. Isso é compatível com .NET 10 e com a política de dependências do projeto.

**Alternativas consideradas**:

- xUnit v2 com VSTest: rejeitado porque não aproveita o runner MTP selecionado pelo projeto e adicionaria um caminho de execução distinto.
- Manter MSTest e apenas acrescentar categorias: rejeitado por contrariar o pedido explícito do owner.
- Instalar adaptador VSTest adicional: não planejado; a documentação oficial diz que ele não é necessário quando se usa apenas MTP. Só reconsiderar se a matriz de IDEs aprovada revelar incompatibilidade concreta.

**Fontes oficiais**:

- [xUnit.net: Microsoft Testing Platform](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform), consulta em 2026-11-10.
- [NuGet: xunit.v3 4.0.3](https://api.nuget.org/v3-flatcontainer/xunit.v3/4.0.3/xunit.v3.nuspec), consulta em 2026-11-10.
- [NuGet: xunit.v3.mtp-v2 4.0.3](https://api.nuget.org/v3-flatcontainer/xunit.v3.mtp-v2/4.0.3/xunit.v3.mtp-v2.nuspec), consulta em 2026-11-10.
- [NuGet: xunit.v3.core 4.0.3](https://api.nuget.org/v3-flatcontainer/xunit.v3.core/4.0.3/xunit.v3.core.nuspec), consulta em 2026-11-10.
- [NuGet: xunit.analyzers 2.3.0](https://api.nuget.org/v3-flatcontainer/xunit.analyzers/2.3.0/xunit.analyzers.nuspec), consulta em 2026-11-10.

**Gate restante**: restaurar o lock em container e auditar todos os pacotes transitivos pelo NuGet oficial antes de usar; nenhuma conclusão sobre transitive closure é inferida apenas da licença dos pacotes diretos.

### Categorias e preservação dos testes

**Decisão**: `Category=Unit` para testes sem serviços externos e `Category=Integration` para testes com PostgreSQL/HTTP reais. Atualizar os perfis `unit`, `integration` e `load` de `tools/scripts/validate-feature.ps1` para a sintaxe de traits MTP. A base atual contém 17 métodos: 15 de integração e 2 unitários.

**Justificativa**: essa taxonomia cobre todo o conjunto atual, mantém filtragem simples e não mistura testes que precisam de infraestrutura. A suite de segurança, persistência, auditoria e recibos usa estado compartilhado; a migração deve substituir `[DoNotParallelize]` por coleções xUnit equivalentes, sem enfraquecer isolamento.

**Alternativas consideradas**: categorias por classe são insuficientes onde um arquivo reúne testes unitários e de integração; portanto, cada método deve receber category explícita ou pertencer a classe com trait inequívoco, validado por teste/inspeção de descoberta.

### Remoção de SBOMs do repositório

**Decisão**: excluir os 12 arquivos `deploy/sboms/*.spdx.json.gz`; retirar do lock de imagens os caminhos, hashes de artefatos SBOM, atestação/camada e estatísticas derivadas da análise SPDX que deixarem de ser mantidas; substituir a seção de inventário de camadas em `docs/dependencies.md` por uma nota objetiva de política. Preservar `digestsByPlatform`, referências de imagem, origem e dados de licença/termos necessários.

**Justificativa**: a constituição 2.0.0 já dispensa SBOM/auditoria por camada como gate geral para uso local/self-hosted e declara SBOMs opcionais quando úteis. A documentação de dependências é o local operacional adequado para informar que este repositório não as mantém. A recomendação de resposta coordenada em `.github/SECURITY.md` permanece aplicável “quando aplicável” e não aponta para SBOM versionado.

**Alternativas consideradas**:

- Manter os arquivos e apenas remover sua obrigatoriedade: rejeitado porque o pedido explícito é excluí-los do repositório.
- Remover digests e inventário de imagens junto com os SBOMs: rejeitado porque pins permanecem necessários à reprodutibilidade.
- Alterar a constituição: não necessário; a regra 2.0.0 já contempla o uso opcional de SBOMs, portanto esta feature documenta a decisão de manutenção sem emenda constitucional.
