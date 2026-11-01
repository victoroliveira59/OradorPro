---
trigger: always_on
description: Regras de arquitetura limpa, separação de camadas e princípios de design para o projeto Agenda de Palestras.
---

# Regras de Arquitetura e Design

## 1. Direção das Dependências

A aplicação segue os princípios da Clean Architecture. As dependências devem apontar exclusivamente para o núcleo do domínio:

```text
AgendaPalestras.Domain
        ↑
AgendaPalestras.Application
        ↑
AgendaPalestras.Infrastructure

AgendaPalestras.Web -> Application
AgendaPalestras.Web -> Infrastructure (apenas para Composição / Injeção de Dependência no Startup)
```

- **Domain:** Entidades puras, Enums, Interfaces de repositório, Exceções de domínio, Regras centrais. Nenhuma dependência externa ou de infraestrutura (nem mesmo Entity Framework Core).
- **Application:** Casos de uso, DTOs (records preferencialmente), mapeamentos estáticos ou explícitos, validadores (FluentValidation), interfaces de serviços externos.
- **Infrastructure:** Implementação de EF Core (DbContext, Configurações Fluent API, Migrations), adaptadores de WhatsApp (Evolution API / Cloud API), Jobs agendados, clientes HTTP.
- **Web:** ASP.NET Core MVC, Razor Views, Controllers enxutos, ViewModels, autenticação por Cookies/JWT, autorização por Policies.

## 2. Princípios de Implementação

1. **Controllers Finos:** Controllers apenas recebem requisições, delegam para serviços de aplicação e retornam Views/ViewModels ou resultados adequados. Nenhuma lógica de negócio em controllers.
2. **DTOs e ViewModels Obrigatórios:** Entidades de domínio nunca devem ser expostas diretamente para Views ou endpoints HTTP.
3. **Mapeamentos Explícitos:** Mapeamentos devem ser realizados via métodos estáticos ou construtores de DTO/ViewModel. Evitar bibliotecas mágicas de mapeamento por reflexão implícita sem controle.
4. **Respeito aos ADRs:** Qualquer decisão estrutural deve consultar e respeitar os ADRs aprovados em `docs/decisions/`.
