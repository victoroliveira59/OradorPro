# Contrato de Comunicação e Execução Multiagente

> Este contrato define as interfaces formais de entrada, saída, transição e tratamento de erros entre os agentes no AI Agent Harness do projeto Agenda de Palestras.

---

## 1. Contrato de Invocação (Entrada para o Agente)

Toda delegação de tarefa realizada pelo **Maestro** para qualquer agente deve fornecer estruturadamente os seguintes 8 elementos:

1. **Fase:** Identificador e nome da fase ativa (ex: `01 - Discovery e Requisitos`).
2. **Task:** Identificador e título formal da task (ex: `T01.03 - Especificar módulo de Congregações`).
3. **Objetivo e Escopo:**
   - *Escopo Incluído:* O que deve ser feito nesta chamada.
   - *Escopo Excluído:* O que expressamente NÃO deve ser alterado ou implementado.
4. **Contexto e Arquivos Obrigatórios:** Lista exata de arquivos que o agente deve ler antes de agir.
5. **Dependências e Decisões Prévias:** ADRs aplicáveis e entregáveis de tasks anteriores.
6. **Critérios de Aceite:** Lista testável e mensurável que comprova a entrega.
7. **Restrições Mandatórias:** Ações proibidas (ex: não commitar credenciais, não suprimir testes, não inventar decisões).
8. **Formato e Destino da Entrega:** Nomes e diretórios dos arquivos que devem ser gerados.

---

## 2. Contrato de Saída (Agent Report Padronizado)

Toda execução de agente deve terminar obrigatoriamente com a emissão de um relatório estruturado no arquivo:
`docs/agent-runs/<fase>/<task-id>/<agent>-report.md`

### Template Mandatório de Relatório:

```markdown
# Agent Report

- Agent: [MAESTRO | ANALISTA | ENGENHEIRO | TESTER | REVISOR]
- Phase: [Identificador da Fase]
- Task: [Identificador da Task]
- Status: [COMPLETED | BLOCKED | NEEDS_CHANGES | FAILED]
- Iteration: [0/3 para primeira entrega, ou 1/3, 2/3, 3/3 para correções]

## Resumo
[Descrição sucinta em português das ações realizadas]

## Arquivos Consultados
- [Caminho do arquivo 1]
- [Caminho do arquivo 2]

## Arquivos Criados ou Alterados
- [Caminho do arquivo 1]
- [Caminho do arquivo 2]

## Decisões Tomadas
[Decisões técnicas pontuais tomadas estritamente dentro do escopo]

## Critérios de Aceite Atendidos
- [x] [Critério 1]: [Explicação de como foi atendido]
- [x] [Critério 2]: [Explicação de como foi atendido]

## Testes e Comandos Executados
```powershell
# Linha de comando real executada
dotnet test ...
```

## Evidências
[Logs capturados, trechos de saída de testes, links para artefatos]

## Riscos e Pendências
[Riscos identificados, limitações ou dúvidas]

## Próximo Agente Recomendado
[Indicação do próximo agente no fluxo]
```

---

## 3. Semântica dos Status de Saída

- **`COMPLETED`**: A tarefa foi cumprida integralmente dentro do escopo, todos os critérios de aceite da etapa foram atendidos e comprovados com evidências.
- **`BLOCKED`**: O agente foi impedido de avançar devido a dependência não atendida, ambiguidade de especificação ou ausência de decisão humana em `docs/product/open-questions.md`. O agente não deve preencher lacunas com suposições.
- **`NEEDS_CHANGES`**: Emitido por Tester ou Revisor ao identificar defeitos, testes reprovados, violações de arquitetura ou falhas de segurança. Exige ciclo de correção.
- **`FAILED`**: Falha irrecuperável, quebra de compilação sem possibilidade de avanço ou erro grave de ambiente.

---

## 4. Contrato de Solicitação de Correção (Change Request)

Quando o status for `NEEDS_CHANGES`, o Maestro gera um documento formal em:
`docs/agent-runs/<fase>/<task-id>/change-request-iter-<N>.md`

```markdown
# Change Request — TASK-XX.XX

- Iteration: [1/3 | 2/3 | 3/3]
- Responsible agent: [ENGENHEIRO | ANALISTA]
- Origin: [TESTER | REVISOR | MAESTRO]
- Severity: [CRITICAL | HIGH | MEDIUM | LOW]

## Finding
[Descrição objetiva do defeito ou violação encontrada]

## Evidence
[Log de erro, stack trace ou trecho de código reprovado]

## Expected Behavior
[Comportamento esperado com base na especificação e critérios de aceite]

## Required Correction
[Ação específica necessária para sanar o problema]

## Acceptance Criteria for Correction
- [ ] [Critério de verificação da correção]

## Files in Scope
- [Arquivos autorizados para alteração]

## Restrictions
[Proibição de alterar outros componentes fora do escopo do defeito]
```

---

## 5. Regras Rígidas de Hand-off

1. **Sequência:** O fluxo padrão `Analista` → `Engenheiro` → `Tester` → `Revisor` → `Maestro` nunca pode ser invertido ou encurtado.
2. **Sem Auto-Aprovação:** Um agente não pode validar ou revisar seu próprio trabalho.
3. **Evidência Obrigatória:** Relatório sem evidência de comandos reais resulta em devolução imediata pelo Maestro.
4. **Respeito ao Limite de 3 Rodadas:** Ao final da 3ª rodada de correção (`3/3`), se a task não estiver aprovada, o Maestro aciona bloqueio (`BLOCKED`) e transfere a decisão para o usuário.
