# Critérios de Aceite Globais — Agenda de Palestras

> Padrão para verificação empírica de tarefas pelo Tester e Revisor.

---

## 1. Padrão de Formatação dos Critérios de Aceite
Todos os critérios de aceite das tarefas das fases de desenvolvimento devem seguir o padrão Given-When-Then (Dado-Quando-Então) ou checklist verificável:

```markdown
- [ ] **AC-XX.01:** Dado que [condição inicial], Quando [ação do usuário/sistema], Então [resultado mensurável esperado].
```

---

## 2. Critérios de Aceite Transversais

### 2.1 Cadastros Básicos (Congregações, Oradores, Discursos)
- [ ] **CA-CAD-01:** Dado um orador com status inativo, Quando o usuário tentar selecioná-lo para uma nova palestra, Então o sistema deve bloquear a operação e exibir mensagem de erro amigável.
- [ ] **CA-CAD-02:** Dado um discurso inativo, Quando o usuário tentar associá-lo a uma nova palestra, Então o sistema deve impedir a gravação e indicar a restrição.
- [ ] **CA-CAD-03:** Dado um orador cadastrado com palestras no histórico, Quando o usuário solicitar exclusão do orador, Então o sistema deve impedir a exclusão física e recomendar inativação lógica.

### 2.2 Agendamento e Conflitos
- [ ] **CA-AGE-01:** Dado que um orador já possui palestra agendada para a Data X e Horário Y, Quando for submetido um novo agendamento para o mesmo orador na mesma data/horário, Então o sistema deve rejeitar a criação com erro de conflito.
- [ ] **CA-AGE-02:** Dado o salvamento concorrente de dois agendamentos conflitantes no mesmo milissegundo, Então a constraint única no banco de dados deve rejeitar o segundo agendamento e manter a consistência.

### 2.3 WhatsApp e Webhooks
- [ ] **CA-WPP-01:** Dado o cadastro de uma nova palestra com dados válidos, Quando a transação for concluída no banco, Então o sistema deve registrar a mensagem de convite no histórico com status PENDING e efetuar o envio via API.
- [ ] **CA-WPP-02:** Dado o recebimento de webhook com resposta "1" originado do telefone do orador da palestra, Quando processado o evento, Então a palestra deve transicionar para status CONFIRMED e registrar o evento no histórico.
- [ ] **CA-WPP-03:** Dado o recebimento repetido do mesmo payload de webhook (duplicação de rede), Quando processado pela segunda vez, Então o sistema deve reconhecer a idempotência e não alterar o estado nem enviar mensagens duplicadas.
- [ ] **CA-WPP-04:** Dado o job diário de lembretes, Quando avaliar uma palestra que ocorre exatamente em 7 dias e cujo lembrete ainda não foi enviado, Então o lembrete deve ser enviado e marcado como SENT, impedindo reenvio posterior.
