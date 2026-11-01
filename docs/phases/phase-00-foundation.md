# Fase 00 — Preparação do Harness

> Status: IN_PROGRESS  
> Responsável Principal: Maestro  
> Dependência: Nenhuma (Fase Raiz)

---

## 1. Objetivo
Preparar a infraestrutura completa do AI Agent Harness, definindo papéis, contratos, skills, regras do projeto, diretórios de documentação e controle de estado, tornando o repositório pronto para a execução multiagente no Antigravity.

---

## 2. Tarefas da Fase

- [x] **T00.01:** Criar e validar estrutura de diretórios, regras e Harness.
- [x] **T00.02:** Criar contratos e definições operacionais dos 5 agentes (`maestro`, `analista`, `engenheiro`, `tester`, `revisor`).
- [x] **T00.03:** Inicializar controle de estado e logs (`workflow-state.md`, `tasks.md`, `execution-log.md`).
- [x] **T00.04:** Criar templates de task, relatórios, ADRs e change request.
- [x] **T00.05:** Definir regras de branches, commits e guardrails do Git em `AGENTS.md`.
- [ ] **T00.06:** Validar que todos os agentes leem e produzem artefatos e obter aprovação humana.

---

## 3. Critérios de Saída da Fase (Exit Criteria)
1. Todos os 5 arquivos de agentes criados em `.agents/agents/<name>/agent.md` com frontmatter válido.
2. Todas as 4 Agent Skills criadas em `.agents/skills/<name>/SKILL.md`.
3. Regras permanentes criadas em `AGENTS.md` e regras especializadas em `.agents/rules/`.
4. Arquivos de controle em `.ai/` operacionais e sincronizados.
5. Templates operacionais criados em `docs/templates/`.
6. Validação do Harness realizada sem erros de caminhos ou de sintaxe.
7. Aprovação humana formal registrada antes do avanço para a Fase 01.
