# Harness Claude Code — LayoutParserLib

Este diretório configura o desenvolvimento assistido por IA do **LayoutParserLib**, a biblioteca de
**criptografia + logging compartilhada** do ecossistema LayoutParser. É **enxuto e focado no domínio**,
inspirado no AIOX (`aiox-core/.claude`) mas sem o peso do framework completo.

> **EN** · AI development harness for LayoutParserLib (the shared Sysmiddle crypto/logging library).
> A lean, domain-focused setup inspired by AIOX. Agents, rules, slash commands and an optional hook.

## Estrutura

```
.claude/
├── CLAUDE.md                 # Comportamento base, padrões e mapa de agentes
├── agents/                   # 6 personas enxutas (subagents) — prefixo lib-
│   ├── lib-architect.md      # Caio   — contrato/compatibilidade (analisa, não coda)
│   ├── lib-crypto-dev.md     # Bruno  — implementação C#/.NET Framework 4.8.1
│   ├── lib-security.md       # Sofia  — cripto + segredos (chave/IV/modo/padding)
│   ├── lib-qa.md             # Quésia — testes + paridade entre cópias
│   ├── lib-devops.md         # Gael   — git push (exclusivo), CI, empacotar/versionar DLL, MCP
│   └── lib-doc.md            # Dora   — documentação bilíngue
├── rules/                    # Regras carregadas por contexto
│   ├── agent-authority.md    # Quem pode o quê (push é só do devops)
│   ├── agent-handoff.md      # Compactação de contexto ao trocar de agente
│   ├── dotnet-standards.md   # Padrões .NET Framework 4.8.1 / C# 7.3 / MSBuild
│   ├── library-contract.md   # Contrato público + paridade entre Lib/Api/Decrypt
│   ├── security.md           # Pendência de chave/IV hardcoded + regras de cripto
│   └── mcp-usage.md          # better-context (btca) + MCP futuro (risco de oráculo)
├── commands/                 # Slash commands
│   ├── security-scan.md      # /security-scan — varre segredos/vazamento
│   ├── crypto-review.md      # /crypto-review — revisa mudança criptográfica
│   ├── check-parity.md       # /check-parity — diff Lib ⇄ cópia do Decrypt
│   └── release-dll.md        # /release-dll — build Release + checklist de publicação
├── hooks/
│   └── git-push-advisory.cjs # Lembrete não-bloqueante sobre push + deploy em cascata (Node)
├── settings.json.example     # Template de settings (idioma + permissões + hook)
└── README.md                 # este arquivo
```

## Como usar

### Agentes

Invoque um agente com `@nome` ou via o tool `Task`:

```
@lib-architect  avalie o impacto de adicionar uma sobrecarga Decrypt(string, key, iv)
@lib-crypto-dev implemente a sobrecarga sem quebrar o contrato existente
@lib-security   audite chave/IV e confirme que nada vaza no log
@lib-qa         valide round-trip e rode a checagem de paridade com o Decrypt
@lib-devops     faça o release/push quando o build estiver verde
```

Fluxo típico:

```
@lib-architect → @lib-crypto-dev → @lib-security → @lib-qa → @lib-doc → @lib-devops
```

### Slash commands

```
/security-scan                 # varredura de segredos + vazamento de plaintext
/crypto-review                 # revisa uma mudança na criptografia
/check-parity                  # compara este Lib com a cópia embutida no Decrypt
/release-dll                   # build Release + checklist de publicação
```

### Ativar o settings (opcional)

O `settings.json` ativo não é criado automaticamente (proteção contra auto-modificação de config).
Para ativar o template:

```bash
cp .claude/settings.json.example .claude/settings.json
# depois remova as chaves de comentário "//..." (JSON não aceita comentários)
```

Ele define: idioma português, uma allowlist de comandos seguros (reduz prompts) e o hook
`git-push-advisory`. **Para desativar o hook**, remova o bloco `hooks` do settings.

> Use `.claude/settings.local.json` para overrides por máquina (não versionar).

## Memória dos agentes

Os agentes usam `memory: project` — fatos persistentes do projeto acumulam na memória do Claude Code
e são recuperados por relevância. Registre aqui decisões não óbvias (ex.: por que a chave/IV está
hardcoded historicamente, qual a direção de sincronização com o Decrypt).

Já estão pré-registrados em memória: o **drift** Lib⇄Decrypt (`Configure` na cópia embutida), a
**chave/IV hardcoded** (cross-repo) e o **push que dispara o deploy do Api**.

## Princípios do harness

1. **Enxuto > completo:** 6 agentes que cobrem o ciclo de uma lib, não 15 genéricos.
2. **Contrato em primeiro lugar:** a superfície pública é consumida por Api e Decrypt — mudança é cross-repo.
3. **Segurança é o produto:** o domínio é cripto; chave/IV/modo/padding e não-vazamento são inegociáveis.
4. **Autoridade clara:** só o `@lib-devops` publica — e push aqui **dispara o deploy do Api**.
5. **Paridade explícita:** o Decrypt copia este fonte; drift é dívida a monitorar (`/check-parity`).
