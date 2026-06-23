---
description: Protocolo de handoff (compactação de contexto) ao trocar de agente.
---

# Agent Handoff — LayoutParserLib

## Propósito

Evitar acúmulo de contexto ao alternar entre agentes (`@lib-*`). Em cada troca, o agente que sai é
compactado num **artefato de handoff (~400 tokens)** em vez de manter sua persona completa.

## Quando se aplica

Sempre que: (1) o usuário invoca um novo agente `@lib-*`, e (2) já havia outro agente ativo.

## Artefato de handoff

Ao sair, gere mentalmente:

```yaml
handoff:
  from_agent: "{agente_atual}"
  to_agent: "{novo_agente}"
  contexto:
    tarefa: "{tarefa/missão em andamento}"
    branch: "{branch git atual}"
    arquivos_tocados: ["{arquivo 1}", "{arquivo 2}"]   # máx. 10
  decisoes:                                            # máx. 5
    - "{decisão-chave 1}"
  contrato:                                            # específico desta lib
    superficie_publica_alterada: false                 # true se mexeu em CryptographySysMiddle público
    impacto_consumidores: "{Api? Decrypt? nenhum}"
  bloqueios: ["{bloqueio ativo, se houver}"]           # máx. 3
  proximo_passo: "{o que o agente que entra deve fazer}"
```

## O que SEMPRE preservar
- Tarefa/missão atual e branch
- Arquivos criados/alterados
- **Se a superfície pública mudou e qual consumidor (Api/Decrypt) é afetado**
- Decisões de cripto/contrato relevantes
- Bloqueios ativos e próximo passo

## O que SEMPRE descartar
- Persona completa do agente anterior
- Lista de tools/missões do agente anterior
- Instruções de contexto já absorvidas

## Limites

| Limite | Valor |
|--------|-------|
| Tamanho do artefato | ~500 tokens |
| Resumos retidos | 3 (o mais antigo é descartado no 4º) |
| Decisões | 5 |
| Arquivos | 10 |
| Bloqueios | 3 |

## Exemplo

`@lib-architect` decide adicionar uma sobrecarga `Decrypt(string, byte[] key, byte[] iv)` → troca para `@lib-crypto-dev`:
- persona do arquiteto (~3K tokens) é **descartada**;
- handoff (~400 tokens) é **retido**: decisão (sobrecarga, não alterar a existente), `superficie_publica_alterada: true`, `impacto_consumidores: "nenhum (aditivo)"`, próximo passo;
- persona do Bruno é **carregada**. Economia ~80% por troca.
