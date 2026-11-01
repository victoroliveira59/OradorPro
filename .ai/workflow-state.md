# Workflow State — Agenda de Palestras

- **Project:** Agenda de Palestras
- **Current Phase:** 14 - Multi-Instâncias da Evolution API por Usuário
- **Current Task:** T14.01 - Isolamento multi-tenant de instâncias da Evolution API, gestão pelo SuperAdmin e liberação condicional para usuários
- **Workflow Status:** DONE
- **Active Agent:** MAESTRO
- **Correction Iteration:** 0/3
- **Last Completed Task:** T14.01 - Isolamento multi-tenant de instâncias da Evolution API, gestão pelo SuperAdmin e liberação condicional para usuários
- **Blockers:** none
- **Last Updated:** 2026-11-01 20:55 BRT

---

## Current Objective
Permitir que múltiplos coordenadores utilizem suas próprias sessões do WhatsApp sem acesso à Evolution API nem à ApiKey global. O Super Admin cria e vincula a instância para cada usuário, e a tela de conexão com QR Code passa a ser liberada para o respectivo usuário.

---

## Next Action
Aguardando novas solicitações do usuário ou implantação em produção.

---

## Approved Decisions
- [ADR-0012](file:///c:/git/OradorPro/docs/decisions/ADR-0012-sessoes-evolution-api-por-usuario.md): Isolamento multi-tenant das instâncias da Evolution API por usuário com gestão centralizada pelo Super Admin.
- [ADR-0011](file:///c:/git/OradorPro/docs/decisions/ADR-0011-programacao-reunioes-mensal-pdf.md): Programação mensal de reuniões de fim de semana com exportação em PDF.
- [ADR-0010](file:///c:/git/OradorPro/docs/decisions/ADR-0010-calendario-mensal-interativo.md): Calendário interativo mensal com controle de unicidade por dia.

---

## Evidence
- `dotnet test AgendaPalestras.sln -c Release`: 97 testes executados (89 unitários, 4 de arquitetura, 4 de integração), 97 aprovados (100% sucesso).
- Migration `20261101235059_AddEvolutionCamposUsuario` gerada com sucesso.
- Interface `IEvolutionApiService` e serviço `EvolutionApiService` com suporte a instâncias por usuário e criação remota de sessões.
- Controller `WhatsAppController` e View `WhatsApp/Index.cshtml` com controle de privilégios e exibição condicional.
