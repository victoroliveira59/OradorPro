# ADR-0006 — Regras de Unicidade e Prevenção de Conflitos na Agenda

- **Status:** ACCEPTED
- **Date:** 2026-11-01
- **Decision Owner:** Engenheiro / Analista
- **Technical Context:** Fase 02 - Arquitetura e Design / Fase 05

---

## 1. Context (Contexto e Problema)
O sistema precisa garantir que não ocorram choques de datas na agenda de palestras, tanto do ponto de vista do orador quanto da congregação hospedeira.

## 2. Decision (Decisão)
Adotar **Dupla Proteção de Unicidade**:
1. **Regra de Unicidade do Orador:** Um orador não pode ter mais de uma palestra agendada no mesmo horário e data em congregações distintas.
2. **Regra de Unicidade da Congregação:** Uma congregação não pode ter duas palestras agendadas simultaneamente na mesma data e horário.
3. **Mecanismo de Aplicação:**
   - Validação prévia na camada de Application com mensagens claras de erro.
   - Constraint única composta no banco de dados para garantia de integridade contra concorrência: `UQ_Agenda_Orador_DataHora` e `UQ_Agenda_Congregacao_DataHora`.

## 3. Consequences (Consequências)
- **Positivas:** Impossibilidade matemática de conflitos de agendamento no banco de dados.
- **Negativas:** Exige captura e tratamento de `DbUpdateException` na interface do usuário.
