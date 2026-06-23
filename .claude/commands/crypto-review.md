---
description: Revisa uma mudança na criptografia quanto a corretude (algoritmo/modo/padding/chave/IV) e compatibilidade.
allowed-tools: Read, Grep, Glob, Bash, WebSearch, WebFetch
---

# /crypto-review

Revisão criptográfica de uma mudança (ou do estado atual) de [`CryptographySysMiddle.cs`](../../CryptographySysMiddle.cs).

## Checklist

- **Algoritmo/modo:** Rijndael/AES, CBC. Mudança altera o formato dos dados já criptografados? (compat!)
- **Chave/IV:** tamanho válido (16/24/32 bytes)? origem segura (não hardcoded)? IV previsível?
- **Padding:** consistente entre encrypt e decrypt?
- **Streams/recursos:** `CryptoStream`/`MemoryStream`/transform descartados corretamente (`using`/`Dispose`)?
- **Tratamento de erro:** exceção propaga com contexto sem vazar segredo no log/mensagem?
- **Round-trip:** existe (ou dá para criar) um caso conhecido entrada→saída para validar?

## Tarefa

A partir do pedido (**$ARGUMENTS**) — ou do `git diff` se vazio — avalie a mudança contra o checklist.
Use **btca** (`btca ask --resource <lib> --question ...`) para confirmar comportamento real de
`System.Security.Cryptography` quando houver dúvida.

- Reporte **compatibilidade** explicitamente: a mudança consegue decriptar dados gerados pela versão anterior?
- Aponte impacto em Api e Decrypt. Não declare OK sem um round-trip mental/real coerente.
- Mudança de algoritmo/modo/IV exige decisão de `@lib-architect` + plano de migração — sinalize.
