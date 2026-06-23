# CLAUDE.md — LayoutParserLib

Este arquivo configura o comportamento do Claude Code ao trabalhar neste repositório.
Inspirado no harness **AIOX** (`aiox-core/.claude`), porém **enxuto e focado no domínio**
desta biblioteca: **criptografia + logging compartilhados** do ecossistema LayoutParser.

> **Idioma:** responda ao usuário em **português (PT-BR)**. Documentação de produto é **bilíngue (PT/EN)**.

---

## 1. O que é este projeto

**Class library .NET Framework 4.8.1** (não-SDK, ToolsVersion 15.0) com **2 responsabilidades**:

| Arquivo | Papel |
|---------|-------|
| [`CryptographySysMiddle.cs`](../CryptographySysMiddle.cs) | Descriptografia Sysmiddle (Rijndael/AES, CBC). **Superfície pública** (`public static string Decrypt(string)`). |
| [`RollingFileLogger.cs`](../RollingFileLogger.cs) | Log em arquivo com rotação por tamanho. **`internal static`** (uso interno da lib). |

É o **componente compartilhado** do ecossistema de 4 repositórios (`C:\Users\elson.lopes\source\repos\`):

| Repo | Como consome este Lib |
|------|-----------------------|
| **LayoutParserApi** (.NET 10) | **Referência de DLL** — hub que parseia documentos e gera XSLT/TCL. |
| **LayoutParserDecrypt** (`.exe`) | **Cópia do código-fonte** (`LayoutParserLib\*.cs` embutidos no `.csproj`). |
| **LayoutParserLib** *(este)* | **Source of truth** da criptografia/log Sysmiddle. |
| **LayoutParserReact** | Front-end (não consome o Lib). |

> ⚠️ **As cópias já divergiram.** O `LayoutParserDecrypt` chama `RollingFileLogger.Configure(...)`,
> método que **não existe nesta versão** do logger. Tratar paridade como concern central — ver
> [`rules/library-contract.md`](rules/library-contract.md).

Como este Lib é referenciado por outros, **qualquer mudança na superfície pública é uma mudança de
contrato cross-repo.** Pense em Api e Decrypt antes de alterar assinaturas.

---

## 2. Sistema de Agentes (enxuto)

Ative com `@nome` ou via `Task` tool. Personas tailored ao domínio cripto/lib compartilhada
(prefixo **`lib-`** para distinguir dos `lp-*` do Api):

| Agente | Persona | Escopo principal |
|--------|---------|------------------|
| `@lib-architect` | **Caio** | Contrato público, compatibilidade, papel de lib compartilhada, trade-offs. **Não implementa.** |
| `@lib-crypto-dev` | **Bruno** | Implementação C#/.NET Framework 4.8.1: cripto e logger. |
| `@lib-security` | **Sofia** | Corretude criptográfica + segredos (chave/IV, algoritmo, modo, padding). Domínio crítico aqui. |
| `@lib-qa` | **Quésia** | Testes, **paridade entre as 3 cópias**, quality gates. |
| `@lib-devops` | **Gael** | `git push` (EXCLUSIVO), CI (`trigger-deploy.yml`), empacotar/versionar a DLL, MCP. |
| `@lib-doc` | **Dora** | Documentação bilíngue (README, XML docs). |

### Regra de autoridade (resumo)
- **Apenas `@lib-devops` faz `git push`** — e aqui o push **dispara o deploy do Api** (CI em cascata).
- `@lib-architect` **analisa e recomenda**, não escreve código de produção.
- Detalhe: [`rules/agent-authority.md`](rules/agent-authority.md).

### Handoff entre agentes
Ao trocar de agente, compacte o contexto anterior num artefato de handoff (~400 tokens):
tarefa atual, branch, decisões-chave, arquivos tocados, próximo passo.
Protocolo: [`rules/agent-handoff.md`](rules/agent-handoff.md).

---

## 3. Padrões de Código — resumo

Detalhe completo em [`rules/dotnet-standards.md`](rules/dotnet-standards.md).

- **.NET Framework 4.8.1, projeto não-SDK, C# 7.3** (default). **Não** use records, nullable reference
  types, `using` implícito, namespaces file-scoped nem top-level statements — não compilam aqui.
- **Build com MSBuild:** `msbuild LayoutParserLib.sln /p:Configuration=Release` (Developer Prompt do VS).
  Só referências de framework (`System.*`) — sem NuGet restore.
- **Superfície pública = contrato.** Não altere assinatura de `CryptographySysMiddle.Decrypt` sem
  avaliar Api e Decrypt. Ver [`rules/library-contract.md`](rules/library-contract.md).
- **Logger resiliente:** o `RollingFileLogger` engole exceções de propósito (`catch { }`) — logging
  **nunca** pode derrubar o chamador. Mantenha esse princípio.
- **Comentários em PT-BR**, no tom já presente no código.

---

## 4. Quality Gates (antes de concluir)

```bash
msbuild LayoutParserLib.sln /p:Configuration=Release   # deve compilar sem erros
```

- Não conclua tarefa com build quebrado.
- Mexeu na cripto ou no logger? Rode `/check-parity` e avalie o impacto em Api e Decrypt.
- Mudou a superfície pública? Atualize o README e avise para sincronizar os consumidores (`@lib-doc`).

---

## 5. Segurança (NON-NEGOTIABLE)

- O domínio **é** criptografia: trate chave, IV, modo e padding com rigor.
- **Pendência aberta:** [`CryptographySysMiddle.cs:17`](../CryptographySysMiddle.cs:17) tem **chave e IV
  hardcoded** em código-fonte (e os tamanhos não batem com Rijndael — investigar). Ver [`rules/security.md`](rules/security.md).
- **NUNCA** logue texto decriptografado (plaintext) nem segredos. O logger atual só registra **tamanhos** — mantenha assim.
- **NUNCA** comite chave/IV/credencial nova em texto plano. Ao detectar uma, pare e sinalize.

---

## 6. Git & Commits

- **Conventional Commits:** `feat:`, `fix:`, `docs:`, `refactor:`, `test:`, `chore:`.
- Trabalhe em branch (`feat/*`, `fix/*`); **não** comite direto na `master` sem pedido.
- **Push só por `@lib-devops`** e só quando o usuário pedir. Lembre: push aqui **aciona o deploy do Api**.

---

## 7. Otimização Claude Code

| Tarefa | Use | Não use |
|--------|-----|---------|
| Buscar conteúdo | `Grep` | `grep`/`rg` no bash |
| Ler arquivos | `Read` | `cat`/`head`/`tail` |
| Editar | `Edit` | `sed`/`awk` |
| Buscar arquivos | `Glob` | `find` |

- Chamadas independentes em **paralelo** num só turno.
- Comandos: `msbuild`/`git` rodam em PowerShell (shell primário) ou Bash.
- **better-context (btca):** ao mexer com APIs de framework (ex.: `System.Security.Cryptography`),
  prefira consultar o código-fonte real via `btca ask --resource <lib> --question "..."` a confiar em
  docs desatualizadas. (Ressalva: btca está em reescrita e pode ter atrito no Windows.)

---

## 8. MCP (futuro — "talvez")

Ainda **não há** MCP Server neste repo. Como o Lib é ponto de conexão do ecossistema, um MCP é uma
**possibilidade futura** (ex.: expor decrypt/inspeção de log como tools). Há um risco de segurança
sério — um decrypt exposto vira **oráculo de descriptografia**. Decisão e desenho ficam para depois,
sob revisão do `@lib-security` + `@lib-devops`. Ver [`rules/mcp-usage.md`](rules/mcp-usage.md).

---

*LayoutParserLib · Claude Code harness v1 · enxuto, focado em cripto/lib compartilhada*
