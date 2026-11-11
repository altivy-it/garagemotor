# Toolchain Android

**Feature**: [001-garagem-pessoal](../specs/001-garagem-pessoal/spec.md)  
**Verificado em**: 2026-11-09  
**Decisões humanas atualizadas em**: 2026-11-11, nesta conversa, conforme [esclarecimentos da spec](../specs/001-garagem-pessoal/spec.md).  
**Origem**: IA assistida. **Revisor humano**: CEO da Altivy e owner do GarageMotor.

## Escopo e estado

Esta toolchain serve exclusivamente a build e testes Android dentro de containers, conforme a exceção aprovada na [constituição](../.specify/memory/constitution.md). Não pode ser executada no host, distribuída com o app, usada para iOS ou tomada como autorização para publicar.

As versões e metadados abaixo foram consultados nas fontes oficiais. Os arquivos não foram baixados nem tiveram seus hashes recalculados localmente. Os checksums são os valores publicados pelas origens. As dependências transitivas do AGP ainda precisam da auditoria prevista em T005 antes de qualquer resolução/uso.

| Componente | Versão/artefato fixado | Origem e integridade publicada | Licença/termos | Estado |
|---|---|---|---|---|
| Eclipse Temurin JDK | 21.0.13+9, Linux x64; `OpenJDK21U-jdk_x64_linux_hotspot_21.0.13_9.tar.gz` | [Release oficial](https://github.com/adoptium/temurin21-binaries/releases/tag/jdk-21.0.13%2B9); [arquivo](https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.13%2B9/OpenJDK21U-jdk_x64_linux_hotspot_21.0.13_9.tar.gz); SHA-256 `d7f5d5cf865a2902b70ff549b97586a7d3f234e016f041ab447ea740c7d66d1c`, [checksum oficial](https://github.com/adoptium/temurin21-binaries/releases/download/jdk-21.0.13%2B9/OpenJDK21U-jdk_x64_linux_hotspot_21.0.13_9.tar.gz.sha256.txt) | GPL-2.0-only com Classpath Exception; coberto pela exceção humana restrita a ferramentas Android | Não baixado nem instalado |
| Gradle | 8.14.3, `gradle-8.14.3-bin.zip` | [Distribuição oficial](https://downloads.gradle.org/distributions/gradle-8.14.3-bin.zip); SHA-256 `bd71102213493060956ec229d946beee57158dbd89d0e62b91bca0fa2c5f3531`, [checksum oficial](https://downloads.gradle.org/distributions/gradle-8.14.3-bin.zip.sha256) | Apache-2.0 | Não baixado nem instalado |
| Android Gradle Plugin | 8.13.0, `com.android.tools.build:gradle:8.13.0` | [JAR oficial](https://dl.google.com/dl/android/maven2/com/android/tools/build/gradle/8.13.0/gradle-8.13.0.jar); SHA-256 `d0195874497b4ac3eaeb3a8525bf794d3a5b63a9a01084ca9ac330fb63060d01`, [checksum oficial](https://dl.google.com/dl/android/maven2/com/android/tools/build/gradle/8.13.0/gradle-8.13.0.jar.sha256) | Apache-2.0, declarada no [POM oficial](https://dl.google.com/dl/android/maven2/com/android/tools/build/gradle/8.13.0/gradle-8.13.0.pom) | Artefato e transitivas não resolvidos; auditoria em T005 |
| Android SDK Platform | API 36, pacote estável `platforms;android-36`, revisão 2 (`platform-36_r02.zip`) | [Catálogo oficial](https://dl.google.com/android/repository/repository2-1.xml); SHA-1 publicado `2c1a80dd4d9f7d0e6dd336ec603d9b5c55a6f576` | `android-sdk-license`; aceite humano declarado em 2026-11-11 | Não baixado nem instalado |
| Android SDK Build-Tools | 36.0.0, Linux (`build-tools_r36_linux.zip`) | [Catálogo oficial](https://dl.google.com/android/repository/repository2-1.xml); SHA-1 publicado `b0b6376977657e8ad9b969bacf4093601da2c6fb` | `android-sdk-license`; aceite humano declarado em 2026-11-11 | Não baixado nem instalado |

O catálogo oficial do SDK publica SHA-1 para Platform API 36 e Build-Tools 36.0.0; não foi encontrado SHA-256 desses dois arquivos no metadado consultado. Os valores acima são checksums anunciados pelo fornecedor, não verificação local de artefatos.

## Aceite e uso

O identificador `android-sdk-license` corresponde ao contrato de licença do Android SDK incorporado ao catálogo oficial. O catálogo informa que quem aceita em nome de uma organização precisa ter autoridade para vinculá-la. Em 2026-11-11, o CEO da Altivy e owner do GarageMotor respondeu "Esta aceito" à pendência dos termos do SDK apresentada nesta conversa. Esse é o registro de aceite humano para os artefatos fixados acima; não é aceite automatizado pelo agente nem cobre novos contratos ou termos alterados. O gate de aceite desses termos foi atendido; integridade, auditoria, versões mantidas e compatibilidade continuam exigências antes do uso.

Nenhuma ferramenta foi baixada ou executada nesta atualização documental. Uso posterior exige tarefa de implementação autorizada, container Linux, versão e origem fixadas, verificação do checksum do artefato obtido contra o valor publicado e conclusão da auditoria de dependências/avisos de T005. Nenhuma validação de build, teste, runtime ou compatibilidade Android foi realizada. O aceite não comprova instalação, compatibilidade ou prontidão do app.

## Identidade, Compatibilidade e Assinatura

Por decisão humana de 2026-11-11, o mínimo suportado é Android 15/API35 (`minSdk 35`); `compileSdk`/`targetSdk` continuam API36 e a matriz inicial API35/36. O identificador permanente recomendado e aceito é `it.altivy.garagemotor`, tanto para `applicationId` quanto para o `appId` Capacitor. Essas configurações ainda devem ser implementadas e conferidas no pacote gerado.

A chave de assinatura e suas senhas ficam sob guarda persistente do vault corporativo, com acesso seguro pelo GitHub para jobs autorizados. Não armazenar o material em arquivos versionados, logs, caches ou artefatos; não solicitar conteúdo de chave, senha ou token pela conversa. O owner indicou "Azure Vault ou 1password via op"; as opções são Azure Key Vault e 1Password via `op`, sem seleção definitiva nem uso simultâneo implícito. Não criar réplica persistente da chave no GitHub por padrão; a diretriz de guarda não significa integração já configurada ou autorização para deploy/publicação. A chave que assina o app não é a mesma coisa que as chaves do Keystore do dispositivo destinadas à sessão do usuário.

## Opções de Acesso Pelo GitHub Actions

| Alternativa informada pelo owner | Mecanismo documentado | Estado |
|---|---|---|
| Azure Key Vault | Identidade Entra com confiança federada restrita ao repositório/environment; GitHub OIDC e acesso temporário com privilégio mínimo | Recomendação técnica para CI; sem recursos, RBAC ou federação configurados |
| 1Password via `op` | No caminho documentado de service account, autenticação com `OP_SERVICE_ACCOUNT_TOKEN`; o token é credencial de bootstrap, não a chave de assinatura | Alternativa informada; não instalada, sem token criado e sem aprovação de licença/versão da CLI como dependência |

Em ambas, o job de assinatura deve usar environment protegido e revisão humana, negar acesso a PRs/forks não confiáveis, obter somente o material necessário e descartá-lo ao término, inclusive em falha. Nunca apresentar credenciais na conversa ou logs. O backend definitivo, as referências não secretas dos itens e as políticas precisam ser confirmados antes da implementação. Nenhum serviço pago ou ferramenta restritiva é incorporado ao app ou imposto ao operador self-hosted por essa indicação corporativa; qualquer adoção de ferramenta continua sujeita à política de dependências.

Referências consultadas em 2026-11-11: [GitHub Actions com Azure OIDC](https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure-openid-connect) e [service accounts com 1Password CLI](https://www.1password.dev/service-accounts/use-with-1password-cli/). A consulta foi documental; nenhum recurso Azure ou cofre foi enumerado ou acessado.
