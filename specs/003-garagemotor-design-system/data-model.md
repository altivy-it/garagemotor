# Modelo de Dados: Design system e identidade GarageMotor

**Origem**: IA assistida a partir da spec e da análise do frontend existente.  
**Revisor humano**: CEO da Altivy e owner do GarageMotor.

## Tema selecionado

Preferência local de aparência da interface. Não é dado de domínio, perfil de usuário ou segredo.

| Campo | Tipo | Regra |
|---|---|---|
| `selected` | enum `light \| dark \| system` | Valor persistido; qualquer outro valor lido do armazenamento é inválido e deve ser descartado em favor de `system`. |
| `effective` | enum `light \| dark` | Valor derivado de `selected`; quando `system`, corresponde à preferência atual do sistema operacional. |
| `storageKey` | string constante versionada | `gm.theme.v1`; escopada pela origem/instalação, sem credenciais, tokens ou identificadores de usuário. |

### Regras e transições

- Inicialização: ler e validar `gm.theme.v1`; na ausência ou valor inválido, selecionar `system`.
- Resolução: `light` e `dark` são explícitos; `system` acompanha `prefers-color-scheme`. Se não houver suporte, usar `light` como tema efetivo sem alterar `selected`.
- Mudança explícita: atualizar seleção, tema efetivo e armazenamento no mesmo fluxo, sem recarregar ou reinicializar sessão/formulário.
- Mudança do dispositivo: atualizar apenas `effective` se `selected === system`; não gravar novo valor nem alterar `selected`.
- Persistência indisponível: manter a seleção durante a sessão, usar `system` como fallback e não interromper a aplicação.
- Escopo: preferência é independente da sessão de autenticação e isolada pela origem. Não há sincronização entre instâncias ou dispositivos.

## Ativo de identidade

Artefato estático local, sem registro de runtime ou envio ao backend.

| Atributo | Regra |
|---|---|
| `kind` | `wordmark`, `symbol`, `favicon`, `app-icon` ou `ui-icon`. |
| `themeVariant` | `light`, `dark` ou `adaptive`; cada variante deve manter contraste e legibilidade. |
| `source` | SVG original do GarageMotor, sob `frontend/web/src/assets/brand/`; ícones de interface podem ser paths agrupados no componente `gm-icon`. |
| `externalReferences` | Nenhuma; sem scripts, imagens, fontes ou URLs externas embutidas. |
| `accessibleName` | Obrigatório quando o ícone comunica ação/estado; decorativos são ocultos da árvore acessível. |
| `provenance` | Origem GarageMotor registrada; fonte Outfit e primitivas reutilizadas mantêm seus avisos/licenças separadamente. |

## Primitive visual

Componente Angular standalone reutilizável. Não persiste estado de negócio.

| Atributo | Regra |
|---|---|
| `semanticRole` | Ação, campo, seleção, checkbox, superfície, navegação, status, feedback ou estado vazio/carregamento. |
| `state` | Estados aplicáveis: normal, hover/pressed, foco, desabilitado, carregando, sucesso, aviso, erro ou vazio. |
| `theme` | Consome tokens semânticos via CSS; não define cores de marca fixas dentro do componente. |
| `accessibility` | Semântica HTML nativa, nome/descrição programáticos, teclado e foco visível; status não depende apenas da cor. |
| `platform` | Mesmo componente Angular/CSS no navegador e no WebView Android; layout deve responder a toque e viewport estreito. |

## Persistência e compatibilidade

Não há schema, migration, API ou contrato externo. Alteração futura da chave requer migração local compatível ou fallback seguro, sem apagar credenciais nem estado da garagem.
