# Registro de Execuções dos Agentes (Agent Runs)

> Diretório destinado ao arquivamento de relatórios gerados por cada agente ao executar uma tarefa.

---

## Estrutura de Diretórios

Os relatórios devem ser organizados por fase e identificador da tarefa:

```text
docs/agent-runs/
└── <phase>/<task-id>/
    ├── analysis-report.md           # Relatório gerado pelo Analista
    ├── implementation-report.md     # Relatório gerado pelo Engenheiro
    ├── test-report.md               # Relatório gerado pelo Tester
    ├── review-report.md             # Relatório gerado pelo Revisor
    ├── change-request-iter-<N>.md   # Change Request do Maestro (se houver correção)
    └── maestro-report.md            # Relatório de consolidação do Maestro
```

---

## Regras
- **Sem Falsos Registros:** Proibido criar relatórios fictícios antes da execução real da tarefa.
- **Imutabilidade Histórica:** Relatórios de iterações anteriores não devem ser sobrescritos; devem ser versionados ou preservados como histórico da tarefa.
