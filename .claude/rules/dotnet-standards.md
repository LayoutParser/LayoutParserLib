---
description: Padrões de código .NET Framework obrigatórios para o LayoutParserLib. Carregar ao editar arquivos .cs.
globs: ["**/*.cs", "**/*.csproj"]
---

# Padrões .NET — LayoutParserLib

Projeto: **class library .NET Framework 4.8.1**, formato **não-SDK** (ToolsVersion 15.0, `Microsoft.CSharp.targets`).
`<Deterministic>true</Deterministic>` ligado. **Sem** `Nullable`, **sem** `ImplicitUsings`.

## Versão da linguagem (C# 7.3)

Projeto clássico net48 usa **C# 7.3 por padrão**. **NÃO** use:
- `record` / `record struct`
- nullable reference types (`string?`, `#nullable enable`)
- `using` implícito / `global using`
- namespaces file-scoped (`namespace X;`)
- top-level statements
- `init` accessors, target-typed `new()`, pattern matching avançado (C# 8+)

**OK** (C# 7.3): interpolação `$"..."`, `?.`, `??`, `?:`, expression-bodied members, `out var`, tuplas.

## Build

- **Use MSBuild** (projeto não-SDK): `msbuild LayoutParserLib.sln /p:Configuration=Release` no Developer
  Prompt do VS. `dotnet build` pode funcionar via MSBuild embutido, mas o canônico aqui é `msbuild`.
- **Sem NuGet restore** — só referências de framework (`System`, `System.Core`, `System.Xml`, etc.).
- Não converta para SDK-style nem mude o `TargetFrameworkVersion` sem o `@lib-architect` (consumidores dependem do net48).

## Superfície pública = contrato

- A única superfície pública é `CryptographySysMiddle` (classe `public`, `static string Decrypt(string)`).
- `RollingFileLogger` é `internal static` — mantenha assim a menos que haja decisão de expor.
- **Adicione** sobrecargas; **não altere** assinaturas públicas existentes sem aval (quebra Api + Decrypt).
- Ver [`library-contract.md`](library-contract.md).

## Criptografia

- `System.Security.Cryptography` — `RijndaelManaged` (legado), modo CBC, `CreateDecryptor(key, iv)`, `CryptoStream`.
- Não troque algoritmo/modo/padding sem o `@lib-architect` + `@lib-security`: muda o formato dos dados já criptografados.
- Descarte recursos cripto/streams adequadamente (`using`/`Dispose`) — hoje o `CryptoStream` é fechado manualmente; ao mexer, prefira `using`.

## Logging

- `RollingFileLogger` é **resiliente por design**: o `try/catch { }` externo engole erros para **nunca**
  derrubar o chamador. Preserve isso.
- Config via env vars `LAYOUTPARSER_LOG_DIR` e `LAYOUTPARSER_CORRELATION_ID`.
- **`File.AppendAllText` não é thread-safe** — sob concorrência, é risco real. Trate se for relevante.
- **Nunca** logue plaintext decriptografado, chave, IV ou conteúdo sensível — só tamanhos/metadados.

## Estilo

- Comentários em **PT-BR**, no tom já presente.
- Nomes: `PascalCase` (tipos/métodos), `camelCase` (locais), `_camelCase` (campos privados), `IFoo` (interfaces).
- `using`s explícitos e ordenados (não há ImplicitUsings).

## Antes de concluir

```bash
msbuild LayoutParserLib.sln /p:Configuration=Release   # obrigatório passar
```
- Mexeu na cripto? Valide um **round-trip** real.
- Mexeu na cripto ou no logger? Rode `/check-parity` (impacto na cópia do Decrypt).
