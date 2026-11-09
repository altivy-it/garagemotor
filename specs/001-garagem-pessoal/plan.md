# Plano de Implementação: Garagem pessoal e familiar

**Branch**: `001-garagem-pessoal` | **Data**: 2026-11-08 | **Spec**: [spec.md](spec.md)

**Entrada**: Especificação em `specs/001-garagem-pessoal/spec.md`.

**Status**: Fases 0 e 1 concluídas para revisão; implementação, instalação e publicação não autorizadas.

**Origem**: IA assistida. **Revisor humano**: CEO da Altivy e owner do GarageMotor.

**Feature selecionada**: `SPECIFY_FEATURE_DIRECTORY=specs/001-garagem-pessoal`.

**Revisão**: 2026-11-09, após quatro esclarecimentos humanos. Modelo, contratos e quickstart foram alinhados; as tarefas de 2026-11-08 ainda precisam ser revisadas e analisadas. Nenhuma alteração de stack ou autorização de implementação.

## Resumo

Preparar o MVP pessoal/familiar para veículos, abastecimentos, despesas, manutenção, lembretes e comprovantes. Primeira entrega: web, self-hosted e Android. iOS, OCR, chat, predição, pequena frota empresarial e edição proprietária ficam fora.

O fluxo oficial resolveu os bloqueios da pesquisa por fontes/tagged manifests da API GitHub e confirmação humana de direitos do DS. A constituição recebeu emenda 1.1.0 restrita às ferramentas Android, sem liberar bibliotecas incompatíveis ou execução no host. Modelo, contratos e guia de validação foram produzidos; resultados operacionais ainda não foram executados.

Os achados, fontes e bloqueios estão em [research.md](research.md).

## Contexto Tecnico

**Linguagem/Versão**: C#/.NET SDK 10.0.403, runtime/ASP.NET Core 10.0.14 LTS; Angular 22.2.5, CLI/build 22.2.6, TypeScript 6.0.3 e Node 24.22.2 LTS em manutenção. Não usar TypeScript 7, fora de >=6.0 <6.1. Versões fixadas, sem ranges flutuantes nos manifests do produto.

**Dependências Principais**: Capacitor core/cli/android 8.6.0 MIT, Npgsql 10.0.3 permissiva, MSTest SDK/framework 4.5.2 MIT, Playwright 1.65.0 Apache-2.0 e Prettier 3.9.10 MIT. UI própria Altivy/Dokpod; sem Ionic UI, ORM, mediator, axe-core/MPL ou nova biblioteca de componentes. Framework Identity/PasswordHasher para identidade local; criptografia própria proibida.

**Persistência**: PostgreSQL 18.6 e volume privado local, conforme [ADR proposto](../../docs/adr/2026-0001-persistencia-garagem.md). SQL parametrizado, transações, RLS como defesa adicional, partições mensais/default/índices por filho e retenção definida no [modelo](data-model.md). Aceite humano do datastore é gate antes de implementar; não há serviço S3/MinIO ou volume distribuído.

**Testes**: MSTest para domínio/API, integração real PostgreSQL/filesystem, contrato OpenAPI, idempotência/concorrência/partições, Playwright para web/componentes e validação Android. Qualidade: dotnet format/analyzers, Prettier, compilação Angular/TypeScript estrita. Sem segundo framework de unit frontend; Playwright testa primitivas em navegador e lógica compartilhada. Nenhum teste da aplicação executado nesta preparação.

**Plataforma Alvo**: API, frontend, banco, nginx 1.30.6 e runners em containers Linux. HTTP exclusivamente por nginx; Android conectado em API35/36 na matriz inicial. JDK Temurin 21.0.13+9, Gradle 8.14.3, AGP 8.13.0, compile/target API36 e build-tools 36.0.0 somente em containers. Aceite de termos e audit/checksums antes de uso. iOS excluído, sem exceção macOS.

**Tipo de Projeto**: monólito modular com uma API ASP.NET Core, SPA Angular compartilhada com Capacitor e jobs internos HostedService. Módulos Identity/Garages/Vehicles/Expenses/Fuel/Maintenance/Documents/Operations como pastas, não microserviços ou camada de dispatch obrigatória.

**Metas de Desempenho**: SC-005: dez usuários simultâneos, dois minutos de aquecimento excluídos e dez de medição, 80% histórico/20% totais. Conjunto fictício determinístico: 10.000 despesas em 20 garagens, três veículos por garagem e 12 meses; matriz4vCPU/8GiB/SSD. P95 <=2s por tipo, sem erros inesperados; seed, filtros e parâmetros registrados. Paginação50/máximo100 e índices por garagem/veículo/data; não extrapolar para outras instalações.

**Restrições**: AGPL-3.0-only; bibliotecas permissivas e ferramentas Android limitadas pela constituição 1.1.0. Antes de instalar: gerar/resolver lockfiles sem scripts, inventariar pacotes/imagens/transitivas, verificar licenças/avisos/hashes e falhar fechado. Não presumir que tags de fonte são hashes de pacote. Direitos do DS confirmados pelo owner, preservando terceiros. Sem IA, escrita offline, push obrigatório ou edição proprietária.

**Escala/Escopo**: pessoas/famílias, carros/motos, múltiplas garagens por identidade com papéis owner/editor/reader. Histórico financeiro até exclusão, auditoria90dias, exportações/staging24h. Dados ativos/arquivos/backups gerenciados contendo a garagem são eliminados até requestedUtc+30dias, inclusive cópias anteriores/durante o pedido. Operador responde pelas cópias sob gestão, sem garantia sobre cópias particulares externas. Matriz inicial é capacidade a testar, não trava de licenciamento.

## Verificação da Constituição

*GATE: Deve ser aprovado antes da pesquisa da Fase 0 e revisado novamente após o design da Fase 1.*

Verificação inicial e após desenho contra a [constituição 1.1.0](../../.specify/memory/constitution.md):

| Princípio | Situação | Tratamento |
|---|---|---|
| I. AGPL e reciprocidade | Conforme na preparação | Decisão humana registrada; arquivo oficial, avisos e inventário ainda precisam de trabalho autorizado, sem publicar dados/secrets. |
| II. Self-hosted | Conforme no escopo | Núcleo sem assinatura/chave; três modalidades de serviço não aprovam preços ou edição proprietária. |
| III. Tecnologia/DS | Conforme para desenho | Tags, faixas e licenças primárias verificadas; direitos DS e exceção Android confirmados. Audit de artefatos/transitivas/termos antes de uso é gate de execução, sem dispensa. |
| IV. Segurança/integridade | Conforme como requisitos, não validado | Spec cobre autorização, concorrência, evidências e recuperação; threat model e testes reais pertencem ao desenho/implementação autorizados. |
| V. Spec Kit/qualidade | Conforme | Somente artefatos; autorização de implementação ainda necessária. Não avançar Fase 1 com esclarecimentos técnicos pendentes. |

**Gate após pesquisa**: F0-1/F0-2/F0-3 resolvidos para desenho, conforme pesquisa atual. Nenhuma licença presumida ou quebra da exigência de containers foi usada como exceção silenciosa.

**Gate após desenho**: conforme para revisão de tarefas. [Modelo](data-model.md), [OpenAPI](contracts/openapi.json), [schemas públicos](contracts/resources.schema.json), [regras HTTP/mobile](contracts/api.md) e [quickstart](quickstart.md) alinhados aos esclarecimentos. ADR permanece proposed; nova análise das tarefas, threat model/revisão humana e validações reais são obrigatórios antes de implementar. Checagens documentais não certificam comportamento de runtime.

## Estrutura do Projeto

### Documentação desta Funcionalidade

```text
specs/001-garagem-pessoal/
  spec.md
  plan.md
  research.md
  data-model.md
  quickstart.md
  contracts/openapi.json
  contracts/resources.schema.json
  contracts/api.md
  checklists/requirements.md
```

### Código-Fonte (raiz do repositório)
Estrutura a ser criada pelas tarefas, não código existente:

```text
backend/GarageMotor.Api/          API, módulos, SQL stores e jobs internos
backend/GarageMotor.Tests/        MSTest de domínio, API e integração
frontend/web/                    SPA Angular, DS próprio e configuração Capacitor
frontend/web/android/            projeto nativo gerado/controlado
frontend/tests/                  Playwright, contrato e fixtures
deploy/                          Dockerfiles, nginx e manifests de imagens auditadas
tools/scripts/                   orquestração PowerShell, qualidade e recuperação
docs/adr/                        decisões e gates humanos
```

**Decisão de Estrutura**: uma API e dois projetos .NET inicialmente, módulos por domínio e UI compartilhada; stores Npgsql sem ORM. Não copiar agentes, terminal, Keycloak, protocolos de containers ou Business do Dokpod. Sem diretórios/scripts novos em .specify para lógica própria.

## Registro de Complexidade

Exceção Android formalizada separadamente pelo humano na constituição 1.1.0, somente ferramentas de build/testes. O plano não amplia essa exceção. Partições/RLS/staging decorrem de integridade e política do projeto; evitar serviços adicionais e criptografia própria.

## Desenho e segurança

Web é servido pelo nginx, que encaminha somente `/api/v1` à API; banco/volume nunca ficam públicos. Desenvolvimento Angular também fica atrás desse proxy. Android usa assets locais do mesmo build e endpoint HTTPS validado, sem relay do fornecedor. CORS é allowlist explícita, cookies web HttpOnly/Secure, antiforgery do framework e bearer opaco Android; revogação/membership em toda operação.

Credenciais de runtime são injetadas por arquivo externo. Chaves protegidas do framework ficam em volume privado e são protegidas por credencial externa; material plaintext nunca vai para código, fonte publicada ou exportação familiar. Android usa Keystore/APIs da plataforma, não localStorage ou plugin não auditado. Operador/DBA self-hosted é parte confiável da infraestrutura; não prometer isolamento criptográfico contra ele.

Threat model mínimo para revisão humana:

| Superfície/risco | Controle proposto | Evidência exigida |
|---|---|---|
| IDOR/membership revogada | Contexto de garagem, RLS, role sem BYPASSRLS, reautorização por operação | Integração de duas garagens, conexões pooled e revogação |
| Conta/sessão/CSRF | Framework Identity/PasswordHasher/antiforgery, rate limit, token opaco com hash, cookie seguro | Enumeração, recuperação/código, origem e expiração/revogação |
| Arquivo/path/upload | Destino gerado, staging, limites, tipo/conteúdo, download attachment/nosniff | Travessia de path, excesso, falha parcial e acesso cruzado |
| Duplicação/corrupção financeira | Transação, versão, recibo persistente e lock no rollover | Escrita simultânea, repetição após falha e recálculo |
| Endpoint/Android | TLS sem bypass, major compatível, troca limpa de sessão, Keystore | Certificado inválido, instância trocada e perda de rede |
| Backup/exclusão | Snapshot coordenado, checksums, destino vazio, retenção explícita | Restore limpo, cópia corrompida, falha antes de substituir destino |

Controles adicionais após esclarecimento:

- Toda criação de domínio usa Idempotency-Key e recibo transacional; criação de garagem usa escopo do ator antes de existir garageId. Replay preserva status/id/version e reautoriza acesso; conflito de operação/conteúdo retorna409. Recibos não armazenam tokens ou respostas privadas de autenticação.
- Recuperação: 16 bytes aleatórios do framework (128bits), base64url22caracteres, apenas hash persistido e comparação de tempo constante. SMTP15min; lote offline10códigos/12meses; reautenticar para renovar, invalidar lote anterior, revogar sessões e demais códigos após sucesso. Janela móvel de cinco falhas/conta15min e teto operacional100/origem15min; contagem concorrente e identidade inexistente não podem permitir enumeração ou bloqueio permanente.
- BackupCatalog/DeletionAuthority no controle operacional existente, sem novo datastore/broker. Pedido fixa deadline30dias após requestedUtc; catálogo antecipa prazo de todas as cópias contendo a garagem e comprova remoção. Restauração nunca substitui autoridade atual pelo dump antigo: origem autenticada ou exportação operacional independente verificável, cobertura desde criação da cópia até promoção e coordenação de sequência. Ausência/corrupção/cobertura parcial/atualidade não demonstrada bloqueiam promoção; hash só não comprova atualidade.
- Reaplicar pedidos pertinentes antes de tornar dados restaurados públicos e invalidar sessões/códigos antigos. Garantia é para backups sob gestão informada, não cópias privadas fora do alcance do operador. Verificação não depende obrigatoriamente de SaaS.
- DTOs públicos são schemas fechados por kind em resources.schema.json; negar propriedade extra/privada, tipo errado ou kind diferente do esperado para operationId. Lote offline é resposta excepcional não cacheável, sem dados em exportação/recibos/logs.

Revisão de segurança e auditoria WCAG 2.2 AA são gates; Playwright verifica teclado/foco/estados e subsets automatizáveis, não certifica sozinho toda conformidade AA. Não adicionar axe-core/MPL sob a regra permissiva sem nova decisão de governança.

## Sequência para tarefas

Correção da ordem de preparação: (1) pins/inventário inicial e gates humanos; (2) manifests mínimos de projetos/package, sem executar ou instalar dependências; (3) resolução de metadados/lockfiles em container isolado, sem secrets nem scripts de pacotes; (4) auditoria transitiva, hashes/avisos e então permissão de restore/install/build nas tarefas autorizadas. Lockfiles NuGet ficam ao lado de cada csproj, não numa pasta backend sem projeto. Não exigir resolução final antes de existir o manifesto que a determina; se a ferramenta executa targets arbitrários ou não permite isolar essa etapa, interromper e revisar o procedimento, sem contornar o gate.

1. Preparação: ratificação do ADR, inventário/lockfiles/imagens/avisos, licença oficial e reconciliação MIT, fundamentos de deploy/qualidade, threat model.
2. Fundação: autenticação, contexto de garagem, stores/migrations/partições, idempotência, DS compartilhado e runners reais.
3. US1/US2/US3: garagem/veículos, custos e abastecimentos com tests primeiro no recorte.
4. US4/US5: planos/lembretes e comprovantes com integridade e recuperação.
5. US6: Android, instalação própria, exportação/backup, carga, compatibilidade e smoke de imagens.
6. Transversal: formato/análise/build/testes, revisão de segurança e acessibilidade, matriz e documentação. Não marcar tarefa como executada sem evidência.

US6 faz parte da entrega, não autorização de publicar app nas lojas. Não iniciar implementação só porque o plano e a spec estão presentes. Próxima fase: `/speckit-tasks`, depois `/speckit-analyze` e revisão/autorização humana explícitas.

A revisão de tarefas deve ampliar T014/testes/criações de US1–US6, campos/testes de recuperação de T016/T017, catálogo/autoridade/restore de T053/T056–T058/T064, validators de respostas de T011 e workload de T063. Esses são pontos de encaminhamento, não tarefas já editadas ou executadas. Reconciliar T003/T005 pela ordem acima e atualizar a referência de hash do contrato.

## Validações e pendências

Checagens em containers: JSON/OpenAPI refs/IDs, estados/decimais, estrutura dos documentos e links. Não foram executados serviços, migrations, unit/integration, carga, Android ou Playwright. Validar o contrato completo na implementação, inclusive DTOs de resposta por kind.

Instalação ainda depende de inventário real e aceites Android; promoção depende de todas as evidências do quickstart. ADR permanece proposto. Preços, orçamento, SLA, prazo, publicação de portal, push e release são decisões separadas. A referência de origem do Dokpod não autoriza copiar materiais de terceiros sem avisos.
