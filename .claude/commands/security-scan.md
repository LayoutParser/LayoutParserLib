---
description: Varre o repositório por chave/IV/credencial em texto plano e por vazamento de plaintext em log.
allowed-tools: Read, Grep, Glob, Bash
---

# /security-scan

Varredura de segurança focada no domínio de cripto desta lib.

## O que procurar

1. **Chave/IV hardcoded:** `GetBytes("...")`, `CreateDecryptor`/`CreateEncryptor`, literais passados a `RijndaelManaged`/`Aes`.
2. **Segredos genéricos:** `password`, `senha`, `apikey`, `token`, `connectionstring`, base64 longos suspeitos.
3. **Vazamento em log:** chamadas de log que passem plaintext decriptografado, chave, IV ou conteúdo sensível (deve logar só tamanhos/metadados).

## Como rodar

- `Grep` por `GetBytes\(|CreateDecryptor|CreateEncryptor|RijndaelManaged|Aes`.
- `Grep` (case-insensitive) por `password|senha|apikey|api_key|token|secret|connectionstring`.
- Revise `RollingFileLogger.Log(` e confirme que nenhum argumento carrega plaintext/segredo.

## Tarefa

A partir do pedido (**$ARGUMENTS**) — ou varredura completa se vazio — liste os achados com
arquivo:linha, severidade e recomendação. Para a pendência conhecida de chave/IV, **referencie**
[`rules/security.md`](../rules/security.md) em vez de repetir o plano.

- **Não** altere código nesta varredura — só reporte. Remediação é fluxo do `@lib-security` + `@lib-devops`.
- Lembre o impacto cross-repo: o mesmo segredo existe na cópia do `LayoutParserDecrypt`.
