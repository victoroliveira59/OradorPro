# ADR-0002 — Modelo de Administração Centralizada de Congregações e Oradores

- **Status:** ACCEPTED
- **Date:** 2026-11-01
- **Decision Owner:** Usuário / Arquiteto
- **Technical Context:** Fase 00 - Preparação do Harness / Modelo de Dados

---

## 1. Context (Contexto e Problema)
Havia indefinição sobre o modelo de administração do sistema (Single-tenant vs Multi-tenant colaborativo entre congregações). Era necessário definir se cada congregação possuiria seus próprios usuários isolados ou se a administração seria centralizada em um usuário administrador que gerencia múltiplas congregações parceiras e cadastra os oradores vinculados a elas.

## 2. Options Considered (Alternativas Avaliadas)
- **Opção 1 (Multitenant com isolamento por congregação):** Cada congregação possui seu próprio usuário administrador, sem acesso aos dados das demais. (Rejeitada pelo usuário).
- **Opção 2 (Administração Centralizada por Usuário Admin):** Um usuário administrador possui visão global, cria e administra múltiplas congregações no banco de dados e registra os oradores pertencentes a cada congregação. (Aprovada pelo usuário).

## 3. Decision (Decisão)
Adotar a **Administração Centralizada**. Na estrutura do banco de dados:
1. A entidade `USUARIO` (Admin) não possui vínculo fixo com uma única congregação (`congregacao_id` não existe na tabela `USUARIO`).
2. O usuário admin cria múltiplas congregações (`CONGREGACAO` possui `usuario_criador_id FK`).
3. Sob cada congregação criada, o admin cadastra os respectivos oradores (`ORADOR` possui `congregacao_id FK`).

## 4. Consequences (Consequências)
- **Positivas:** Modelo relacional simplificado; eliminação de complexidade desnecessária de isolamento multitenant no EF Core; cadastro e organização ágil de congregações e oradores para agendamentos.
- **Negativas:** Exige que a interface web forneça filtros eficientes por congregação, visto que todas as congregações e oradores residem no mesmo contexto administrativo.

## 5. Security and Operational Impact (Impacto em Segurança e Operação)
- Acesso restrito ao usuário administrador autenticado.
- Simplifica as policies de autorização do ASP.NET Core (`[Authorize(Roles = "Admin")]`).
