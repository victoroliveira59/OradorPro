# ADR-0010 — Visão de Calendário Mensal e Bloqueio de Agendamento por Dia

- **Status:** ACCEPTED
- **Date:** 2026-11-01
- **Decision Owner:** Analista / Engenheiro / Maestro
- **Technical Context:** Fase 11 — Calendário Mensal e Bloqueio Diário de Agendamento

---

## 1. Contexto e Problema
A visualização tabular simples de agendamentos dificulta que o coordenador de palestras enxergue o panorama geral do mês, identifique finais de semana vagos e acompanhe rapidamente os oradores e temas programados para cada dia.
Além disso, no modelo de reuniões congregacionais, cada congregação realiza apenas uma palestra pública por data de reunião (geralmente domingo ou final de semana). O sistema permitia agendamentos em horários diferentes no mesmo dia se os minutos fossem distintos, o que não reflete a regra de negócio real das congregações.

## 2. Decisão
1. **Visão em Grade de Calendário Mensal (UI):**
   - Disponibilizar na tela de agendamentos (`Agenda/Index`) uma visualização em grade de calendário mensal completa (dias da semana de Domingo a Sábado).
   - Permitir navegar entre meses anteriores/posteriores, botão "Hoje" e seleção direta de Mês e Ano.
   - Permitir alternância limpa entre a visualização de **Grade de Calendário** e a visualização em **Tabela**.
   - Cada célula do dia exibe os discursos com tema completo, número do discurso, nome do orador, congregação, horário e badge com status visual da palestra (Confirmado, Pendente, Recusado, Cancelado).
   - Dias vagos possuem atalho rápido de agendamento pré-selecionando aquela data.

2. **Bloqueio Estrito de Múltiplos Agendamentos no Mesmo Dia (Backend):**
   - Na criação e reagendamento de palestras, validar se a congregação hospedeira já possui **qualquer palestra ativa** (`Status != Cancelado` e `Status != Recusado`) marcada para a **mesma data civil (dia)**, independentemente da hora informada.
   - Validar também se o orador convidado já possui palestra agendada em qualquer congregação na mesma data civil.
   - Caso haja conflito, a camada de aplicação lança `DomainException` impedindo a criação/atualização.

## 3. Consequências
- **Positivas:**
  - Visão executiva clara e imediata de toda a escala do mês.
  - Elimina risco de agendar dois oradores no mesmo dia para uma mesma congregação.
  - Previne agendar o mesmo orador em congregações diferentes no mesmo dia.
- **Negativas:**
  - Caso extraordinário de evento com duas palestras no mesmo dia na mesma congregação exigiria cancelamento ou suporte manual se não previsto.
