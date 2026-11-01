---
trigger: model_decision
description: Convenções de código C# e boas práticas para desenvolvimento da plataforma .NET no Agenda de Palestras.
---

# Convenções de Código e Boas Práticas .NET

## 1. Convenções de C#

1. **Async / Await e Cancelamento:**
   - Todo método com operação de I/O (banco de dados, chamadas HTTP, sistema de arquivos) deve ser assíncrono e retornar `Task` ou `Task<T>`.
   - Propagar `CancellationToken cancellationToken = default` em todos os métodos assíncronos das camadas de Aplicação e Infraestrutura.
2. **Nullable Reference Types:**
   - Habilitar `<Nullable>enable</Nullable>` em todos os projetos.
   - Tratar referências nulas explicitamente, sem silenciar avisos com operador `!` sem justificativa documentada.
3. **Imutabilidade e Tipos de Dados:**
   - Utilizar `record` para DTOs, mensagens de eventos e ViewModels quando apropriado.
   - Propriedades de entidades devem possuir encapsulamento (`private init;` ou setters privados quando mutáveis via métodos de domínio).
4. **Validações:**
   - Regras de formato e presença em DTOs devem utilizar FluentValidation.
   - Invariantes de negócio de domínio devem ser validadas nas próprias entidades ou serviços de domínio lançando exceções especializadas.

## 2. Padrões de Entity Framework Core

1. **Configurações Fluent API:**
   - Toda configuração de entidade (tabelas, colunas, índices, constraints) deve residir em classes isoladas implementando `IEntityTypeConfiguration<T>` na camada de Infraestrutura.
   - Nenhuma anotação de dados de infraestrutura (como `[Table]`, `[Column]`) nas entidades de Domínio.
2. **Exclusões e Restrições Estrangeiras:**
   - Definir explicitamente o comportamento de deleção de chaves estrangeiras (`DeleteBehavior.Restrict`), evitando deleções em cascata acidentais em cadastros fundamentais (ex: Congregação → Orador).
3. **Migrations:**
   - Migrations devem possuir nomes descritivos em PascalCase e serem testadas antes de aprovação.
