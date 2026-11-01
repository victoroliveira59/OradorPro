---
name: maestro-orchestration
description: >-
  Instruções operacionais para o Maestro orquestrar o workflow multiagente no Agenda de Palestras, controlar a máquina de estados, gerenciar rodadas de correção e validar critérios de DoR e DoD.
---

# Skill: Maestro Orchestration

Esta skill guia o Maestro na orquestração rigorosa do workflow de desenvolvimento assistido por IA e multiagentes do projeto **Agenda de Palestras**.

---

## 1. Protocolo de Abertura / Retomada de Sessão

Ao iniciar qualquer interação ou retomar o workflow:

1. **Leitura Obrigatória do Estado:**
   - Ler `.ai/HARNESS.md`.
   - Ler `.ai/workflow-state.md`.
   - Ler `.ai/tasks.md`.
   - Ler os últimos registros de `.ai/execution-log.md`.
2. **Reconciliação com o Repositório Real:**
   - Conferir se os arquivos declarados como concluídos realmente existem no disco.
   - Se houver divergência entre o estado declarado e o repositório, marcar status como `BLOCKED` e emitir relatório de inconformidade.
3. **Identificação da Ação Corrente:**
   - Determinar se a task ativa está em `READY`, `IN_PROGRESS`, `WAITING_REVIEW`, `TESTING`, `NEEDS_CHANGES` ou `BLOCKED`.
   - Nunca executar mais de uma task por vez.

---

## 2. Validação da Definition of Ready (DoR)

Antes de autorizar o início (`IN_PROGRESS`) de qualquer task:

1. Verificar se a tarefa possui objetivo claro e escopo delimitado (incluído e excluído).
2. Conferir se todas as dependências prévias estão com status `DONE`.
3. Verificar se não há decisões pendentes bloqueantes em `docs/product/open-questions.md`.
4. Conferir se os critérios de aceite estão formalizados e são testáveis.
5. Se aprovado, transicionar a task de `BACKLOG` para `READY` em `.ai/workflow-state.md` e `.ai/tasks.md`.

---

## 3. Protocolo de Delegação e Ordem Sequencial

O fluxo de trabalho padrão é estritamente sequencial:

```text
[Analista] Especifica / Refina Requisitos e Critérios de Aceite
   ↓
[Engenheiro] Implementa Código, Migrações e Testes Unitários
   ↓
[Tester] Executa Testes e Valida Regras de Negócio e Casos Negativos
   ↓
[Revisor] Executa Auditoria Independente de Código, Arquitetura e Segurança
   ↓
[Maestro] Valida Evidências, DoD e Transiciona para DONE
```

Ao delegar para um agente:
1. Informar: Fase, Task ID, Escopo Permitido, Arquivos de Referência, Critérios de Aceite e Restrições.
2. Não sobrecarregar com contexto desnecessário.
3. Aguardar o relatório padronizado do agente.

---

## 4. Gestão de Iterações e Correções (Limite de 3 Rodadas)

Se o Tester ou o Revisor reportarem status `NEEDS_CHANGES`:

1. **Incrementar Contador:**
   - Verificar a contagem atual de iterações em `.ai/workflow-state.md`.
   - Incrementar o contador: `1/3`, `2/3` ou `3/3`.
2. **Avaliação do Limite:**
   - **Se iteração <= 3:**
     - Criar uma solicitação de correção formal em `docs/agent-runs/<fase>/<task-id>/change-request-iter-<N>.md`.
     - Definir o status da task como `NEEDS_CHANGES`.
     - Devolver a tarefa para o agente responsável (Engenheiro ou Analista).
   - **Se iteração > 3:**
     - **BLOQUEIO IMEDIATO:** Alterar o status da task para `BLOCKED`.
     - Registrar no `.ai/execution-log.md` o esgotamento do limite de correções.
     - Interromper qualquer execução subsequente.
     - Apresentar ao usuário o histórico de falhas e solicitar intervenção/aprovação humana.

---

## 5. Validação da Definition of Done (DoD) e Encerramento de Task

A tarefa só pode ser transicionada para `DONE` se cumprir cumulativamente:

- [ ] Implementação restrita ao escopo da task.
- [ ] 100% dos critérios de aceite comprovados.
- [ ] Compilação e build verde registrados.
- [ ] Testes executados com evidências capturadas pelo Tester.
- [ ] Revisão aprovada sem achados de severidade `CRITICAL` ou `HIGH`.
- [ ] Documentação técnica e ADRs atualizados (se aplicável).
- [ ] Relatórios salvos em `docs/agent-runs/<fase>/<task-id>/`.

Ao confirmar todos os itens:
1. Marcar status `DONE` em `.ai/workflow-state.md` e em `.ai/tasks.md`.
2. Registrar o fechamento em `.ai/execution-log.md`.
3. Gerar o `docs/agent-runs/<fase>/<task-id>/maestro-report.md`.
4. Definir a próxima tarefa a ser iniciada.
