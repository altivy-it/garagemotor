# Especificação da Funcionalidade: Garagem pessoal e familiar

**Branch da Funcionalidade**: `001-garagem-pessoal`

**Criada em**: 2026-11-08

**Status**: Rascunho para revisão humana; implementação não autorizada

**Entrada**: "Pessoas e famílias com veículos, abastecimentos, despesas, manutenção, lembretes e comprovantes. Web, self-hosted e Android primeiro; iOS após resolver a cadeia de build."

**Origem**: IA assistida, a pedido humano. **Revisor humano**: CEO da Altivy e owner do GarageMotor.

**Base**: [handoff da descoberta](../../.specify/assessments/gestao-veicular-inteligente/decision.md) e [constituição](../../.specify/memory/constitution.md).

## Clarifications

### Session 2026-11-09

- Q: Quais criações devem impedir duplicação quando o usuário repete a mesma operação? → A: Todas as criações de recursos do domínio: garagens, veículos, convites, despesas, abastecimentos, manutenção, planos, compromissos, comprovantes e exportações.
- Q: Qual política de validade e tentativas deve proteger os códigos de recuperação de conta? → A: Códigos aleatórios com ao menos 128 bits, uso único; SMTP válido por 15 minutos; dez códigos offline por lote, válidos por 12 meses; até cinco falhas por conta em 15 minutos e limite adicional por origem. Renovação invalida o lote anterior; recuperação concluída revoga sessões e os demais códigos, sem bloqueio permanente por falhas.
- Q: Como devem ser contados os prazos para excluir a garagem e impedir sua volta por um backup antigo? → A: Dados ativos, arquivos e backups gerenciados contendo a garagem são eliminados até 30 dias após o pedido. Restauração verifica e reaplica pedidos posteriores ao backup; fica bloqueada se não conseguir verificá-los. O operador responde pelas cópias sob sua gestão, sem garantia do app para cópias particulares externas a esse controle.
- Q: Qual cenário deve comprovar o tempo de resposta do histórico e dos totais? → A: Dez usuários simultâneos, dois minutos de aquecimento e dez de medição; 80% histórico e 20% totais. Conjunto fictício determinístico de 10.000 despesas em 20 garagens, três veículos por garagem e 12 meses; matriz de 4 vCPU, 8 GiB RAM e SSD; P95 de cada tipo em até dois segundos, sem erros inesperados, medido pelo proxy.

## Cenários de Usuário e Testes *(obrigatório)*

### História de Usuário 1 - Organizar uma garagem familiar (Prioridade: P1)

Uma pessoa cria sua garagem, cadastra um veículo e compartilha acesso com familiares sem expor dados a outras famílias.

**Motivo da prioridade**: estabelece identidade, veículo e fronteira de acesso das demais jornadas.

**Teste independente**: duas famílias separadas cadastram veículos; convite, papéis e revogação são verificados sem despesas prévias.

**Cenários de aceitação**:

1. **Dado** um usuário autenticado sem garagem, **Quando** cria sua garagem e cadastra um carro ou moto, **Então** torna-se proprietário e consulta apenas seus veículos.
2. **Dado** um convite válido aceito, **Quando** o familiar acessa a garagem, **Então** consulta ou altera dados conforme papel; revogação impede novos acessos imediatamente.
3. **Dado** um usuário de outra família, **Quando** tenta acessar um identificador conhecido, **Então** não recebe dados nem confirmação da existência do recurso.
4. **Dado** um envio de criação cuja resposta não chegou, **Quando** o usuário autorizado repete a mesma operação e conteúdo, **Então** recebe o resultado anterior sem criar outra garagem, veículo ou convite; conteúdo diferente para a mesma operação é recusado como conflito.
5. **Dado** um código de recuperação usado, expirado, substituído ou uma conta com cinco falhas nos últimos 15 minutos, **Quando** ocorre nova tentativa, **Então** a recuperação é recusada genericamente sem revelar a existência da conta; códigos offline válidos permitem recuperar acesso sem SMTP ou SaaS.

### História de Usuário 2 - Conhecer despesas e custos (Prioridade: P1)

O usuário registra despesas e consulta totais por veículo, categoria e período, com origem identificável.

**Motivo da prioridade**: entrega controle financeiro mesmo sem histórico de combustível.

**Teste independente**: cadastrar um veículo e três despesas conhecidas, conferir totais e editar/anular uma delas.

**Cenários de aceitação**:

1. **Dado** despesas válidas em categorias distintas, **Quando** seleciona período e veículo, **Então** os totais correspondem exatamente aos registros incluídos.
2. **Dado** uma despesa corrigida ou anulada, **Quando** consulta novamente, **Então** o histórico identifica a mudança e o total não duplica o valor anterior.
3. **Dado** distância insuficiente no período, **Quando** consulta custo por quilômetro, **Então** vê indisponibilidade ou cobertura parcial explícita, nunca um número apresentado como completo.

### História de Usuário 3 - Acompanhar abastecimento e consumo (Prioridade: P1)

O usuário registra abastecimentos e acompanha volume, gasto e consumo quando existe uma sequência válida de medição.

**Motivo da prioridade**: acompanha uma despesa recorrente e evita indicadores enganosos.

**Teste independente**: usar um veículo com abastecimentos completos e parciais, incluindo correção de odômetro.

**Cenários de aceitação**:

1. **Dado** tanque cheio em 100.000 km e novo tanque cheio em 100.550 km com 50 litros, **Quando** consulta o intervalo, **Então** o consumo é 11 km/l; o volume relevante é o que recompõe o tanque no fim do intervalo.
2. **Dado** abastecimentos parciais entre dois completos, **Quando** consulta consumo, **Então** usa o volume acumulado correspondente ao intervalo; sem fechamento válido, o consumo fica indisponível.
3. **Dado** repetição do envio ou odômetro regressivo, **Quando** registra, **Então** não duplica a operação e exige correção ou justificativa explícita para uma leitura histórica fora de sequência.

### História de Usuário 4 - Planejar manutenção e compromissos (Prioridade: P2)

O usuário acompanha manutenção preventiva/corretiva e compromissos de óleo, revisão, seguro e licenciamento por data ou quilometragem.

**Motivo da prioridade**: torna o histórico útil para obrigações futuras, sem previsão mecânica automática.

**Teste independente**: criar um plano por data e outro por quilometragem e registrar sua conclusão.

**Cenários de aceitação**:

1. **Dado** compromisso com limite por data ou uso atingido, **Quando** abre os lembretes, **Então** vê vencimento e critério acionado com texto, não somente cor.
2. **Dado** manutenção concluída com custo, **Quando** registra uma despesa associada, **Então** o custo entra uma única vez no total e o próximo ciclo decorre da regra registrada.
3. **Dado** uma garagem sem novos odômetros, **Quando** consulta um plano por uso, **Então** vê a data da última leitura, sem afirmar quilometragem atual desconhecida.

### História de Usuário 5 - Associar e recuperar comprovantes (Prioridade: P2)

O usuário associa comprovantes a despesas ou manutenção e os recupera somente com acesso autorizado.

**Motivo da prioridade**: permite conferência e preservação do histórico, sem criar um cofre documental genérico.

**Teste independente**: enviar PDF, JPG e PNG válidos, baixar o mesmo conteúdo e recusar arquivos inválidos ou de outra família.

**Cenários de aceitação**:

1. **Dado** um comprovante permitido de até 10 MiB, **Quando** envia e consulta o registro, **Então** o arquivo é associado e pode ser recuperado com nome e integridade preservados.
2. **Dado** arquivo incompatível, maior que o limite ou acesso não autorizado, **Quando** tenta enviar ou baixar, **Então** a operação é recusada sem exposição de caminhos internos ou conteúdo.
3. **Dado** uma operação repetida ou interrompida, **Quando** retoma, **Então** não aparecem vínculos duplicados ou comprovantes parcialmente disponíveis.

### História de Usuário 6 - Usar o produto com seus próprios dados (Prioridade: P1)

Uma pessoa usa a garagem no navegador e Android, conectada ao serviço gerenciado ou à instância própria, e consegue exportar e recuperar seus dados.

**Motivo da prioridade**: autonomia e portabilidade são necessidades explicitamente declaradas pelo owner.

**Teste independente**: instalar uma instância isolada, executar a jornada básica no navegador e Android, exportar dados e restaurar cópia em ambiente limpo.

**Cenários de aceitação**:

1. **Dado** uma instância própria acessível com conexão confiável, **Quando** o usuário seleciona essa instância no Android, **Então** autentica e executa o mesmo núcleo sem conta ou chave de licença SaaS.
2. **Dado** indisponibilidade de rede, **Quando** tenta gravar, **Então** a interface informa falha e não declara sucesso nem gera duplicação ao repetir após reconectar.
3. **Dado** uma cópia válida do histórico e dos comprovantes e o estado verificado dos pedidos de exclusão posteriores à cópia, **Quando** restaura em instância limpa, **Então** recupera relações, valores e arquivos ainda autorizados, reaplica as exclusões e não inclui secrets na exportação; sem conseguir verificar esses pedidos, a restauração fica bloqueada antes de disponibilizar dados.

### Casos de Borda

- Convite expirado, reutilizado, revogado ou aceito por identidade diferente; último proprietário não pode sair sem transferência.
- Garagem sem veículos, veículo arquivado com histórico, ausência de gastos no período e página de resultados vazia.
- Combustíveis distintos no mesmo veículo, abastecimento parcial, ausência de tanque cheio, distância zero e odômetro histórico corrigido.
- Pagamento ou manutenção repetidos; edições concorrentes não podem sobrescrever mudanças silenciosamente.
- Vencimento na troca de mês, fuso da garagem e regra que vence por data ou uso, o que ocorrer primeiro.
- Comprovante removido, conteúdo incompatível, caminho malicioso, limite excedido, escrita interrompida e recusa de execução inline.
- Endpoint próprio inválido, certificado não confiável, sessão expirada, família revogada e versão de servidor incompatível.
- Restauração incompleta ou corrompida deve falhar sem substituir silenciosamente os dados existentes.
- Backup criado durante processamento de exclusão mantém a mesma data-limite de 30 dias após o pedido; ausência do estado verificável de pedidos posteriores à cópia bloqueia restauração, sem recuperar garagem excluída.

## Requisitos *(obrigatório)*

### Requisitos Funcionais

- **FR-001**: Permitir cadastro, autenticação, encerramento de sessão e recuperação de acesso, sem expor credenciais ou existência de contas a terceiros. Códigos de recuperação têm ao menos 128 bits de aleatoriedade e uso único: SMTP expira em 15 minutos; lote de dez códigos offline expira em 12 meses e é entregue uma única vez após reautenticação. Até cinco tentativas inválidas por conta são permitidas em janela móvel de 15 minutos, com limitação adicional por origem e sem bloqueio permanente da conta. Renovar um lote invalida os códigos offline anteriores; concluir recuperação revoga sessões e todos os demais códigos de recuperação.
- **FR-002**: Permitir criar uma garagem familiar e convidar, revogar e transferir propriedade; convites são de uso único e expiram em 72 horas.
- **FR-003**: Aplicar papéis proprietário, editor e leitor em toda consulta/operação; somente proprietário gerencia membros e exclui a garagem; editor registra e corrige; leitor apenas consulta.
- **FR-004**: Isolar dados e comprovantes entre garagens, inclusive consultas diretas, totais, arquivos e exportações; uma identidade pode participar de mais de uma garagem sem mistura de dados.
- **FR-005**: Cadastrar e editar carro ou moto com identificação, marca, modelo, ano e combustível; placa, chassi e Renavam são opcionais. Arquivar veículo preservando histórico, sem apagar despesas.
- **FR-006**: Registrar despesa com veículo, data, categoria, descrição e valor positivo em BRL; categorias iniciais são combustível, manutenção, seguro, impostos e outros.
- **FR-007**: Permitir correção e anulação por usuário autorizado, preservando autoria e mudança; anulação exclui o valor dos totais, sem apagar a evidência histórica.
- **FR-008**: Apresentar gasto mensal/anual e totais por categoria/veículo com filtros e estados vazios, sem dupla contagem de combustível ou manutenção associados a despesas.
- **FR-009**: Calcular custo/km somente com distância e despesas do intervalo coberto; explicitar cobertura parcial e impedir divisão por zero.
- **FR-010**: Registrar abastecimento com data, volume positivo em litros, valor positivo, odômetro, combustível, tanque cheio e posto opcional.
- **FR-011**: Calcular consumo entre dois registros de tanque cheio com volume que recompõe o tanque, incluindo parciais intermediários; distinguir combustível e excluir intervalos inválidos ou incompletos.
- **FR-012**: Tratar odômetro regressivo/histórico com validação e justificativa, recalculando os intervalos afetados sem alterar dados de outra garagem.
- **FR-013**: Impedir duplicação em repetição da mesma criação de garagem, veículo, convite, despesa, abastecimento, manutenção, plano, compromisso, comprovante ou exportação. Mesma operação, ator, contexto e conteúdo retorna o resultado anterior após revalidar acesso; conteúdo diferente para a mesma operação e edição com versão divergente são recusados como conflito, com informação recuperável ao usuário. Cadastro de conta e abertura de sessão têm regras próprias e não pertencem a essa lista de recursos do domínio.
- **FR-014**: Registrar manutenção preventiva/corretiva, conclusão, data, odômetro e despesa vinculada sem contabilização duplicada.
- **FR-015**: Definir planos e compromissos por data, quilometragem ou ambos; vence pelo primeiro limite atingido e recorrência parte da conclusão registrada.
- **FR-016**: Exibir lembretes próximos, vencidos e concluídos, com critério e última leitura usados; janela inicial de proximidade é 30 dias ou 500 km.
- **FR-017**: Manter lembretes acessíveis dentro do produto sem exigir push ou infraestrutura de notificações externa; entrega push não faz parte desta feature.
- **FR-018**: Associar até cinco comprovantes PDF, JPG ou PNG por despesa/manutenção, com no máximo 10 MiB cada, sem processamento OCR.
- **FR-019**: Validar conteúdo, tipo, tamanho e destino autorizado dos arquivos; impedir navegação de diretório e execução inline; nunca expor caminho real de armazenamento.
- **FR-020**: Exigir autorização em cada recuperação de arquivo e preservar integridade/consistência entre vínculo e conteúdo, inclusive após falhas ou exclusão.
- **FR-021**: Oferecer o núcleo US1–US5 no navegador e Android, com conexão necessária para gravação; seleção de instância própria deve separar sessão e dados de instâncias anteriores.
- **FR-022**: Validar conexão e versão compatível antes do uso; não aceitar silenciosamente certificado inválido ou servidor incompatível nem declarar sucesso quando a gravação falhar.
- **FR-023**: Disponibilizar exportação estruturada de dados da garagem ao proprietário, sem credenciais; oferecer procedimento de backup/restauração que inclua comprovantes e verifique integridade. Antes de disponibilizar dados restaurados, verificar e reaplicar pedidos de exclusão posteriores ao backup; sem estado verificável desses pedidos, bloquear a restauração, sem depender obrigatoriamente de SaaS para essa verificação.
- **FR-024**: Permitir solicitar exclusão da garagem após confirmação e exportação opcional. Dados ativos, comprovantes e backups gerenciados que contenham a garagem devem ser eliminados até 30 dias após o pedido, inclusive cópias criadas durante seu processamento. O operador SaaS ou self-hosted responde pelas cópias sob sua gestão; cópias particulares fora desse controle não têm eliminação garantida pelo app, e essa limitação deve ser informada ao proprietário. Término de suporte não bloqueia direitos ou acesso aos próprios dados.
- **FR-025**: Aplicar pt-BR, BRL, quilômetros, litros e fuso da garagem, inicialmente America/Sao_Paulo, com apresentação legível e consistente.
- **FR-026**: Seguir a identidade visual Altivy com referência Dokpod, teclado/foco visível, estados com texto/ícone, redução de movimento e acessibilidade AA nas jornadas principais.
- **FR-027**: Permitir instalação própria e uso do núcleo sem chave comercial, telemetria obrigatória ou dependência de conta SaaS; serviços opcionais não podem bloquear registros/consultas.
- **FR-028**: Distribuir código próprio sob AGPL-3.0-only com avisos, licença e acesso ao código correspondente nos termos aplicáveis; preservar direitos/avisos de terceiros e separar dados/secrets do material publicado.
- **FR-029**: Publicar matriz de capacidade testada e procedimento de instalação, atualização e recuperação; os limites testados não constituem restrição legal de veículos.
- **FR-030**: Registrar eventos de segurança e alterações relevantes sem conteúdo de comprovantes, credenciais ou dados pessoais desnecessários, com acesso restrito e retenção documentada.

### Entidades Principais *(se a funcionalidade envolver dados)*

- **Usuário**: identidade autenticada, participação em garagens e códigos de recuperação com modalidade, validade, uso e revogação; credenciais e códigos não entram em exportações. Códigos SMTP e offline seguem os limites de FR-001; valores secretos não são mostrados novamente após entrega.
- **Garagem/Membro/Convite**: fronteira familiar, papel, fuso e transições de acesso.
- **Veículo**: identificação e estado ativo/arquivado, ligado a uma garagem.
- **Despesa**: valor, categoria, período, origem e histórico de correção/anulação.
- **Abastecimento**: medição e condição de tanque, ligado a uma única despesa de combustível.
- **Manutenção/Plano/Compromisso**: execução, recorrência e vencimento com despesa opcional única.
- **Comprovante**: evidência privada com tipo, tamanho, integridade e vínculo autorizado.
- **Evento de auditoria**: ação relevante e autoria mínima, separado do conteúdo sensível.

## Critérios de Sucesso *(obrigatório)*

### Resultados Mensuráveis

- **SC-001**: Um usuário preparado conclui criação da garagem, primeiro veículo e primeira despesa em até cinco minutos no roteiro de aceite; tempo real deve ser medido, não presumido.
- **SC-002**: Totais e consumo coincidem com 100% dos resultados esperados dos casos completos, parciais, corrigidos e anulados; intervalos inválidos não apresentam indicador completo.
- **SC-003**: Nenhum acesso indevido entre duas garagens é permitido nos testes de registros, relatórios, comprovantes, membros e exportações.
- **SC-004**: Repetição das criações abrangidas por FR-013 não duplica recursos ou valores; conflitos de conteúdo/versão não sobrescrevem mudanças silenciosamente, e acesso revogado impede a recuperação do resultado anterior em todos os cenários de concorrência definidos.
- **SC-005**: Dez usuários simultâneos consultam dados autorizados pelo proxy, com dois minutos de aquecimento excluídos da medição e dez minutos de medição; 80% das consultas são de histórico e 20% de totais. Usar conjunto fictício determinístico de 10.000 despesas, distribuídas por 20 garagens com três veículos cada e por 12 meses, na matriz de 4 vCPU, 8 GiB RAM e SSD. O percentil 95 de cada tipo de consulta deve ser de até dois segundos, sem erros inesperados; registrar separadamente resultados, conjunto e parâmetros para repetição.
- **SC-006**: Backup/restauração recupera 100% dos valores, relações e arquivos ainda autorizados do conjunto de teste, reaplicando os pedidos de exclusão posteriores ao backup sem reintroduzir garagens excluídas. Cópia inválida ou estado de exclusões não verificável é recusado antes de substituir ou disponibilizar dados; todos os backups gerenciados contendo a garagem respeitam a data-limite de 30 dias após seu pedido.
- **SC-007**: As seis jornadas de aceite passam no navegador e Android na matriz inicial; teclado e auditoria de acessibilidade não apontam falhas AA bloqueantes nas jornadas web.
- **SC-008**: Uma instalação própria executa o núcleo sem licença online, SaaS, push ou IA; documentação e pacote de fonte incluem instruções reproduzíveis e avisos exigidos.

## Premissas

- O primeiro público é pessoal/familiar; frotas empresariais, governo e máquinas agrícolas ficam fora do MVP.
- Web e Android entram nesta entrega; iOS fica fora até resolução explícita da cadeia de build. Sem promessa de lançamento nas lojas nesta preparação.
- O primeiro uso é conectado; não há fila offline de escrita, sincronização offline ou push nesta feature. Falhas de rede devem preservar a possibilidade de repetir sem duplicação.
- Convites são aceitos por conta correspondente; autenticação e recuperação devem funcionar conforme configuração da instalação, sem serviço SaaS obrigatório.
- Registros anulados permanecem até exclusão da garagem. Dados ativos, arquivos e backups gerenciados contendo a garagem são eliminados até 30 dias após o pedido, sem reiniciar o prazo pela criação de nova cópia. Restauração exige verificar/reaplicar pedidos posteriores ao backup; o operador deve informar o alcance e as limitações de sua gestão de cópias.
- OCR, chat, predição, cofre documental genérico, cobrança automática, edição proprietária e preços anteriores não pertencem à feature.
- Stack e integração mobile seguem constituição e plano. Em esclarecimento de 2026-11-08, o owner escolheu código compartilhado com componentes próprios, sem obrigatoriedade de componentes Ionic e sem substituir a identidade Altivy/Dokpod.
- Os critérios acima são critérios de aceite propostos, não evidência de implementação, desempenho ou adesão já obtida.