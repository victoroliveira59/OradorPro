# Workflow State — Agenda de Palestras

- **Project:** Agenda de Palestras
- **Current Phase:** 10 - Conexão WhatsApp Admin (Evolution API e QR Code)
- **Current Task:** T10.05 - Integrar link no menu de navegação e testes automatizados da conexão
- **Workflow Status:** DONE
- **Active Agent:** MAESTRO
- **Correction Iteration:** 0/3
- **Last Completed Task:** T10.05 - Conexão WhatsApp Admin e QR Code
- **Blockers:** none
- **Last Updated:** 2026-11-01 12:26 BRT

---

## Current Objective
Fase 10 (Conexão WhatsApp Admin com Evolution API v2 e QR Code) desenvolvida, integrada com Clean Architecture, com migration executada no PostgreSQL, View Razor moderna com QR Code e Pairing Code, e 48 testes unitários passando.

---

## Next Action
Orientar o usuário a reiniciar a aplicação no seu ambiente para carregar as novas rotas e a interface de conexão do WhatsApp.

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
