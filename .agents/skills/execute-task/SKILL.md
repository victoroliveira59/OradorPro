---
name: execute-task
description: >-
  Instruções operacionais para agentes especializados (Analista ou Engenheiro) executarem uma tarefa delegada, respeitando escopo delimitado, regras de arquitetura e gerando relatórios padronizados.
---

# Skill: Execute Task

Esta skill define as instruções operacionais para a execução rigorosa de uma tarefa delegada pelo Maestro no projeto **Agenda de Palestras**.

---

## 1. Passo Inicial: Recepção e Verificação de Escopo

1. **Leitura da Ordem do Maestro:**
   - Conferir Fase e Task ID.
   - Identificar o escopo incluído e os itens expressamente excluídos.
   - Ler os critérios de aceite que devem ser comprovados.
2. **Checagem de Decisões e Bloqueios:**
   - Verificar se há alguma dependência não satisfeita ou questão em aberto em `docs/product/open-questions.md`.
   - Se houver impedimento que inviabilize o trabalho, não avançar com suposições: emitir status `BLOCKED` no relatório e notificar o Maestro.

---

## 2. Passo de Execução: Analista (Requisitos e Regras)

1. Analisar as diretrizes do `SPEC.md` pertinentes à task.
2. Escrever ou atualizar os arquivos em `docs/product/`:
   - Detalhar fluxos felizes e de exceção em `requirements.md`.
   - Formalizar regras de negócio estritas em `business-rules.md`.
   - Formatar critérios de aceite objetivos e verificáveis em `acceptance-criteria.md`.
3. Levantar dúvidas objetivas em `open-questions.md` caso surjam ambiguidades.
4. Elaborar `docs/agent-runs/<fase>/<task-id>/analysis-report.md`.

---

## 3. Passo de Execução: Engenheiro (.NET e Arquitetura)

1. Consultar os critérios de aceite definidos pelo Analista e as regras em `.agents/rules/`.
2. Implementar a solução na camada correta:
   - `Domain`: entidades, enums, contratos (sem dependências de infraestrutura).
   - `Application`: casos de uso, DTOs, validadores.
   - `Infrastructure`: mapeamentos EF Core, DbContext, repositórios e adaptadores.
   - `Web`: controllers, Views Razor, ViewModels.
3. Criar os testes unitários da nova lógica implementada.
4. Executar compilação local:
   ```powershell
   dotnet build AgendaPalestras.sln -c Release
   dotnet test tests/AgendaPalestras.UnitTests/ -c Release --no-build
   ```
5. Garantir que nenhum segredo esteja no código ou em arquivos de configuração versionados.
6. Elaborar `docs/agent-runs/<fase>/<task-id>/implementation-report.md`.

---

## 4. Passo de Fechamento: Emissão do Relatório Padronizado

Todo agente executor deve finalizar sua atuação emitindo o relatório estruturado no padrão:

```markdown
# Agent Report

- Agent: [ANALISTA | ENGENHEIRO]
- Phase: [Fase]
- Task: [Task-ID]
- Status: [COMPLETED | BLOCKED | NEEDS_CHANGES | FAILED]

## Resumo
## Arquivos consultados
## Arquivos criados ou alterados
## Decisões tomadas
## Critérios de aceite atendidos
## Testes/comandos executados
## Evidências
## Riscos e pendências
## Próximo agente recomendado
```

Salvar o relatório em `docs/agent-runs/<fase>/<task-id>/` e notificar o Maestro para a transição de estado.
