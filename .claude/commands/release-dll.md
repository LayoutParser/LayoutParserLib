---
description: Prepara um release da DLL — build Release determinístico, conferência de versão e checklist de publicação.
allowed-tools: Read, Grep, Glob, Bash, Edit
---

# /release-dll

Prepara a publicação da DLL consumida pelo Api. **Push é do `@lib-devops`** e **dispara o deploy do Api**.

## Passos

1. **Pré-checagem**
   - `git status` limpo? branch correta?
   - Rode `/check-parity` e `/security-scan` — bloqueie se houver drift ou segredo novo.
2. **Build Release**
   ```bash
   msbuild LayoutParserLib.sln /p:Configuration=Release
   ```
   - Deve passar sem erros. Artefato em `bin/Release/LayoutParserLib.dll`.
   - `<Deterministic>true</Deterministic>` está ligado — build reproduzível.
3. **Versão**
   - Confira `Properties/AssemblyInfo.cs` (`AssemblyVersion`/`AssemblyFileVersion`).
   - **Mudou a superfície pública?** Faça bump coerente (a DLL é contrato para o Api).
4. **Publicação (somente `@lib-devops`, sob pedido do usuário)**
   - `git push` → ⚠️ aciona `trigger-deploy.yml` → deploy do `LayoutParserApi`.
   - Confirme intenção e tenha noção do efeito em produção.

## Tarefa

A partir do pedido (**$ARGUMENTS**), execute o passo solicitado (ou o fluxo completo até o build).
**Não faça push** a menos que seja explicitamente o `@lib-devops` agindo a pedido do usuário.
Reporte: status do build, versão atual/proposta, resultado de paridade e segurança.
