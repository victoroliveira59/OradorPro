# Fase 05 — Agenda de Palestras

> Status: BACKLOG  
> Responsável Principal: Engenheiro  
> Dependência: Fase 04 aprovada

---

## 1. Objetivo
Implementar o núcleo de agendamento de palestras, regras de validação de datas, bloqueio de conflitos de horário e oradores, controle de concorrência e visualização de agenda por calendário e listagem.

---

## 2. Tarefas da Fase

- [ ] **T05.01:** Criar entidade Agenda e relacionamentos no domínio.
- [ ] **T05.02:** Implementar relacionamento com Congregação, Orador e Discurso.
- [ ] **T05.03:** Implementar regra de unicidade da agenda conforme ADR aprovado.
- [ ] **T05.04:** Implementar validação de orador e discurso ativos.
- [ ] **T05.05:** Implementar criação, edição e cancelamento de palestras.
- [ ] **T05.06:** Implementar consulta por período, filtros e visão calendário.
- [ ] **T05.07:** Implementar status e ciclo de vida da confirmação da palestra.
- [ ] **T05.08:** Implementar concorrência e tratamento de conflitos no banco de dados.
- [ ] **T05.09:** Criar e executar testes de regras de agenda e cenários concorrentes.

---

## 3. Critérios de Saída da Fase
1. Agendamento operacional sem duplicidades ou conflitos de orador/data.
2. Controle de concorrência validado no banco de dados.
3. Testes cobrindo cenários negativos e tentativas de agendamento conflitante.
4. Relatórios do Tester e Revisor aprovados.
