---
trigger: always_on
description: Diretrizes mandatórias de segurança, proteção de segredos, autorização e tratamento de dados sensíveis.
---

# Regras de Segurança e Proteção de Dados

## 1. Gestão de Segredos e Configurações

1. **Proibição Absoluta de Segredos no Repositório:** Nenhuma chave de API, senha de banco de dados, token JWT, secret do WhatsApp ou certificado pode ser armazenado no código ou versionado no Git.
2. **Uso de Variáveis de Ambiente:** Toda configuração sensível deve ser lida a partir de variáveis de ambiente (`EnvironmentVariables`) ou User Secrets (.NET local) / GitHub Secrets (CI/CD).
3. **Exemplos Seguros:** Arquivos de exemplo (como `.env.example` ou `appsettings.Development.json`) devem conter apenas placeholders fictícios como `CHAVE_AQUI`.

## 2. Autenticação e Autorização

1. **Validação no Servidor:** Validações de permissão e autenticação devem ser executadas no backend. Validações na interface são apenas para experiência de usuário.
2. **Princípio do Menor Privilégio:** Usuários só podem acessar recursos da sua respectiva congregação, a menos que possuam permissão explícita de administração global.
3. **Proteção de Cookies e Sessão:** Cookies de autenticação devem ter flags `HttpOnly`, `Secure` e `SameSite=Strict` ou `Lax` devidamente configuradas.
4. **Proteção Antifalsificação (Anti-CSRF):** Todos os formulários Razor POST/PUT/DELETE devem utilizar tokens antifalsificação (`[ValidateAntiForgeryToken]`).

## 3. Prevenção de Vulnerabilidades OWASP

1. **Injeção de Código e SQL:** Utilizar exclusivamente consultas parametrizadas do EF Core ou Dapper. Proibida concatenação de strings em queries SQL.
2. **XSS (Cross-Site Scripting):** Garantir que dados renderizados nas Razor Views sejam sanitizados ou codificados automaticamente pelo Razor (`@Model.Property`).
3. **Validação e Autenticidade de Webhooks:** Endpoints receptores de webhooks (WhatsApp) devem validar assinaturas ou segredos de cabeçalho para impedir falsificação de mensagens.
4. **Logs Seguros:** Proibido registrar senhas, tokens de sessão, números de telefone desnecessários ou payloads completos de autenticação nos logs da aplicação.
