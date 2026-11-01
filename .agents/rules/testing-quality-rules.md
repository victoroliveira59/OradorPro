---
trigger: always_on
description: Diretrizes de testes, garantia de qualidade, evidências obrigatórias e critérios de aceitação.
---

# Regras de Qualidade e Validação de Testes

## 1. Princípio da Evidência Real

1. **Evidência Obrigatória:** Nenhuma tarefa pode ser marcada como `DONE` sem evidência real. O relatório do Tester deve conter:
   - Linha de comando exata executada (ex: `dotnet test tests/... --logger "console;verbosity=normal"`).
   - Saída resumida contendo total de testes passados, com falha e ignorados.
   - Status dos testes: 100% dos testes da task devem passar.
2. **Proibição de Falsos Positivos:** Testes comentados, asserções vazias ou testes desabilitados com `[Ignore]` para contornar falhas são estritamente proibidos e serão reprovados na revisão.
3. **Reproducibilidade de Defeitos:** Ao reportar um defeito, o Tester deve fornecer o cenário de reprodução detalhado, entradas usadas e comportamento esperado versus obtido.

## 2. Escopo dos Testes

1. **Testes Unitários:** Devem cobrir regras de domínio, validações de modelo, regras de negócio isoladas e manipuladores de aplicação sem depender de banco de dados ou rede.
2. **Testes de Integração:** Devem validar persistência com EF Core (usando container de banco ou provider em memória devidamente justificado), transações e endpoints de controller/webhook.
3. **Cenários Negativos e Bordas:** Testes devem obrigatoriamente validar casos de erro (ex: orador inativo, congregação duplicada, resposta inválida no WhatsApp, choque de data).
4. **Idempotência:** Validações de webhook e jobs de lembrete devem comprovar que reprocessamento não gera duplicidade de registros ou de envios.
