# Índice de Architecture Decision Records (ADRs)

> Registro central de decisões arquiteturais tomadas ao longo do projeto Agenda de Palestras.
> Todas as decisões abaixo foram formalmente aceitas e orientam a implementação.

---

## Tabela de Decisões

| ID | Título | Status | Data | Responsável | Fase de Origem |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **ADR-0001** | [Template Padrão de ADR](ADR-0001-template.md) | ACCEPTED | 2026-11-01 | Maestro | Fase 00 |
| **ADR-0002** | [Modelo de Administração Centralizada](ADR-0002-modelo-administracao-centralizada.md) | ACCEPTED | 2026-11-01 | Usuário / Arquiteto | Fase 00 |
| **ADR-0003** | [Versão do .NET e Runtime Oficial (.NET 8 LTS)](ADR-0003-versao-dotnet.md) | ACCEPTED | 2026-11-01 | Engenheiro | Fase 02 |
| **ADR-0004** | [Banco de Dados e Migrations (EF Core / SQLite / PostgreSQL)](ADR-0004-banco-de-dados-e-migrations.md) | ACCEPTED | 2026-11-01 | Engenheiro | Fase 02 |
| **ADR-0005** | [Integração de WhatsApp e Webhooks](ADR-0005-integracao-whatsapp-e-webhooks.md) | ACCEPTED | 2026-11-01 | Engenheiro | Fase 02 |
| **ADR-0006** | [Regras de Unicidade e Conflito de Agenda](ADR-0006-regra-unicidade-agenda.md) | ACCEPTED | 2026-11-01 | Analista / Engenheiro | Fase 02 |
| **ADR-0007** | [Processamento de Tarefas Agendadas (PeriodicTimer 7 Dias)](ADR-0007-processamento-tarefas-agendadas.md) | ACCEPTED | 2026-11-01 | Engenheiro | Fase 02 |
| **ADR-0008** | [Estratégia de Autenticação e Segurança MVC](ADR-0008-estrategia-autenticacao-seguranca.md) | ACCEPTED | 2026-11-01 | Engenheiro / Revisor | Fase 02 |

---

## Diretrizes de Uso
1. Toda alteração de escopo estrutural deve consultar estes registros antes da codificação.
2. A reversão ou alteração de qualquer decisão exige novo ADR que declare `SUPERSEDES ADR-XXXX`.
