---
name: release-project
description: >-
  Instruções operacionais para encerramento de fases, validação final de release do projeto Agenda de Palestras, verificação dos 10 critérios de sucesso e aprovação humana para publicação.
---

# Skill: Release Project

Esta skill orienta o processo de encerramento de fases e a validação das condições mandatórias para release e publicação do sistema **Agenda de Palestras**.

---

## 1. Critérios Mandatórios de Sucesso para Release

O projeto só pode ser declarado pronto para release quando TODAS as seguintes 10 condições forem formalmente atendidas e comprovadas:

1. [ ] **Fases Concluídas:** Todas as fases obrigatórias (00 a 09) registradas como `DONE` no `.ai/tasks.md`.
2. [ ] **Build de Release Aprovado:** Compilação da solution em modo Release (`dotnet build -c Release`) sem erros e sem warnings bloqueantes.
3. [ ] **Suite de Testes Aprovada:** 100% dos testes unitários, de integração e de arquitetura executados e verdes.
4. [ ] **Revisão Limpa:** Nenhuma vulnerabilidade ou achado de severidade `CRITICAL` ou `HIGH` pendente no relatório do Revisor.
5. [ ] **Migrations Validadas:** Estratégia de migrations do EF Core documentada, com scripts idempotentes e planos de rollback.
6. [ ] **Docker Build Verde:** Imagem de produção compilada com sucesso via Dockerfile multi-stage, executando com usuário não-root.
7. [ ] **Pipeline CI/CD Aprovado:** Pipeline do GitHub Actions testado e verde nas branches `main` e `develop`.
8. [ ] **Configurações e Secrets Seguros:** Todas as variáveis de ambiente e segredos de produção devidamente documentados sem vazamentos.
9. [ ] **Smoke Tests de Homologação Aprovados:** Testes de fumaça executados e validados no ambiente de homologação (staging).
10. [ ] **Aprovação Humana Registrada:** Assinatura explícita do usuário autorizando a publicação em produção.

---

## 2. Procedimento de Fechamento de Fase pelo Maestro

Ao término de todas as tasks de uma fase:

1. **Auditoria de Conclusão:**
   - Conferir se todas as tasks da fase estão marcadas como `DONE` em `.ai/tasks.md`.
   - Verificar se todos os relatórios da fase existem em `docs/agent-runs/<fase>/`.
2. **Elaboração do Relatório de Conclusão da Fase:**
   - Preencher o template em `docs/phases/phase-<XX>-completion.md` com:
     - Tarefas concluídas e evidências.
     - Critérios de saída da fase atendidos.
     - Decisões de arquitetura registradas.
     - Riscos residuais e débitos técnicos mapeados.
     - Parecer oficial do Maestro.
3. **Validação Humana:**
   - Apresentar o relatório ao usuário.
   - Aguardar autorização explícita antes de avançar para a próxima fase no `.ai/workflow-state.md`.

---

## 3. Procedimento de Rollback e Contingência

Antes de qualquer deploy em produção:
1. Validar disponibilidade do backup do banco de dados pré-deploy.
2. Confirmar a tag imutável da imagem Docker anterior.
3. Testar os scripts de rollback das migrations.
4. Documentar o procedimento de reversão no checklist de release em `docs/phases/phase-09-delivery.md`.
