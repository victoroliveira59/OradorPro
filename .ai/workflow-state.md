# Workflow State — Agenda de Palestras

- **Project:** Agenda de Palestras
- **Current Phase:** 11 - Calendário Mensal e Bloqueio Diário de Agendamento
- **Current Task:** T11.05 - Executar testes automatizados, coletar evidências e relatório de conclusão
- **Workflow Status:** DONE
- **Active Agent:** MAESTRO
- **Correction Iteration:** 0/3
- **Last Completed Task:** T11.05 - Calendário Mensal e Bloqueio Diário de Agendamento
- **Blockers:** none
- **Last Updated:** 2026-11-01 15:01 BRT

---

## Current Objective
Fase 11 (Calendário Mensal e Bloqueio Diário de Agendamento) concluída com sucesso: visualização em grade de calendário com discursos completos, navegação rápida por meses/anos, botão Hoje, alternância Calendário/Lista, atalho para dias vagos e bloqueio de múltiplos agendamentos no mesmo dia tanto na UI quanto no backend.

---

## Next Action
Apresentar a nova funcionalidade ao usuário e orientar sobre a navegação e regras de agendamento por dia.

---

## Approved Decisions
- Adoção do modelo de 5 agentes especializados: Maestro, Analista, Engenheiro, Tester, Revisor.
- Adoção de Agent Skills em substituição aos workflows legados.
- Limite rígido de 3 rodadas de correção por task com bloqueio para intervenção humana.
- Arquitetura limpa em camadas para a solução .NET (Domain, Application, Infrastructure, Web).
- **Administração Centralizada (ADR-0002):** Um único usuário Admin cria e gerencia múltiplas congregações no banco de dados e cadastra os oradores vinculados a elas.
- **Repertório do Orador (N:N):** Um orador pode ter múltiplos discursos associados ao seu repertório (`ORADOR_DISCURSO`), permitindo selecionar na agenda apenas discursos que ele está apto a proferir.
- **Prevenção de Conflitos e Unicidade (ADR-0006):** Bloqueio estrito de choque de datas e horários tanto para o orador quanto para a congregação.
- **Automação WhatsApp e Idempotência (ADR-0005):** Notificação de convite, webhook com deduplicação de mensagens e background job diário para lembrete de 7 dias.
- **Segurança por Padrão (ADR-0008):** Cookie Authentication seguro, Anti-CSRF global em verbos POST, senha com PBKDF2/SHA256 e cabeçalhos de segurança HTTP.

---

## Pending Decisions
Nenhuma pendência técnica ou de produto em aberto. Todas as decisões foram formalizadas em ADRs e aprovadas.

---

## Evidence
- 10 fases com relatórios de conclusão em `docs/phases/phase-XX-completion.md`.
- `dotnet test AgendaPalestras.sln`: 52 testes executados (43 unitários, 4 de arquitetura, 5 de integração), 52 aprovados, 0 falhas.
- `dotnet build -c Release AgendaPalestras.sln`: 0 erros, 0 warnings.
- `Dockerfile` multi-stage com usuário `$APP_UID` e porta 8080.
- `docker-compose.yml` para orquestração de homologação com PostgreSQL 16.
- Pipelines de CI e CD ativas em `.github/workflows/`.
- Relatório consolidado de release em `docs/phases/project-release-report.md`.
