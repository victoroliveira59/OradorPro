# Diretrizes Globais de Desenvolvimento — Agenda de Palestras

> Regras permanentes e universais para todos os agentes e desenvolvedores que atuam no repositório.
> Estas regras têm precedência e devem ser obedecidas em todas as fases e tarefas.

---

## 1. Princípios Fundamentais

1. **Uma task ativa por vez:** Nenhum agente deve executar múltiplas tarefas concorrentemente sem autorização expressa do Maestro.
2. **Execução sequencial por padrão:** O ciclo de entrega segue rigorosamente:
   `Analista` → `Engenheiro` → `Tester` → `Revisor` → `Maestro`.
3. **Sem implementação sem especificação:** Nenhuma linha de código deve ser escrita sem requisitos claros, regras de negócio definidas e critérios de aceite testáveis.
4. **Sem conclusão sem evidência:** Uma tarefa só é declarada `DONE` quando logs, comandos, testes executados e relatórios comprovarem o resultado. Suposição não é evidência.
5. **Mudanças estritamente rastreáveis:** Toda alteração de arquivo deve estar associada a uma fase e task ativa registrada em `.ai/workflow-state.md` e `.ai/tasks.md`.
6. **Escopo estrito:** Os agentes são proibidos de expandir o escopo deliberadamente ou assumir funcionalidades não solicitadas.
7. **Registro obrigatório de decisões (ADRs):** Decisões técnicas ou de produto de impacto duradouro devem ser registradas em `docs/decisions/` no formato ADR.
8. **Segurança por padrão (Security by Design):** Credenciais, segredos, chaves de API e senhas NUNCA devem ser commitados. Validações devem ocorrer no backend, e dados sensíveis devem ser protegidos.
9. **Falha não é sucesso:** Erros de compilação, testes ignorados, testes quebrados ou relatórios incompletos devem resultar em status `FAILED` ou `NEEDS_CHANGES`.
10. **Soberania humana (Human-in-the-Loop):** Decisões de negócio ambíguas, publicação em produção, custos e ações destrutivas requerem obrigatoriamente aprovação humana.

---

## 2. Papéis dos Agentes

- **Maestro (Orquestrador):** Conduz o fluxo, valida pré-condições (DoR), pós-condições (DoD), atualiza estados, delega tasks, controla rodadas de correção e interrompe o fluxo quando necessário. Não codifica funcionalidades de negócio.
- **Analista (Requisitos e Regras):** Refina especificações, detalha regras de negócio, modela jornadas, lista critérios de aceite e mapeia dúvidas pendentes (`open-questions.md`). Não codifica.
- **Engenheiro (Arquitetura e Implementação .NET):** Escreve código limpo, migrations, testes unitários associados, serviços e controllers seguindo os padrões arquiteturais estabelecidos.
- **Tester (Qualidade e Validação):** Valida a implementação de forma objetiva, executa suites de testes, valida cenários negativos, concorrência e bordas, e gera relatórios com comandos reais e evidências.
- **Revisor (Revisão Independente):** Inspeciona diffs, segurança (OWASP), padrões de código, aderência à arquitetura e critérios de aceite. Atribui severidades (`CRITICAL`, `HIGH`, `MEDIUM`, `LOW`).

---

## 3. Limite de Rodadas de Correção

- O limite máximo é de **três rodadas de correção** por task (iterações 1/3, 2/3 e 3/3), sem contar a execução inicial.
- Se a task não atingir aprovação na terceira rodada, o Maestro deve marcá-la obrigatoriamente como `BLOCKED` e suspender o avanço até intervenção e decisão humana.
- É proibido zerar o contador de iterações para contornar o bloqueio.

---

## 4. Padrões de Comunicação e Relatórios

- Toda interação deve seguir o contrato em `.ai/agent-contract.md`.
- Toda finalização de trabalho de um agente deve produzir seu respectivo relatório estruturado em `docs/agent-runs/<fase>/<task-id>/`.
- O idioma obrigatório para toda a documentação, comentários e relatórios é **Português do Brasil (pt-BR)**. Código-fonte, identificadores e APIs seguem convenções em Inglês.
