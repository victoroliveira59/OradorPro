---
name: validate-task
description: >-
  Instruções operacionais para os agentes de validação (Tester e Revisor) realizarem testes automatizados, verificação de regras de negócio, auditoria de segurança e revisão independente com captura de evidências.
---

# Skill: Validate Task

Esta skill orienta o processo de validação independente das entregas no projeto **Agenda de Palestras**, cobrindo o ciclo do **Tester** (validação empírica) e do **Revisor** (auditoria independente).

---

## 1. Fase de Testes: Atuação do Tester

O Tester atua quando a task está no estado `TESTING`:

### 1.1 Preparação dos Cenários
1. Ler os critérios de aceite em `docs/product/acceptance-criteria.md`.
2. Ler a especificação do Analista e o relatório de implementação do Engenheiro.
3. Projetar cenários para:
   - Caminhos válidos (Happy Path).
   - Invariantes violadas (dados inválidos, orador/discurso inativo).
   - Regras de conflito e unicidade de agenda.
   - Idempotência de webhooks do WhatsApp e jobs de lembrete (7 dias).

### 1.2 Execução e Coleta de Evidências
1. Executar as suites de teste reais via terminal:
   ```powershell
   dotnet test tests/AgendaPalestras.UnitTests/ -c Release --logger "console;verbosity=normal"
   dotnet test tests/AgendaPalestras.IntegrationTests/ -c Release --logger "console;verbosity=normal"
   ```
2. Registrar no relatório:
   - Linha de comando executada.
   - Total de testes aprovados, reprovados e ignorados.
   - Saída de erro e stack trace caso haja falha.

### 1.3 Resultado do Tester
- Se **100% dos testes passarem**: emitir parecer `COMPLETED`.
- Se **houver qualquer falha**: detalhar o defeito e emitir parecer `NEEDS_CHANGES`.
- Salvar em `docs/agent-runs/<fase>/<task-id>/test-report.md`.

---

## 2. Fase de Revisão: Atuação do Revisor

O Revisor atua após os testes ou em conjunto no ciclo de revisão:

### 2.1 Auditoria do Diff
1. Executar inspeção em todos os arquivos modificados na task:
   - Respeito à Clean Architecture (Domain puro, Application isolada, Infrastructure desacoplada).
   - Controllers limpos delegando para Application.
   - DTOs separados de entidades de banco.
   - Presença de validações no backend.
   - Uso de `CancellationToken` em métodos assíncronos.

### 2.2 Auditoria de Segurança (OWASP)
1. **Segredos e Credenciais:** Garantir que nenhuma chave, senha ou token tenha sido inserido.
2. **Injeção de Código e SQL:** Conferir se todas as consultas são parametrizadas pelo EF Core.
3. **Anti-CSRF:** Conferir validação de tokens nos formulários POST.
4. **Webhooks:** Conferir validação de cabeçalhos ou assinatura do webhook do WhatsApp.

### 2.3 Matriz de Achados e Severidade
Classificar cada apontamento:
- `CRITICAL`: Quebra grave, vazamento de segredo, perda de dados.
- `HIGH`: Falha em regra de negócio, autorização ou integridade.
- `MEDIUM`: Débito técnico, falta de tratamento de exceção ou teste faltante.
- `LOW`: Sugestão cosmética ou melhoria leve de legibilidade.

### 2.4 Parecer do Revisor
- Se houver achado `CRITICAL` ou `HIGH`: emitir parecer `NEEDS_CHANGES`.
- Se apenas achados `MEDIUM` ou `LOW` ou nenhum achado: emitir parecer `COMPLETED`.
- Salvar em `docs/agent-runs/<fase>/<task-id>/review-report.md`.

---

## 3. Consolidação pelo Maestro

O Maestro coleta ambos os relatórios:
- Se ambos forem `COMPLETED`: avançar para DoD e marcar a task como `DONE`.
- Se qualquer um for `NEEDS_CHANGES`: emitir `Change Request`, incrementar iteração e devolver para correção (respeitando o limite de 3 rodadas).
