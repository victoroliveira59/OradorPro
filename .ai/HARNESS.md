# AI Agent Harness — Manual Operacional do Sistema

> Projeto: Agenda de Palestras  
> Versão do Harness: 1.0  
> Status Atual: Fase 00 (Preparação do Harness)  
> Plataforma: Google Antigravity / .NET 8 / PostgreSQL ou SQL Server  
> Idioma: Português (Brasil)

---

## 1. Visão Geral do Harness

O **AI Agent Harness** é a infraestrutura de governança, controle de qualidade, regras operacionais e rastreabilidade que rege o ciclo de vida de desenvolvimento do sistema **Agenda de Palestras**.

O Harness não é um conjunto passivo de instruções. Ele impõe:
- Máquina de estados determinística para tarefas e fases.
- Contrato estrito de comunicação entre agentes especializados.
- Limite estrito de iterações para correções (guardrails anti-loop).
- Requisito mandatório de evidências reais para aprovação.
- Soberania humana permanente para decisões críticas, segredos e produção.

---

## 2. Agentes Especializados

| Agente | Papel | Responsabilidade Principal |
| :--- | :--- | :--- |
| **Maestro** | Orquestrador | Governança da máquina de estados, delegação sequencial, DoR/DoD, controle de iterações e bloqueios. |
| **Analista** | Requisitos e Regras | Especificações funcionais, regras de negócio, critérios de aceite testáveis e catálogo de dúvidas em aberto. |
| **Engenheiro** | Arquiteto e Desenvolvedor | Implementação em .NET, Clean Architecture, Entity Framework Core, Razor MVC e testes unitários. |
| **Tester** | Qualidade e Validação | Execução real de testes, validação de regras de negócio, cenários negativos, idempotência e evidências. |
| **Revisor** | Auditor Independente | Auditoria de código, segurança OWASP, anti-CSRF, isolamento de camadas e matriz de achados por severidade. |

---

## 3. Máquina de Estados e Ciclo de Vida

### 3.1 Estados de uma Task
- `BACKLOG`: Planejada, aguardando início da respectiva fase ou refinamento.
- `READY`: Atende à Definition of Ready (DoR); apta para início.
- `IN_PROGRESS`: Em desenvolvimento ou análise pelo agente ativo.
- `WAITING_REVIEW`: Entrega realizada; aguardando testes e revisão.
- `TESTING`: Sob validação ativa pelo Tester.
- `NEEDS_CHANGES`: Reprovada com apontamentos; aguardando correção.
- `BLOCKED`: Impedimento crítico, dependência externa ou limite de correções atingido.
- `DONE`: Atende à Definition of Done (DoD) com evidências comprovadas.
- `CANCELLED`: Cancelada motivadamente com aprovação humana.

### 3.2 Diagrama de Transições
```text
BACKLOG -> READY -> IN_PROGRESS -> WAITING_REVIEW -> TESTING -> DONE
                                              ↑           ↓
                                              └── NEEDS_CHANGES
Qualquer estado não-terminal -> BLOCKED -> Estado anterior (após resolução)
```

---

## 4. Guardrail de Iterações e Correções

- **Limite Global:** No máximo **3 rodadas de correção** por task (`1/3`, `2/3` e `3/3`).
- **Bloqueio Automático:** Se ao final da 3ª rodada persistirem falhas críticas ou testes reprovados, o Maestro é obrigado a marcar a task como `BLOCKED`.
- **Intervenção Humana:** O desbloqueio exige análise e aprovação explícita do usuário. É proibido reiniciar contadores artificialmente.

---

## 5. Critérios de Entrada e Saída

### 5.1 Definition of Ready (DoR)
1. Objetivo da task claro e escopo incluído/excluído delimitado.
2. Todas as dependências anteriores com status `DONE`.
3. Nenhuma decisão pendente impeditiva em `docs/product/open-questions.md`.
4. Critérios de aceite formulados de maneira testável.

### 5.2 Definition of Done (DoD)
1. Implementação estritamente contida no escopo da task.
2. 100% dos critérios de aceite atendidos.
3. Compilação (`dotnet build`) sem erros.
4. Testes automatizados executados pelo Tester com logs e evidências reais.
5. Revisão do Revisor aprovada sem achados `CRITICAL` ou `HIGH`.
6. Relatórios dos agentes arquivados em `docs/agent-runs/<fase>/<task-id>/`.
7. Maestro registra a transição oficial no `workflow-state.md` e `tasks.md`.

---

## 6. Condições de Parada Imediata

O Maestro deve interromper a execução do workflow e aguardar o usuário quando:
1. Uma decisão de produto ou arquitetura necessária estiver pendente.
2. Uma dependência externa ou credencial estiver indisponível.
3. Houver risco iminente de perda de dados ou quebra de segurança.
4. O limite de 3 iterações de correção for atingido.
5. For exigida ação sensível (deploy, publicação, alteração de banco produtivo).
