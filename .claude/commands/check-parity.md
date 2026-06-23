---
description: Compara o código deste Lib com as cópias embutidas no LayoutParserDecrypt e reporta drift.
allowed-tools: Read, Grep, Glob, Bash
---

# /check-parity

Verifica a **paridade** entre o source of truth (este repo) e a **cópia de fonte** no `LayoutParserDecrypt`.

## Arquivos a comparar

| Source of truth (este repo) | Cópia no Decrypt |
|-----------------------------|------------------|
| `CryptographySysMiddle.cs` | `LayoutParserDecrypt/LayoutParserLib/CryptographySysMiddle.cs` |
| `RollingFileLogger.cs` | `LayoutParserDecrypt/LayoutParserLib/RollingFileLogger.cs` |

## Como rodar

```bash
# Ajuste o caminho do Decrypt se necessário
DEC="C:/Users/elson.lopes/source/repos/LayoutParserDecrypt/LayoutParserLib"
diff -u "CryptographySysMiddle.cs" "$DEC/CryptographySysMiddle.cs"
diff -u "RollingFileLogger.cs"     "$DEC/RollingFileLogger.cs"
```

(Ou use `Read` nos dois lados e compare se o `diff` não estiver disponível.)

## Tarefa

A partir do pedido (**$ARGUMENTS**) — ou ambos os arquivos se vazio — produza um relatório:

1. **Diferenças por arquivo** (métodos a mais/menos, assinaturas divergentes).
2. **Drift já conhecido:** a cópia embutida `LayoutParserDecrypt/LayoutParserLib/RollingFileLogger.cs` tem `Configure(...)` + campos `_logDir`/`_corr` que **não existem aqui** (esta versão lê env vars). O `CryptographySysMiddle.cs` deve estar em paridade. Confirme se persiste. *(Obs.: o `Log` de 6 args do Decrypt vive em outra classe, `LayoutParserDecrypt.RollingFileLogger`, fora desta comparação.)*
3. **Veredito:** PARIDADE OK / DRIFT — e o que precisa sincronizar em qual direção.
4. Se a divergência for intencional, recomende registrar em [`rules/library-contract.md`](../rules/library-contract.md).

Não sincronize automaticamente — proponha a direção e deixe a decisão de contrato para `@lib-architect`.
