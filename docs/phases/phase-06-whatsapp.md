# Fase 06 — WhatsApp e Automações

> Status: BACKLOG  
> Responsável Principal: Engenheiro  
> Dependência: Fase 05 aprovada

---

## 1. Objetivo
Implementar a integração com o provedor de WhatsApp aprovado, disparo automático de convite após cadastro de palestra, endpoint seguro para recepção de webhook, correlação idempotente de respostas (1=confirmação, 2=recusa) e job em background para disparo de lembrete preventivo de 7 dias.

---

## 2. Tarefas da Fase

- [ ] **T06.01:** Implementar contrato/abstração da interface do provedor WhatsApp.
- [ ] **T06.02:** Implementar adaptador do provedor de WhatsApp aprovado.
- [ ] **T06.03:** Configurar injeção de credenciais seguras via environment.
- [ ] **T06.04:** Implementar envio automático de convite ao criar palestra.
- [ ] **T06.05:** Implementar persistência e rastreamento de histórico de mensagens.
- [ ] **T06.06:** Implementar endpoint seguro receptor de webhooks.
- [ ] **T06.07:** Validar autenticidade e assinatura do webhook recebido.
- [ ] **T06.08:** Correlacionar resposta do webhook com agenda e telefone do orador.
- [ ] **T06.09:** Processar confirmação e recusa de forma estritamente idempotente.
- [ ] **T06.10:** Implementar background job de lembrete 7 dias antes do evento.
- [ ] **T06.11:** Implementar trava contra duplicidade de lembretes.
- [ ] **T06.12:** Implementar retentativas (retry) e fallback de comunicação.
- [ ] **T06.13:** Criar e executar suite de testes de webhook, idempotência e bordas.

---

## 3. Critérios de Saída da Fase
1. Convites e lembretes enviados com rastreamento completo em banco.
2. Webhook seguro validando assinaturas e sem permitir falsificações.
3. Idempotência comprovada (reprocessamento não duplica mensagens nem estados).
4. Evidências de testes com cenários de falhas de rede e respostas anômalas.
