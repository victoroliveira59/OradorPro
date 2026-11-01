# Fase 03 — Fundação Técnica e DevOps

> Status: BACKLOG  
> Responsável Principal: Engenheiro  
> Dependência: Fase 02 aprovada

---

## 1. Objetivo
Criar a solution .NET e projetos com estrutura compilável, configurar referências de Clean Architecture, Entity Framework Core, ambiente local com Docker Compose, Dockerfile multi-stage e pipeline básico de CI com GitHub Actions.

---

## 2. Tarefas da Fase

- [ ] **T03.01:** Criar solution e projetos nas pastas `src/` e `tests/`.
- [ ] **T03.02:** Configurar referências e isolamento entre camadas.
- [ ] **T03.03:** Configurar Nullable reference types, analyzers e convenções em `Directory.Build.props`.
- [ ] **T03.04:** Configurar DbContext e Entity Framework Core.
- [ ] **T03.05:** Configurar banco de dados local via Docker Compose e `.env.example`.
- [ ] **T03.06:** Criar Dockerfile multi-stage seguro (usuário não-root).
- [ ] **T03.07:** Criar `.dockerignore` otimizado.
- [ ] **T03.08:** Criar endpoint de health check (`/healthz`).
- [ ] **T03.09:** Criar pipeline GitHub Actions CI básico (`.github/workflows/ci.yml`).
- [ ] **T03.10:** Executar compilação, testes e build Docker local com sucesso comprovado.

---

## 3. Critérios de Saída da Fase
1. Solution compila sem erros em modo Release (`dotnet build -c Release`).
2. Contêiner do banco e aplicação sobem via `docker compose up`.
3. Pipeline CI do GitHub Actions testado e verde.
4. Relatório do Tester comprovando execução e evidências reais.
