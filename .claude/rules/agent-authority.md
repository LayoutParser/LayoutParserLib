---
description: Matriz de autoridade e delegação entre os agentes do LayoutParserLib.
---

# Agent Authority — LayoutParserLib

## Matriz de delegação

### @lib-devops (Gael) — Autoridade EXCLUSIVA

| Operação | Exclusivo? | Outros agentes |
|----------|-----------|----------------|
| `git push` / `git push --force` | SIM | BLOQUEADO |
| `gh pr create` / `gh pr merge` | SIM | BLOQUEADO |
| Editar `.github/workflows/` (inclui `trigger-deploy.yml`) | SIM | BLOQUEADO |
| Empacotar/versionar a DLL, `Properties/AssemblyInfo.cs` (versão) | SIM | BLOQUEADO |
| Adicionar/configurar MCP (`.mcp.json`) | SIM | BLOQUEADO |
| Rotação/migração de chave/IV | SIM | BLOQUEADO |

> ⚠️ **Push aqui dispara o deploy do Api** (via `trigger-deploy.yml`). Tratar como operação de produção.

### @lib-architect (Caio) — Design

| Possui | Delega para |
|--------|-------------|
| Decisões de contrato/compatibilidade da lib | — |
| Estratégia de de-duplicação (Decrypt) | `@lib-crypto-dev` (implementação) |
| Especificação de um eventual MCP | `@lib-devops` (registro) / `@lib-crypto-dev` (código) |
| **NÃO** escreve código de produção | `@lib-crypto-dev` |

### @lib-crypto-dev (Bruno) — Implementação

| Permitido | Bloqueado |
|-----------|-----------|
| `git add`, `git commit`, `git status`, `git diff` (local) | `git push` → `@lib-devops` |
| Editar `CryptographySysMiddle.cs` / `RollingFileLogger.cs` | `gh pr create/merge` → `@lib-devops` |
| Branch/checkout/merge local | Editar CI/versão/MCP → `@lib-devops` |

### @lib-security (Sofia) — Cripto & Segredos

| Possui | Não possui |
|--------|-----------|
| Auditoria de cripto (chave/IV/modo/padding), leak-check | git push / CI |
| Detectar segredo → BLOQUEIA e aciona `@lib-devops` | Rotação unilateral (coordena com devops) |

### @lib-qa (Quésia) — Qualidade

| Possui | Não possui |
|--------|-----------|
| Quality gates, round-trip, **parity-check**, veredito PASS/FAIL | Implementar a correção (devolve a dev) |
| Criar/rodar projeto de testes | git push |

### @lib-doc (Dora) — Documentação

| Possui | Não possui |
|--------|-----------|
| README bilíngue, XML docs, diagramas | Código de produção · git push |

## Fluxos de delegação

```
Mudança:    @lib-architect (avalia contrato) → @lib-crypto-dev (implementa)
            → @lib-security (audita) → @lib-qa (valida + paridade) → @lib-doc (documenta) → @lib-devops (push)

Git push:   QUALQUER agente → @lib-devops  (lembrar: dispara deploy do Api)

Segredo:    QUALQUER agente detecta → @lib-security → @lib-devops (rotação coordenada)
```

## Escalonamento

1. Agente não consegue concluir → escalar ao usuário com contexto.
2. Quality gate / paridade falha → retorna ao dev com feedback específico.
3. Segredo/credencial detectado → BLOQUEIA commit, aciona `@lib-security` + `@lib-devops`.
4. Mudança de superfície pública → exige aval do `@lib-architect` (impacto em Api e Decrypt).
