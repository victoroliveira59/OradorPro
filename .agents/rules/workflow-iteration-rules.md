---
trigger: always_on
description: Regras da máquina de estados do Harness, transições válidas, controle de iterações e bloqueio humano.
---

# Regras do Workflow, Estados e Iterações

## 1. Estados Oficiais da Task

Toda tarefa deve estar em exatamente um dos seguintes estados:

| Estado | Significado |
| :--- | :--- |
| `BACKLOG` | Identificada no planejamento, aguardando início da fase ou refinamento. |
| `READY` | Atende à Definition of Ready (DoR); apta para execução pelo agente responsável. |
| `IN_PROGRESS` | Em desenvolvimento ou refinamento pelo agente ativo. |
| `WAITING_REVIEW` | Implementação ou especificação concluída, aguardando testes ou revisão. |
| `TESTING` | Em validação ativa pelo Tester. |
| `NEEDS_CHANGES` | Defeitos ou não conformidades encontrados; direcionada para correção. |
| `BLOCKED` | Bloqueada por falta de dependência, decisão pendente ou limite de correções. |
| `DONE` | Atende integralmente à Definition of Done (DoD), com evidências registradas. |
| `CANCELLED` | Cancelada justificadamente pelo Maestro com aprovação humana. |

## 2. Transições Válidas de Estado

```text
BACKLOG -> READY
READY -> IN_PROGRESS
IN_PROGRESS -> WAITING_REVIEW
WAITING_REVIEW -> TESTING
TESTING -> NEEDS_CHANGES
TESTING -> DONE
NEEDS_CHANGES -> IN_PROGRESS
qualquer estado não terminal -> BLOCKED
BLOCKED -> estado anterior (após resolução do bloqueio)
BACKLOG / READY -> CANCELLED (com justificativa registrada)
```

- **Autoridade Exclusiva:** Apenas o Maestro tem autorização para alterar o estado oficial no arquivo `.ai/workflow-state.md` e em `.ai/tasks.md`.

## 3. Limite de Rodadas de Correção (Iteration Guardrail)

1. Para cada task, permite-se até **três rodadas de correção** (`1/3`, `2/3`, `3/3`), sem contar a entrega inicial.
2. A cada reprovação (originada pelo Tester ou Revisor), o Maestro incrementa o contador de iteração em `.ai/workflow-state.md` e emite uma solicitação de correção padronizada (`Change Request`).
3. Se ao término da terceira iteração a task continuar com pendências críticas ou reprovada:
   - O Maestro **deve obrigatoriamente** mudar o estado da task para `BLOCKED`.
   - O Maestro deve gerar alerta em `.ai/workflow-state.md` e solicitar intervenção e aprovação humana.
   - É estritamente proibido reiniciar o contador de iterações ou forçar avanço sem autorização explícita do usuário.
