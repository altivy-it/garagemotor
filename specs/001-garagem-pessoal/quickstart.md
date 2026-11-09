# Guia de Validação: Garagem pessoal e familiar

**Origem**: IA assistida. **Revisor humano**: CEO da Altivy e owner do GarageMotor.
**Estado**: roteiro planejado; aplicação e scripts citados ainda não implementados. Não executar como se já existissem.
**Base**: [spec](spec.md), [modelo](data-model.md), [API](contracts/api.md) e [ADR](../../docs/adr/2026-0001-persistencia-garagem.md).

**Revisão**: 2026-11-09; os comandos e cenários abaixo incorporam os esclarecimentos, mas ainda dependem de scripts/tarefas revisados e implementação autorizada.

## Pré-requisitos e impacto

Diretório de execução: raiz do GarageMotor. Após gerar tarefas e receber autorização humana, os scripts em `tools/scripts/` devem orquestrar containers, não executar build/testes no host. Usar Podman rootless neste ambiente ou Docker compatível, PowerShell e imagens fixadas/auditadas.

Antes de instalar toolchain Android, verificar origem, hashes e licenças e obter aceite humano de termos quando necessário. Nenhum script deve aceitar contratos automaticamente. Build Android/JDK/Gradle ocorre em container; iOS não integra esta matriz.

Secrets de desenvolvimento ficam somente em `~/.microsoft/usersecrets/GarageMotor/.env`, ou no caminho APPDATA definido nas instruções para Windows. Verificar existência, não mostrar conteúdo. Os scripts repassam o caminho por `--env-file` ao container, sem logs integrais, prints de variáveis ou cópias no repositório. Destino de gravação de secrets existente deve causar falha fechada.

HTTP do navegador e dos runners só pela URL do nginx em container, inclusive servidor Angular de desenvolvimento. Não acessar diretamente API ou ng serve no host. Banco/filesystem são reais e isolados em volumes de teste; dados fictícios sem dados de clientes.

## Comandos previstos

Somente após implementação dos scripts pelas tarefas aprovadas:

```powershell
pwsh -NoProfile -File tools/scripts/start-local.ps1 -Engine podman
pwsh -NoProfile -File tools/scripts/validate-feature.ps1 -FeatureDirectory specs/001-garagem-pessoal -Profile quality
pwsh -NoProfile -File tools/scripts/validate-feature.ps1 -FeatureDirectory specs/001-garagem-pessoal -Profile unit
pwsh -NoProfile -File tools/scripts/validate-feature.ps1 -FeatureDirectory specs/001-garagem-pessoal -Profile integration
pwsh -NoProfile -File tools/scripts/validate-feature.ps1 -FeatureDirectory specs/001-garagem-pessoal -Profile web-e2e
pwsh -NoProfile -File tools/scripts/validate-feature.ps1 -FeatureDirectory specs/001-garagem-pessoal -Profile android
pwsh -NoProfile -File tools/scripts/validate-feature.ps1 -FeatureDirectory specs/001-garagem-pessoal -Profile load
pwsh -NoProfile -File tools/scripts/validate-feature.ps1 -FeatureDirectory specs/001-garagem-pessoal -Profile restore
pwsh -NoProfile -File tools/scripts/stop-local.ps1 -Engine podman
```

`start-local` sobe API, web, PostgreSQL e nginx e informa a URL do proxy, sem credenciais. `quality` executa formatador e análise estática/build do recorte em containers, verificação de contrato e avisos. Unit não exige nginx ocioso. Integração/E2E/smoke que usam HTTP exigem nginx. Perfis retornam exit code não zero em falhas e resumos sanitizados. Stop não apaga volumes sem solicitação explícita.

## Aceite por história

| História | Preparação | Ação e resultado esperado |
|---|---|---|
| US1 | Duas contas/garagens fictícias | Cadastro, veículo, convite 72h, aceite/papel/revogação; negar acesso direto cruzado; impedir saída do último owner |
| US2 | Três despesas conhecidas | Totais exatos; correção/anulação sem duplicação; custo/km indisponível quando cobertura não fecha |
| US3 | Tanques cheios em 100.000/100.550 km; final 50 L | 11 km/l; incluir parciais, misturas e leituras justificadas; repetição/conflito não duplicam valores |
| US4 | Plano por data e por uso | Proximidade 30 dias/500 km, primeiro vencimento, conclusão/recorrência sem dupla contagem e leitura defasada identificada |
| US5 | PDF/JPG/PNG fictícios válidos e inválidos | Até cinco/10 MiB; negar excesso, path malicioso, outra garagem e arquivo pendente; download exato autorizado com attachment/nosniff |
| US6 | Instância própria e Android API35/36 | Núcleo sem SaaS/licença online/push/IA; troca de instância limpa sessão; falha de rede é recuperável; backup íntegro restaura em destino vazio |

## Cenários adicionais da sessão de esclarecimento

- Repetir todas as dez criações de domínio com mesma chave/ator/contexto/corpo: mesmo recurso e status, sem valor duplicado. Mesmo contexto/chave com corpo ou operação diferente retorna 409; acesso revogado não recupera resposta antiga. Criar garagem testa contexto do ator sem garageId prévio.
- Recuperação: códigos de ao menos 128 bits/22 caracteres base64url, SMTP15min, dez offline/lote12meses; quinto erro em janela móvel15min limita novas tentativas; origem100falhas/15min também limita. Renovação invalida lote anterior; sucesso revoga sessões/demais códigos; códigos consumidos/expirados/revogados e contas inexistentes falham genericamente. Testar concorrência e rollover da janela sem logs de códigos.
- Respostas: fixtures de cada kind válido e inválido nos [schemas](contracts/resources.schema.json); rejeitar campo privado, propriedade extra, tipo errado, kind incompatível com operationId e lote offline com quantidade distinta de dez. Leitura de recursos nunca devolve segredo de recuperação.
- Exclusão: snapshot anterior ao pedido, cópia criada durante exclusão e conjunto de múltiplos backups. Todos respeitam requestedUtc+30dias; comprovar remoção sob gestão do operador, sem reiniciar prazo pela cópia mais recente. Informar limites para cópias particulares externas.
- Restore: autoridade de exclusões atual separada do snapshot; pedido posterior ao backup é reaplicado antes de tornar os dados acessíveis. Ausência/corrupção/atualidade não verificável bloqueia. Hash íntegro de registro antigo não libera restore. Sessões/códigos do snapshot ficam inválidos, e outras garagens são preservadas.

Para o perfil restore, o script deve exigir fonte atual autenticada ou artefato operacional independente de exclusões e evidência de sua procedência/atualidade. Exemplo de interface futura: `restore.ps1 -BackupPath <copia-ficticia> -DeletionStatePath <estado-operacional-atual> -Target <destino-vazio>`. Esses caminhos são argumentos do operador, não paths confiados sem normalização/autorização, nem secrets copiados do arquivo externo.

Verificar sourceInstanceId/coverageFromUtc/verifiedAtUtc/sequência: cobertura desde a criação do backup até o ponto coordenado de promoção. Testar histórico incompleto, origem que avança a sequência durante restore e artefato íntegro porém antigo; todos exigem recaptura/reaplicação ou bloqueio. Não bastam timestamp declarado ou checksum para afirmar atualidade. Usar estado operacional de fonte confiável sob responsabilidade do operador, não o snapshot da autoridade na própria cópia.

## Verificações transversais

- **SC-001**: medir a jornada inicial em até cinco minutos com usuário preparado; não tratar execução automatizada como estudo de satisfação.
- **SC-002–SC-004**: comparar fixtures exatas, testar duas garagens, SQL parametrizado/RLS, role runtime e conexões pooled; repetir operação após rollover e simular edição concorrente.
- **SC-005**: matriz4vCPU/8GiB/SSD, dez usuários simultâneos; dois minutos de aquecimento excluídos e dez de medição, 80%histórico/20%totais. Dados fictícios determinísticos: 10.000 despesas em 20 garagens com três veículos cada, distribuídas por 12 meses. Seed estável registrada no relatório; escolha de garagem/papel/filtros também reproduzível. Medir P95 <=2s separadamente para cada tipo de consulta pelo nginx, sem erros inesperados; reportar contagens, parâmetros/hardware/versões, não só média global.
- **SC-006**: manifesto/checksums de banco/arquivos, pausa de escrita e snapshot consistente; restauração isolada com autoridade atual separada, reaplicação de exclusões posteriores e recuperação de 100% dos dados ainda autorizados. Destino existente/cópia corrompida/estado de exclusões ausente ou desatualizado falham antes de disponibilizar dados; nenhum segredo operacional é incluído.
- **SC-007**: desktop 1280x800 e mobile 390x844; Chromium/Firefox/WebKit fixados pelo Playwright; capturas, teclado, foco, labels, estados não só cor, movimento reduzido e layouts sem sobreposição. Android qualificado em API35/36 com WebView mantido.
- **SC-008**: desabilitar serviços opcionais e confirmar núcleo; inventário/licenças, fonte correspondente e ausência de secrets/telemetria obrigatória no pacote. Não publicar pacote nesta preparação.
- **Partições**: dados atuais, doze meses anteriores, três futuros e fora da janela; rollover idempotente, migração segura do default, índices por filho e retenção sem remover dados financeiros de outra garagem.

## Operação e falhas

Sinais: erro de autorização, conflito repetido, latência, job de partição falho, arquivos pendentes e backup inválido. Diagnóstico usa traceId e resumo sanitizado. Mitigação mantém escrita bloqueada durante restauração/falha de integridade; não promove destino parcial nem apaga a origem. Recuperação usa cópia verificada e release compatível. Escalonar ao administrador self-hosted ou suporte contratado com dados mínimos, sem logs integrais ou credenciais.

## Validações nesta etapa

Apenas sintaxe/estrutura dos artefatos e links foram verificados. Nenhum serviço foi iniciado e nenhum teste de API, banco, filesystem, Android ou Playwright foi executado. Resultados de aceite dependem de implementação autorizada e execução real deste roteiro.