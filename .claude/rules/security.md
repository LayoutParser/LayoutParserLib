---
description: Regras de segurança do LayoutParserLib e a pendência de chave/IV hardcoded.
---

# Segurança — LayoutParserLib

O domínio desta lib **é** criptografia. Segurança aqui é o produto, não um adendo.

## 1. Pendência aberta: chave e IV hardcoded 🔴

[`CryptographySysMiddle.cs:17`](../../CryptographySysMiddle.cs:17) embute **chave e IV em texto plano** no código:

```csharp
CreateDecryptor(Encoding.UTF8.GetBytes("dbc%$#h92785"),        // chave: 12 bytes
                Encoding.UTF8.GetBytes("Ca#&UjO){Qwz*@FcsPs")); // IV:    19 bytes
```

| Problema | Detalhe |
|----------|---------|
| **Segredo versionado** | Chave/IV no fonte e no histórico git — e replicados na cópia do Decrypt. |
| **Tamanhos suspeitos** | 12 e 19 bytes **não** são válidos para Rijndael (16/24/32). Pode lançar `CryptographicException`. **Investigar** se o caminho roda em produção antes de afirmar quebra. |
| **IV fixo** | CBC com IV constante por mensagem é fraqueza criptográfica conhecida. |

### Plano de remediação (NÃO executar sem confirmação do usuário)
- [ ] **Confirmar** se o decrypt realmente é exercitado (Decrypt/Api) com esses valores — entender o estado real.
- [ ] **Mover** chave/IV para configuração/secure store (env var / `App.config` protegido / DPAPI), fora do fonte.
- [ ] **Rotacionar** chave/IV — **coordenado entre Lib, Api e Decrypt** (a mesma chave vive nos 3) e ciente de que dados já criptografados com a chave antiga ficam ilegíveis.
- [ ] **Limpar histórico git** (`git filter-repo`/BFG) — **só via `@lib-devops`, sob confirmação**. NÃO substitui a rotação.

> ⚠️ Trocar algoritmo/modo/IV **muda o formato** dos dados criptografados. Não "conserte" a cripto
> isoladamente — é decisão de `@lib-architect` + `@lib-security` com plano de migração.

## 2. Não vazar plaintext nem segredos

- O `RollingFileLogger` hoje loga **apenas tamanhos** (`len=`, `outLen=`) — **mantenha assim**.
- **NUNCA** logue: texto decriptografado, chave, IV, base64 de entrada sensível.
- **NUNCA** envie chave ou plaintext real para serviço externo, incluindo LLM em nuvem.

## 3. Regras gerais (todos os agentes)

- **NUNCA** comite segredo, chave, IV ou credencial nova em texto plano.
- Ao **detectar** um segredo (qualquer arquivo), **pare**, sinalize ao usuário e acione `@lib-security` → `@lib-devops`. Não silencie.
- Use `/security-scan` para varredura rápida.
- Qualquer mudança que afete sigilo (modo, IV, padding, tamanho de chave) passa por `@lib-security`.
