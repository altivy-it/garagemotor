# Pesquisa Técnica: Garagem pessoal e familiar

**Feature**: [spec.md](spec.md). **Data**: 2026-11-08.
**Status**: Fase 0 incompleta; F0-1/F0-2/F0-3 bloqueiam a Fase 1.
**Origem**: IA assistida, incluindo pesquisa delegada somente leitura.
**Revisor humano**: CEO da Altivy e owner do GarageMotor.

## Decisões confirmadas

### Público e distribuição

- **Decisão**: pessoas/famílias; web, self-hosted e Android primeiro; iOS posterior.
- **Justificativa**: esclarecimento humano e [handoff](../../.specify/assessments/gestao-veicular-inteligente/decision.md).
- **Alternativas consideradas**: pequena frota inicial e lançamento simultâneo com iOS; não selecionadas pelo humano.

### Frontend e integração mobile

- **Decisão**: Angular 22 obrigatório; Angular + Capacitor sem obrigação de componentes Ionic; DS Altivy com Dokpod como referência Angular.
- **Justificativa**: escolha humana de 2026-11-08, registrada nas premissas da spec.
- **Alternativas consideradas**: Ionic Angular como base de UI; não obrigatório. Não substituir DS por biblioteca genérica.
- **Limite**: decisão de direção não verifica versões, compatibilidade ou licenças exatas.

### Licenciamento e serviços

- **Decisão**: AGPL-3.0-only para código próprio; autogerido, SaaS e suporte self-hosted são modalidades de serviço.
- **Justificativa**: decisão humana e constituição 1.0.0. Licença permite comercialização cumprindo seus termos.
- **Alternativas consideradas**: MIT anterior é histórico; Business proprietária do Dokpod não é decisão para GarageMotor.
- **Limite**: dependências seguem regra permissiva e verificada. Não assumir que AGPL raiz concede todos os direitos de assets/fontes ou aprova copyleft adicional.

## Achados da pesquisa local delegada

O agente Explore foi acionado pela Fase 0, somente leitura e sem consultas Web, instalação ou acesso a secrets. Consultou arquivos concretos do Dokpod e retornou:

- SDK .NET 10.0.400 na configuração local; backend usa várias dependências com versões centralizadas, mas suas licenças não estão inventariadas completamente.
- Frontend declara Angular 22.2.3, Node `>=24.22.1 <25` e TypeScript `~6.0.2`; não foi encontrado lockfile npm. Essas declarações não comprovam resolução, suporte ou licença da versão exata.
- DS próprio contém button, card, charts, checkbox, data-grid, drawer, empty-state, field, format, icon, key-value, kpi, progress, search, select, status, tabs e toast, com tokens/styles separados.
- ADR próprio do Dokpod registra derivação Altivy, vidro limitado e WCAG 2.2 AA. Isso informa a referência, não transfere aprovação de dependências ou ADRs ao GarageMotor.
- Outfit/JetBrains Mono têm avisos OFL locais; `axe-core` é MPL-2.0 e somente de teste no Dokpod. Esse aceite não vale automaticamente aqui, onde a política de dependências permanece restritiva.
- Dokpod está em prévia, sem release estável. Seus componentes não devem ser adotados como biblioteca externa estável sem avaliação de direitos, proveniência, manutenção e testes.

## Candidato backend confirmado parcialmente

- **Decisão candidata**: .NET 10 LTS, SDK 10.0.403 e runtime 10.0.14.
- **Justificativa verificável**: metadata oficial Microsoft respondeu HTTP 200, com canal `10.0`, tipo `lts`, fase `active` e fim de suporte `2028-11-14`.
- **Alternativas consideradas**: SDK local Dokpod 10.0.400; a referência local não substitui atualização da linha suportada.
- **Limite**: metadata de suporte não constitui auditoria de licenças de todo o backend ou de imagens. Nenhum pacote foi instalado.

## Bloqueios da Fase 0

| ID | Decisão pendente | Evidência faltante | Consequência |
|---|---|---|---|
| F0-1 | Versões fixadas e compatíveis de Angular 22, Node/TypeScript, Capacitor e ferramentas de teste | Metadata oficial e requisitos de compatibilidade não puderam ser obtidos | Não instalar, fixar versões por palpite ou produzir matriz mobile como validada |
| F0-2 | Licenças exatas e direitos de DS, driver e ferramentas | Licenças de candidatos Npgsql/frontend não verificadas; componentes Dokpod exigem proveniência própria | Regra local bloqueia adoção; não importar dependências/avisos automaticamente |
| F0-3 | Banco, armazenamento e particionamento | Versão PostgreSQL estável suportada não confirmada; driver e decisão de datastore pendentes | Não produzir schema, migrations ou contrato definitivo antes da pesquisa/ADR correspondente |

Os bloqueios são técnicos, não exigência de provar receita antes de especificar. A necessidade declarada pelo owner sustenta a spec; ela não permite omitir verificação constitucional no plano.

## Fontes e resultados

Hosts não previstos na allowlist da descoberta foram autorizados explicitamente pelo humano nesta rodada. As conexões foram fixadas a IP público validado, com validação de peer e TLS. Não foram seguidos redirecionamentos ou instruções remotas. Falha de conexão não informa conteúdo nem licença da fonte.

| Fonte sanitizada | Host | Resultado |
|---|---|---|
| https://registry.npmjs.org/@angular%2fcore/22.2.3 | registry.npmjs.org | Conexão encerrada; também falhou na checagem pelo host |
| https://registry.npmjs.org/@angular%2fcompiler-cli/22.2.3 | registry.npmjs.org | Conexão encerrada |
| https://registry.npmjs.org/@capacitor%2fcore/latest | registry.npmjs.org | Conexão encerrada; também falhou na checagem pelo host |
| https://registry.npmjs.org/@capacitor%2fandroid/latest | registry.npmjs.org | Conexão encerrada |
| https://registry.npmjs.org/typescript/6.0.2 | registry.npmjs.org | Conexão encerrada |
| https://registry.npmjs.org/@playwright%2ftest/latest | registry.npmjs.org | Conexão encerrada |
| https://www.postgresql.org/support/versioning/ | www.postgresql.org | Timeout de TLS |
| https://dotnetcli.blob.core.windows.net/dotnet/release-metadata/10.0/releases.json | dotnetcli.blob.core.windows.net | HTTP 200; metadata .NET consultada |
| https://api.github.com/repos/npgsql/npgsql/contents/LICENSE?ref=v10.0.0 | api.github.com | Conexão encerrada; também falhou na checagem pelo host |

O endpoint `latest` foi apenas tentativa de descoberta de versão, não tag flutuante aprovada para instalação. Fonte Microsoft é mutável: metadata consultada em 2026-11-08. Registrar versões e hashes reais na conclusão da pesquisa.

## Próximas verificações

Conseguir acesso verificável às fontes aprovadas ou fornecer inventário confiável de versão/licença com proveniência. Resolver os três bloqueios, rever o gate constitucional e somente então executar a Fase 1 de `/speckit-plan`. Não há contratos, tarefas executáveis ou análise SDD completa enquanto a pesquisa estiver incompleta.