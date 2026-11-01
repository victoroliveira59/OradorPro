# ADR-0007 — Processamento de Tarefas Agendadas (Lembretes)

- **Status:** ACCEPTED
- **Date:** 2026-11-01
- **Decision Owner:** Engenheiro / Arquiteto
- **Technical Context:** Fase 02 - Arquitetura e Design / Fase 06

---

## 1. Context (Contexto e Problema)
O sistema precisa avaliar e enviar lembretes automáticos via WhatsApp exatamente 7 dias antes de cada palestra, sem gerar dependências pesadas de mensageria externa nesta fase inicial.

## 2. Decision (Decisão)
Adotar o `BackgroundService` nativo do .NET com `PeriodicTimer` configurado para executar diariamente.
- O job consulta palestras cuja `DataHora` ocorra entre 7 e 8 dias à frente e cujo `DataLembreteEnviado` seja nulo.
- Ao enviar o lembrete, a coluna `DataLembreteEnviado` é preenchida na mesma transação, assegurando que novas execuções não reenviem a mensagem.

## 3. Consequences (Consequências)
- **Positivas:** Zero dependências externas de bibliotecas terceiras (como Hangfire ou Quartz), execução integrada no ciclo de vida da aplicação.
- **Negativas:** Em cenários multi-instância futuros, exigirá trava distribuída ou job runner isolado.
