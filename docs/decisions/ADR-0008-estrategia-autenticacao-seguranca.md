# ADR-0008 — Estratégia de Autenticação e Segurança MVC

- **Status:** ACCEPTED
- **Date:** 2026-11-01
- **Decision Owner:** Engenheiro / Revisor
- **Technical Context:** Fase 02 - Arquitetura e Design / Fase 07

---

## 1. Context (Contexto e Problema)
A aplicação MVC Razor necessita de autenticação segura, proteção contra CSRF e controle de acesso restrito ao perfil de Administrador.

## 2. Decision (Decisão)
1. **Autenticação por Cookies:** `CookieAuthenticationDefaults.AuthenticationScheme` nativo com `HttpOnly = true`, `SecurePolicy = CookieSecurePolicy.Always` e `SameSite = SameSiteMode.Lax`.
2. **Armazenamento de Senhas:** Hashing seguro usando `PasswordHasher<Usuario>` do ASP.NET Core Identity (PBKDF2 com salt aleatório e HMAC-SHA256).
3. **Proteção Anti-CSRF:** Atributo global `[AutoValidateAntiforgeryToken]` ou `[ValidateAntiForgeryToken]` em todos os formulários Razor de alteração de estado (POST/PUT/DELETE).
4. **Autorização:** Política `AdminOnly` exigindo a claim `Role = Admin`.

## 3. Consequences (Consequências)
- **Positivas:** Padrão recomendado pela Microsoft para aplicações Web MVC server-side, altíssima segurança contra ataques de sessão e XSS.
