# Change Request — TASK-XX.XX (Iteração N/3)

- **Iteration:** [1/3 | 2/3 | 3/3]
- **Responsible Agent:** [ENGENHEIRO | ANALISTA]
- **Origin:** [TESTER | REVISOR | MAESTRO]
- **Severity:** [CRITICAL | HIGH | MEDIUM | LOW]
- **Date:** YYYY-MM-DD HH:mm

---

## 1. Finding (Defeito / Inconformidade Identificada)
[Descrição detalhada do defeito, violação de regra de negócio, quebra de arquitetura ou falha de segurança]

## 2. Evidence (Evidência da Falha)
```powershell
# Linha de comando ou log de erro capturado
dotnet test ...
# Saída do erro:
```

## 3. Expected Behavior (Comportamento Esperado)
[O que a especificação e os critérios de aceite determinam que deveria ocorrer]

## 4. Required Correction (Correção Exigida)
[Ação precisa que o agente responsável deve realizar para solucionar a causa raiz]

## 5. Acceptance Criteria for Correction
- [ ] **ACC-01:** [Critério específico que o Tester/Revisor validará nesta iteração]
- [ ] **ACC-02:** [Ausência de regressão em testes já existentes]

## 6. Files in Scope (Arquivos Autorizados para Alteração)
- `src/...`
- `tests/...`

## 7. Restrictions (Restrições)
- Proibido alterar outros módulos ou comportamentos fora do escopo desta correção.
- Proibido silenciar testes ou suprimir verificações de segurança.
