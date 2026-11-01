# ADR-0004 — Banco de Dados e Estratégia de Migrations

- **Status:** ACCEPTED
- **Date:** 2026-11-01
- **Decision Owner:** Engenheiro / Arquiteto
- **Technical Context:** Fase 02 - Arquitetura e Design

---

## 1. Context (Contexto e Problema)
O sistema precisa de persistência relacional com suporte a constraints de unicidade, transações ACID e flexibilidade para execução local sem dependências pesadas e produção escalável.

## 2. Options Considered (Alternativas Avaliadas)
- **Opção 1 (PostgreSQL exclusivo):** Excelente ORM support, open source e performático, porém exige daemon Docker ativo para desenvolvimento e testes locais.
- **Opção 2 (SQL Server):** Excelente, porém com consumo elevado de memória em contêiner e licenciamento proprietário.
- **Opção 3 (EF Core com suporte híbrido: SQLite em Desenvolvimento/Testes e PostgreSQL em Produção):** Permite executar localmente e rodar testes de integração instantaneamente com SQLite em arquivo ou memória, mantendo compatibilidade com PostgreSQL para staging/produção via migrations agnósticas do EF Core.

## 3. Decision (Decisão)
Adotar **Entity Framework Core com suporte configurável a PostgreSQL e SQLite**.
- Em Desenvolvimento e Testes de Integração locais: o sistema pode inicializar com SQLite (`AgendaPalestras.db`), permitindo compilação e validação imediatas mesmo se o Docker Desktop não estiver ativo.
- Em Homologação e Produção: o sistema utiliza `Npgsql.EntityFrameworkCore.PostgreSQL`.
- As configurações relacionais são 100% expressas via Fluent API (`IEntityTypeConfiguration<T>`).

## 4. Consequences (Consequências)
- **Positivas:** Execução rápida de testes locais, zero fricção de setup, aderência a padrões abertos.
- **Negativas:** Requer evitar extensões de SQL exclusivas de um único dialeto nas consultas EF Core.
