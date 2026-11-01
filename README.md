# Agenda de Palestras — AI Agent Harness

> Ambiente profissional de desenvolvimento multiagente para o projeto **Agenda de Palestras**, construído em conformidade com o ecossistema **Google Antigravity**.

---

## 1. Sobre o Projeto
O **Agenda de Palestras** é uma aplicação web ASP.NET Core MVC projetada para organizar a administração de congregações, oradores convidados, discursos públicos e agendas de reuniões especiais, com integração automatizada via WhatsApp para envio de convites, recebimento e interpretação de respostas por webhook e disparos de lembretes preventivos de 7 dias.

---

## 2. O AI Agent Harness

O desenvolvimento do sistema é governado pelo **AI Agent Harness**, um arcabouço rigoroso de controle de estado, qualidade e divisão de papéis:

- **Orquestração pelo Maestro:** Nenhuma tarefa avança sem validação de DoR (Definition of Ready) e DoD (Definition of Done).
- **Execução Sequencial Estrita:** `Analista` → `Engenheiro` → `Tester` → `Revisor` → `Maestro`.
- **Evidências Reais Obrigatórias:** Sem compilação e testes reais executados, não há avanço.
- **Guardrail de Correções:** Limite máximo de **três rodadas de correção** por task com bloqueio compulsório para decisão humana.
- **Soberania Humana:** Decisões de negócio ambíguas, publicação em produção e ações destrutivas requerem aprovação formal do usuário.

---

## 3. Agentes Especializados

| Agente | Arquivo de Definição | Missão Principal |
| :--- | :--- | :--- |
| **Maestro** | [agent.md](file:///c:/git/OradorPro/.agents/agents/maestro/agent.md) | Orquestrador geral, governança de estados, DoR/DoD e controle de iterações. |
| **Analista** | [agent.md](file:///c:/git/OradorPro/.agents/agents/analista/agent.md) | Engenharia de requisitos, modelagem de regras e critérios de aceite testáveis. |
| **Engenheiro** | [agent.md](file:///c:/git/OradorPro/.agents/agents/engenheiro/agent.md) | Arquitetura limpa, desenvolvimento .NET, EF Core e testes unitários. |
| **Tester** | [agent.md](file:///c:/git/OradorPro/.agents/agents/tester/agent.md) | Automação e execução de testes empíricos, validação de regras e evidências. |
| **Revisor** | [agent.md](file:///c:/git/OradorPro/.agents/agents/revisor/agent.md) | Auditoria independente de código, segurança OWASP e conformidade técnica. |

---

## 4. Agent Skills Disponíveis

1. [maestro-orchestration](file:///c:/git/OradorPro/.agents/skills/maestro-orchestration/SKILL.md): Protocolo de início, reconciliação de estado, DoR/DoD e limite de 3 iterações.
2. [execute-task](file:///c:/git/OradorPro/.agents/skills/execute-task/SKILL.md): Instruções para Analista e Engenheiro executarem tarefas delimitadas.
3. [validate-task](file:///c:/git/OradorPro/.agents/skills/validate-task/SKILL.md): Procedimentos de validação empírica do Tester e auditoria do Revisor.
4. [release-project](file:///c:/git/OradorPro/.agents/skills/release-project/SKILL.md): Validação dos 10 critérios mandatórios de release e encerramento de fases.

---

## 5. Estrutura do Repositório

```text
AgendaPalestras/
├── .agents/                        # Customizações Antigravity nativas
│   ├── agents/                     # Definições operacionais dos 5 agentes
│   ├── rules/                      # Regras especializadas (sempre ativas / por demanda)
│   └── skills/                     # Agent Skills (maestro, execute, validate, release)
│
├── .ai/                            # Mecanismo de controle do Harness
│   ├── HARNESS.md                  # Manual operacional detalhado
│   ├── workflow-state.md           # Estado persistente da máquina de estados
│   ├── tasks.md                    # Catálogo mestre de todas as fases e tarefas
│   ├── agent-contract.md           # Interfaces de comunicação e relatórios
│   ├── execution-log.md            # Log cronológico de auditoria
│   └── prompts/                    # Prompts operacionais dos agentes
│
├── docs/                           # Documentação central do projeto
│   ├── product/                    # Visão, requisitos, regras e dúvidas abertas
│   ├── architecture/               # Arquitetura, modelo de dados, integrações e segurança
│   ├── decisions/                  # Architecture Decision Records (ADRs)
│   ├── phases/                     # Documentação e critérios das Fases 00 a 09
│   ├── templates/                  # Templates de tasks, ADRs, relatórios e change request
│   └── agent-runs/                 # Evidências e relatórios de execução por task
│
├── AGENTS.md                       # Regras globais permanentes do projeto
├── SPEC.md                         # Especificação original do projeto
└── README.md                       # Apresentação do repositório
```

---

## 6. Estado Atual

- **Fase Ativa:** 00 - Preparação do Harness
- **Task Ativa:** T00.01 - Criar e validar estrutura do Harness
- **Status:** Preparação concluída, aguardando validação e aprovação humana para transição.
