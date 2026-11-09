# Guia de Validação: Garagem pessoal e familiar

**Origem**: IA assistida. **Revisor humano**: CEO da Altivy e owner do GarageMotor.
**Estado**: roteiro planejado; aplicação e scripts citados ainda não implementados. Não executar como se já existissem.
**Base**: [spec](spec.md), [modelo](data-model.md), [API](contracts/api.md) e [ADR](../../docs/adr/2026-0001-persistencia-garagem.md).

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

## Verificações transversais

- **SC-001**: medir a jornada inicial em até cinco minutos com usuário preparado; não tratar execução automatizada como estudo de satisfação.
- **SC-002–SC-004**: comparar fixtures exatas, testar duas garagens, SQL parametrizado/RLS, role runtime e conexões pooled; repetir operação após rollover e simular edição concorrente.
- **SC-005**: matriz de referência 4 vCPU, 8 GiB RAM e disco SSD, dez usuários simultâneos e 10.000 registros; medir P95 <=2s e reportar hardware/versões sem secrets. É matriz de teste proposta, não garantia universal.
- **SC-006**: manifesto/checksums de banco/arquivos; interrupção de escrita, snapshot consistente, restauração em instância limpa e rejeição antes de substituir destino existente/cópia corrompida.
- **SC-007**: desktop 1280x800 e mobile 390x844; Chromium/Firefox/WebKit fixados pelo Playwright; capturas, teclado, foco, labels, estados não só cor, movimento reduzido e layouts sem sobreposição. Android qualificado em API35/36 com WebView mantido.
- **SC-008**: desabilitar serviços opcionais e confirmar núcleo; inventário/licenças, fonte correspondente e ausência de secrets/telemetria obrigatória no pacote. Não publicar pacote nesta preparação.
- **Partições**: dados atuais, doze meses anteriores, três futuros e fora da janela; rollover idempotente, migração segura do default, índices por filho e retenção sem remover dados financeiros de outra garagem.

## Operação e falhas

Sinais: erro de autorização, conflito repetido, latência, job de partição falho, arquivos pendentes e backup inválido. Diagnóstico usa traceId e resumo sanitizado. Mitigação mantém escrita bloqueada durante restauração/falha de integridade; não promove destino parcial nem apaga a origem. Recuperação usa cópia verificada e release compatível. Escalonar ao administrador self-hosted ou suporte contratado com dados mínimos, sem logs integrais ou credenciais.

## Validações nesta etapa

Apenas sintaxe/estrutura dos artefatos e links foram verificados. Nenhum serviço foi iniciado e nenhum teste de API, banco, filesystem, Android ou Playwright foi executado. Resultados de aceite dependem de implementação autorizada e execução real deste roteiro.