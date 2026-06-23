---
description: Contrato público da lib e paridade entre as cópias do código (Lib, Api, Decrypt).
---

# Library Contract & Parity — LayoutParserLib

Este Lib é **consumido por outros projetos**. Mudá-lo sem critério quebra produção em cascata.

## 1. Superfície pública (o contrato)

| Membro | Assinatura | Consumido por |
|--------|-----------|---------------|
| `CryptographySysMiddle` | `public class` | Api (DLL), Decrypt (cópia) |
| `CryptographySysMiddle.Decrypt` | `public static string Decrypt(string)` | Api, Decrypt |
| `RollingFileLogger` | `internal static` | Interno. A cópia embutida no Decrypt também é `internal`, mas **adicionou `Configure` + campos** — drift. |

**Regras:**
- **Não altere** a assinatura de `Decrypt(string)`. Para evoluir, **adicione** sobrecarga.
- Não promova/rebaixe visibilidade pública sem o `@lib-architect`.
- Mudou o contrato? **Versione** a DLL e documente a migração de Api e Decrypt.

## 2. As três cópias do código

```
LayoutParserLib (este)        → source of truth   (CryptographySysMiddle.cs, RollingFileLogger.cs)
LayoutParserApi               → referência de DLL  (consome o binário compilado)
LayoutParserDecrypt           → CÓPIA DE FONTE     (.csproj: <Compile Include="LayoutParserLib\*.cs">)
```

> O `.csproj` do Decrypt explica: *"o repositório do Decrypt não contém a solução do Lib como sibling,
> por isso incluímos o código mínimo do Lib como arquivos fonte locais."*

## 3. Drift conhecido (dívida ativa) 🔴

- A cópia embutida `LayoutParserDecrypt/LayoutParserLib/RollingFileLogger.cs` (mesmo namespace `LayoutParserLib`, também `internal static`) **adicionou** `Configure(logDir, correlationId)` + os campos estáticos `_logDir`/`_corr`, e seu `Log(level, message, ex)` passou a usar esses campos. **Esta versão (source of truth) NÃO tem `Configure`** — lê as env vars a cada chamada.
- `CryptographySysMiddle.cs` está **em paridade** (idêntico a menos de formatação).
- ⚠️ **Não confundir:** o Decrypt tem **outra** classe de log própria, `LayoutParserDecrypt.RollingFileLogger` (namespace `LayoutParserDecrypt`), com um `Log(logDir, file, corr, level, msg, ex)` de 6 args. Isso **não** faz parte do Lib compartilhado e **não** é drift dele.
- **Implicação:** o "source of truth" do logger hoje é uma ficção — há duas verdades. Resolver é prioridade de arquitetura.

## 4. Política de paridade

- Toda mudança em `CryptographySysMiddle.cs` ou `RollingFileLogger.cs` **deve** considerar o espelhamento no Decrypt.
- Use `/check-parity` para comparar e listar diferenças antes de concluir.
- `@lib-qa` trata divergência como **FAIL de paridade** até ser resolvida ou conscientemente aceita.

## 5. Caminho para eliminar a duplicação (proposta — decisão do `@lib-architect`)

1. **NuGet interno** (feed privado/local): o Decrypt passa a referenciar o pacote, não copiar fonte.
2. **Shared Project** (`.shproj`) ou **git submodule** apontando para este repo.
3. **Mínimo viável agora:** um `/check-parity` no CI que falha quando as cópias divergem.

Nada disso é trivial (o Decrypt compila isolado no CI hoje). Trazer ao usuário antes de executar.
