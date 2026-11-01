# ADR-0005 — Integração de WhatsApp e Webhooks

- **Status:** ACCEPTED
- **Date:** 2026-11-01
- **Decision Owner:** Engenheiro / Arquiteto
- **Technical Context:** Fase 02 - Arquitetura e Design / Fase 06

---

## 1. Context (Contexto e Problema)
A aplicação necessita enviar convites automáticos e lembretes aos oradores via WhatsApp e receber suas confirmações ou recusas via webhook. É fundamental não acoplar o domínio a um provedor específico.

## 2. Options Considered (Alternativas Avaliadas)
- **Opção 1 (Acoplamento direto a Evolution API ou Cloud API):** Menos camadas, mas gera dependência direta e dificulta testes automatizados.
- **Opção 2 (Portas e Adaptadores com interface agnóstica):** Interface `IWhatsAppNotificationService` na camada de Aplicação com adaptadores na Infraestrutura (Evolution API, Mock para testes e Meta Cloud API).

## 3. Decision (Decisão)
Adotar a **Arquitetura de Portas e Adaptadores (Clean Architecture)**:
1. Contrato `IWhatsAppNotificationService` no núcleo da aplicação.
2. Adaptador concreto `EvolutionApiNotificationService` configurável via variáveis de ambiente.
3. Adaptador de testes `MockWhatsAppNotificationService` para validação de testes sem rede.
4. Endpoint de Webhook `/api/webhooks/whatsapp` protegido por validação de secret no cabeçalho HTTP (`X-Webhook-Secret`).
5. Processamento idempotente: toda mensagem recebida tem seu payload e messageId validados contra a tabela `MensagemHistorico` para impedir reprocessamentos.

## 4. Consequences (Consequências)
- **Positivas:** Fácil troca de provedor, testes unitários e de integração confiáveis e seguros, resiliência contra repetição de mensagens.
- **Negativas:** Exige criação e manutenção de contratos de abstração.
