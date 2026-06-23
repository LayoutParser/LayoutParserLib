---
description: Uso de ferramentas e o status (futuro) de um MCP Server para o LayoutParserLib.
---

# MCP & Tools — LayoutParserLib

## Ferramentas nativas primeiro

- Para arquivo/busca/edição use as **tools nativas** do Claude Code (`Read`/`Edit`/`Grep`/`Glob`/`Bash`).
- **better-context (btca)** para consultar código-fonte real de libs/frameworks (ex.: `System.Security.Cryptography`):
  ```bash
  btca ask --resource <lib> --question "como CryptoStream trata padding em CBC?"
  ```
  Ressalva: btca está em **reescrita** e pode ter atrito no **Windows**. Se falhar, caia para `WebSearch`/`WebFetch`.

## MCP Server — status: **NÃO existe (possibilidade futura)**

Ainda **não há** MCP Server neste repo. O usuário levantou "talvez" criar um, por o Lib ser ponto de
conexão do ecossistema. Antes de construir, decidir **o quê** expor e **com que proteção**.

### Risco crítico: oráculo de descriptografia 🔴

Expor `Decrypt` como tool MCP cria um **decryption oracle** — qualquer cliente do MCP poderia
descriptografar conteúdo arbitrário. **Inaceitável** sem autenticação/escopo forte.

### Se for construir (proposta, decisão de `@lib-architect` + `@lib-devops`)

- Tools candidatas **seguras**: inspeção/tail de log, metadados de versão da lib, `/check-parity` como tool.
- `Decrypt` como tool **somente** com autenticação e justificativa explícita — ou **não expor**.
- Padrão do ecossistema (espelhar o Api): **MCP em C# (.NET)**, não Node/Python.
- Gestão de MCP (registro em `.mcp.json`, build) é **exclusiva do `@lib-devops`**.

> Enquanto não existir, este arquivo serve de guarda-corpo: ninguém cria MCP aqui sem passar por
> segurança e autoridade.
