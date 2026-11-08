# Plano de Implementação: Garagem pessoal e familiar

**Branch**: `001-garagem-pessoal` | **Data**: 2026-11-08 | **Spec**: [spec.md](spec.md)

**Entrada**: Especificação em `specs/001-garagem-pessoal/spec.md`.

**Status**: BLOQUEADO NA FASE 0; desenho da Fase 1, tarefas e implementação não autorizados por este artefato.

**Origem**: IA assistida. **Revisor humano**: CEO da Altivy e owner do GarageMotor.

**Feature selecionada**: `SPECIFY_FEATURE_DIRECTORY=specs/001-garagem-pessoal`.

## Resumo

Preparar o MVP pessoal/familiar para veículos, abastecimentos, despesas, manutenção, lembretes e comprovantes. Primeira entrega: web, self-hosted e Android. iOS, OCR, chat, predição, pequena frota empresarial e edição proprietária ficam fora.

O fluxo oficial preparou o plano e iniciou pesquisa delegada e consultas públicas. .NET 10 LTS e seus SDK/runtime foram confirmados; versões/compatibilidade/licenças frontend, driver de banco e versão PostgreSQL permanecem sem verificação suficiente. O fluxo exige resolver esses pontos antes da Fase 1; portanto, este documento registra preparação, não plano completo ou autorização de instalação.

Os achados, fontes e bloqueios estão em [research.md](research.md).

## Contexto Tecnico

**Linguagem/Versão**: C#/.NET 10 como candidato LTS; metadata oficial confirma SDK 10.0.403, runtime 10.0.14 e suporte até 2028-11-14. Angular 22 obrigatório. NEEDS CLARIFICATION F0-1: fixar versões frontend, TypeScript/Node e compatibilidade com Capacitor após verificação.

**Dependências Principais**: Angular + Capacitor, sem obrigatoriedade de componentes Ionic, por decisão humana de 2026-11-08. NEEDS CLARIFICATION F0-2: verificar licenças da versão exata, proveniência do DS e dependências de acesso ao banco/testes. Nenhuma adoção automática de dependências do Dokpod.

**Persistência**: PostgreSQL e arquivos privados são candidatos. NEEDS CLARIFICATION F0-3: fixar versão estável suportada do banco e driver, justificar datastore por ADR e decidir armazenamento/particionamento antes do modelo e contratos.

**Testes**: devem cobrir unidade, integração real de banco/filesystem, concorrência, contratos, Playwright web e aceite Android. Bibliotecas e imagens de runner dependem da verificação F0-1/F0-2; nenhum teste da aplicação executado nesta preparação.

**Plataforma Alvo**: serviços Linux em containers; HTTP de aplicação, navegador e runner somente por nginx em container. Android conectado; build/testes em containers. iOS excluído desta feature, sem exceção silenciosa de macOS.

**Tipo de Projeto**: aplicação web/API e cliente Android com código frontend compartilhado; organização modular enxuta candidata, sem microserviços, broker ou cache distribuído.

**Metas de Desempenho**: SC-005 da spec: P95 até dois segundos, 10 usuários simultâneos e 10.000 registros em matriz a definir. Sem promessa de resultado já obtido.

**Restrições**: código próprio AGPL-3.0-only; dependências continuam permissivas, gratuitas e verificadas; Angular 22/DS Altivy obrigatórios; demais versões LTS/estáveis mantidas e fixadas. Dokpod é referência, não biblioteca estável aprovada. Sem IA, push obrigatório, escrita offline ou edição proprietária.

**Escala/Escopo**: garagem pessoal/familiar; carros e motos; papéis proprietário/editor/leitor e isolamento entre garagens. Capacidade operacional, retenção e matriz precisam ser fechadas antes de tarefas executáveis.

## Verificação da Constituição

*GATE: Deve ser aprovado antes da pesquisa da Fase 0 e revisado novamente após o design da Fase 1.*

Verificação inicial contra a [constituição 1.0.0](../../.specify/memory/constitution.md):

| Princípio | Situação | Tratamento |
|---|---|---|
| I. AGPL e reciprocidade | Conforme na preparação | Decisão humana registrada; arquivo oficial, avisos e inventário ainda precisam de trabalho autorizado, sem publicar dados/secrets. |
| II. Self-hosted | Conforme no escopo | Núcleo sem assinatura/chave; três modalidades de serviço não aprovam preços ou edição proprietária. |
| III. Tecnologia/DS | BLOCKED para adoção | Angular 22 e DS confirmados; versões, licenças e maturidade precisam ser verificadas antes de qualquer instalação ou reutilização. |
| IV. Segurança/integridade | Conforme como requisitos, não validado | Spec cobre autorização, concorrência, evidências e recuperação; threat model e testes reais pertencem ao desenho/implementação autorizados. |
| V. Spec Kit/qualidade | Conforme | Somente artefatos; autorização de implementação ainda necessária. Não avançar Fase 1 com esclarecimentos técnicos pendentes. |

**Gate após pesquisa**: ERROR F0-1/F0-2/F0-3 não resolvidos. Não há justificativa que dispense verificação de licença ou permita usar versões presumidas.

**Gate após desenho**: não executado, porque a Fase 1 não iniciou. Modelo de dados, contratos, quickstart, ADRs e tarefas não foram produzidos nesta rodada.

## Estrutura do Projeto

### Documentação desta Funcionalidade

```text
specs/001-garagem-pessoal/
  spec.md
  plan.md
  research.md
  checklists/requirements.md
```

### Código-Fonte (raiz do repositório)
Nenhum código de aplicação foi criado. Diretórios candidatos para o desenho posterior: `backend/`, `frontend/web/`, `frontend/tests/`, `contracts/` e `deploy/`. Essa organização não aprova datastore, biblioteca, identidade ou deployment sem os gates e decisões correspondentes.

**Decisão de Estrutura**: adiada até conclusão da pesquisa; não copiar a estrutura de agente, containers gerenciados, Business ou protocolos do Dokpod, que pertencem a outro domínio.

## Registro de Complexidade

Nenhuma exceção constitucional foi aprovada. As indefinições são bloqueios, não autorização para aumentar a complexidade ou enfraquecer a regra de dependências.

## Retomada do planejamento

Retomar `/speckit-plan` com esta feature explicitamente selecionada quando houver evidência verificável dos candidatos. Não executar novamente o hook de criação de branch. Concluir pesquisa, então modelo de dados, contratos e quickstart, depois `/speckit-tasks` e `/speckit-analyze`.

Preços, aquisição, SLA e publicação continuam pendentes de decisões próprias. Licença oficial e reconciliação operacional devem entrar nas tarefas do desenho completo; a criação de repositório remoto não autorizou push.
