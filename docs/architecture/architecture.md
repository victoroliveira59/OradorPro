# Arquitetura da Solução — Agenda de Palestras

> Versão: 1.0  
> Status: Fase 00 (Estrutura Inicial)  
> Alinhamento: Clean Architecture MVC em .NET

---

## 1. Visão Arquitetural

A solução adota os preceitos de Clean Architecture em conjunto com ASP.NET Core MVC (Razor Views), promovendo baixo acoplamento, alta coesão e independência de frameworks externos no núcleo de regras de negócio.

```text
+-------------------------------------------------------------+
|                  AgendaPalestras.Web                        |
|       (Controllers MVC, Razor Views, ViewModels, DI)        |
+------------------------------+------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|               AgendaPalestras.Application                   |
|     (Casos de Uso, DTOs, Validadores FluentValidation)      |
+------------------------------+------------------------------+
                               |
                               v
+-------------------------------------------------------------+
|                 AgendaPalestras.Domain                      |
|       (Entidades Ricas, Enums, Regras, Interfaces)          |
+-------------------------------------------------------------+
                               ^
                               |
+------------------------------+------------------------------+
|              AgendaPalestras.Infrastructure                 |
|    (EF Core DbContext, Mapeamentos Fluent API, Migrations,  |
|       Adaptadores de WhatsApp, Background Jobs)             |
+-------------------------------------------------------------+
```

---

## 2. Descrição das Camadas

### 2.1 AgendaPalestras.Domain
- Contém entidades, objetos de valor (Value Objects), enums e exceções de domínio.
- Define as interfaces de repositórios e serviços de domínio (`IRepository<T>`, `IAgendaRepository`, `IWhatsAppNotificationService`).
- Totalmente agnóstico de infraestrutura, banco de dados ou bibliotecas externas.

### 2.2 AgendaPalestras.Application
- Orquestra os fluxos da aplicação (casos de uso).
- Contém DTOs imutáveis (`records`) para entrada e saída de dados.
- Implementa validações de entrada com FluentValidation.
- Converte DTOs para modelos de domínio e vice-versa sem expor entidades do banco.

### 2.3 AgendaPalestras.Infrastructure
- Implementa o acesso a dados via Entity Framework Core (`ApplicationDbContext`).
- Configura mapeamentos relacionais via classes separadas com `IEntityTypeConfiguration<T>`.
- Gerencia migrations e transações de banco de dados.
- Implementa a integração externa de mensageria WhatsApp via `HttpClient` resiliente.
- Hospeda os background services e jobs periódicos para avaliação de lembretes (7 dias).

### 2.4 AgendaPalestras.Web
- Ponto de entrada da aplicação ASP.NET Core MVC.
- Controllers magros responsáveis apenas por deserialização, autorização e encaminhamento para a camada de Application.
- Razor Views utilizando Bootstrap e ViewModels tipadas.
- Configuração de Injeção de Dependências, autenticação por cookies seguros e middlewares de tratamento de erros e segurança.
