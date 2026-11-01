# Arquitetura de Segurança e Privacidade — Agenda de Palestras

> Alinhamento com SPEC.md Seção 2, Seção 4.5 e Fase 07.

---

## 1. Princípios de Segurança por Design

1. **Defesa em Profundidade:** Validação e sanitização de dados no frontend (experiência do usuário) e obrigatoriamente no backend (segurança real).
2. **Menor Privilégio:** Usuários vinculados a congregações só acessam dados de sua própria congregação, exceto perfis com a claim `GlobalAdmin`.
3. **Proteção de Segredos:** Proibição de armazenamento de segredos no código-fonte ou no Git.

---

## 2. Controles de Autenticação e Sessão

- **Algoritmo de Hash de Senha:** PBKDF2 ou Argon2id através do ASP.NET Core Identity (`PasswordHasher<T>`).
- **Cookies de Autenticação Seguros:**
  - `HttpOnly = true` (impede leitura via JavaScript / XSS).
  - `SecurePolicy = CookieSecurePolicy.Always` (transmitido exclusivamente via HTTPS).
  - `SameSite = SameSiteMode.Lax` ou `Strict` (proteção contra ataques CSRF).
  - `ExpireTimeSpan` configurado com renovação deslizante (sliding expiration).

---

## 3. Mitigações contra OWASP Top 10

| Vetor de Risco | Mitigação Implementada |
| :--- | :--- |
| **SQL Injection** | Parametrização universal via Entity Framework Core. Proibido SQL concatenado. |
| **Broken Access Control** | Políticas de autorização declarativas (`[Authorize(Policy = "...")]`) nos controllers. |
| **Cross-Site Request Forgery (CSRF)** | `[ValidateAntiForgeryToken]` aplicado globalmente em todos os requests de modificação. |
| **Cross-Site Scripting (XSS)** | Encodificação automática em Razor Views (`@Model.Texto`) e headers `Content-Security-Policy`. |
| **Security Misconfiguration** | Headers `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Strict-Transport-Security`. |
| **Vazamento de Segredos** | Secrets injetados por variáveis de ambiente ou montagem de volumes Docker no deploy. |
