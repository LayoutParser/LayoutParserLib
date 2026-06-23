# LayoutParserLib

**PT-BR:** Biblioteca .NET Framework 4.8.1 com a **criptografia (Sysmiddle)** e o **logger em arquivo**
compartilhados do ecossistema **LayoutParser**. É consumida pela API (referência de DLL) e pelo
utilitário de descriptografia.

**EN:** .NET Framework 4.8.1 class library providing the shared **Sysmiddle cryptography** and
**rolling file logger** for the **LayoutParser** ecosystem. Consumed by the API (DLL reference) and the
decrypt utility.

---

## Ecossistema / Ecosystem

| Repo | Papel · Role | Consome este Lib? |
|------|--------------|-------------------|
| **LayoutParserApi** (.NET 10) | Hub: parse de documentos + IA→XSLT/TCL | **Sim** — referência de DLL |
| **LayoutParserLib** *(este)* | Cripto + logging compartilhados | — (source of truth) |
| **LayoutParserDecrypt** (`.exe`) | Console de descriptografia | **Sim** — cópia de fonte (`<Compile Include="LayoutParserLib\*.cs">`) |
| **LayoutParserReact** | Front-end (Vite + React) | Não |

> ⚠️ **Paridade / Parity:** o `LayoutParserDecrypt` **copia** estes `.cs` em vez de referenciar a DLL.
> As cópias **já divergiram** (o Decrypt chama `RollingFileLogger.Configure(...)`, que não existe nesta
> versão). Veja [`.claude/rules/library-contract.md`](.claude/rules/library-contract.md) e rode `/check-parity`.

---

## Superfície pública / Public API

```csharp
// Descriptografa uma mensagem Sysmiddle (Base64 → texto). Rijndael/AES, modo CBC.
public static string LayoutParserLib.CryptographySysMiddle.Decrypt(string messageToDecrypt);
```

`RollingFileLogger` é `internal` (uso interno da lib).

### Logger — configuração por variáveis de ambiente / Logger env vars

| Variável | Efeito | Default |
|----------|--------|---------|
| `LAYOUTPARSER_LOG_DIR` | Diretório dos logs | `<BaseDirectory>\logs` |
| `LAYOUTPARSER_CORRELATION_ID` | Correlação nas linhas de log | `N/A` |

- Rotação por tamanho: ~2 MB por arquivo (limite de 2049 KB), até 10 arquivos (`layoutparserlib-*.log`).
- O logger é **resiliente**: falhas de log nunca derrubam o chamador.

---

## Build

Projeto **não-SDK** (clássico). Use **MSBuild** (Developer Prompt do Visual Studio):

```bash
msbuild LayoutParserLib.sln /p:Configuration=Release
# Artefato: bin/Release/LayoutParserLib.dll
```

- Só referências de framework (`System.*`) — **sem** NuGet restore.
- `<Deterministic>true</Deterministic>` — build reproduzível.
- Linguagem: **C# 7.3** (default em net48 não-SDK). Sem records/nullable/implicit usings.

---

## Segurança / Security 🔴

A chave e o IV da criptografia estão **hardcoded** em [`CryptographySysMiddle.cs`](CryptographySysMiddle.cs)
(e replicados na cópia do Decrypt). Há também tamanhos de chave/IV que não correspondem a valores válidos
de Rijndael — **a investigar**. Plano de remediação e regras em
[`.claude/rules/security.md`](.claude/rules/security.md).

- **Nunca** logue texto decriptografado, chave ou IV (o logger só registra tamanhos).
- Rotação de chave é **cross-repo** (Lib + Api + Decrypt) — nunca unilateral.

---

## CI / CD

[`.github/workflows/trigger-deploy.yml`](.github/workflows/trigger-deploy.yml): um push em `master`/`main`
aqui **dispara o workflow de deploy do `LayoutParserApi`** (efeito em cascata). Publicação é tarefa do
`@lib-devops` — ver harness.

---

## Desenvolvimento com IA / AI-assisted dev

Este repo tem um **harness Claude Code** em [`.claude/`](.claude/README.md): 6 agentes (`@lib-*`),
rules de contrato/segurança/padrões, e slash commands (`/security-scan`, `/crypto-review`,
`/check-parity`, `/release-dll`). Comece pelo [`.claude/README.md`](.claude/README.md).

Para consultar o comportamento real de APIs de framework, os agentes usam **better-context (btca)**.
Um **MCP Server** é possibilidade futura, sob revisão de segurança (risco de oráculo de decrypt).
