# AI Agent Harness --- Agenda de Palestras

> Workflow operacional de desenvolvimento assistido por IA e
> multiagentes\
> Projeto: Agenda de Palestras\
> Versão: 1.0\
> Status inicial: Planejamento\
> Idioma: Português (Brasil)

------------------------------------------------------------------------

## 1. Objetivo

Este documento define o processo completo para desenvolver o sistema
**Agenda de Palestras** utilizando agentes de IA, execução orientada a
fases e tasks, validação contínua, Docker e CI/CD com GitHub Actions.

O Harness não é apenas uma coleção de prompts. É o conjunto de regras,
contratos, estados, evidências e critérios que controla:

-   qual agente pode atuar;
-   quando uma tarefa pode começar;
-   quais entradas o agente deve consultar;
-   quais arquivos e evidências deve produzir;
-   como os resultados são validados;
-   como tratar falhas e solicitar correções;
-   quando uma task, fase ou execução pode ser encerrada.

O Maestro é o único agente autorizado a avançar o workflow, delegar
tarefas e declarar fases concluídas.

## 2. Princípios obrigatórios

1.  **Uma task ativa por vez**, salvo autorização expressa do Maestro
    para tarefas independentes.
2.  **Execução sequencial por padrão:** Analista → Engenheiro → Tester →
    Revisor → Maestro.
3.  **Sem implementação sem especificação:** requisitos, regras de
    negócio e critérios de aceite devem existir antes da codificação.
4.  **Sem conclusão sem evidência:** build, testes, revisão e
    documentação devem ser registrados.
5.  **Mudanças rastreáveis:** toda alteração deve estar vinculada a uma
    fase e task.
6.  **Escopo controlado:** agentes não podem ampliar requisitos por
    iniciativa própria.
7.  **Decisões registradas:** decisões técnicas relevantes devem gerar
    ADR (Architecture Decision Record).
8.  **Segurança desde o início:** segredos nunca devem ser gravados no
    repositório; autenticação, autorização, validação e proteção de
    dados são requisitos transversais.
9.  **Falha não é sucesso:** teste não executado, integração
    indisponível ou evidência ausente não pode ser reportada como
    aprovada.
10. **Humano no controle:** decisões de produto, credenciais, publicação
    em produção, custos e ações irreversíveis exigem aprovação humana.

## 3. Contexto do produto

O Agenda de Palestras será uma aplicação web para administrar
congregações, oradores, discursos e uma agenda de palestras.

### 3.1 Escopo funcional inicial

-   Autenticação e autorização de usuários.
-   Cadastro e manutenção de congregações.
-   Cadastro e manutenção de oradores.
-   Cadastro e manutenção de discursos.
-   Agenda de palestras.
-   Envio de convite ao orador via WhatsApp após o cadastro da palestra.
-   Recebimento da resposta do orador por webhook.
-   Interpretação de confirmação ou recusa.
-   Lembrete automático sete dias antes da palestra.
-   Histórico de mensagens e eventos da agenda.
-   Dashboard com próximas palestras e pendências.

### 3.2 Regras de negócio já identificadas

-   Uma congregação pode possuir vários oradores.
-   Cada orador pertence a somente uma congregação.
-   Um orador inativo não pode ser escolhido em uma nova agenda.
-   Um discurso inativo não pode ser escolhido em uma nova agenda.
-   O cadastro da agenda deve associar data, congregação, orador e
    discurso.
-   Deve existir uma regra de unicidade para impedir conflito de agenda.
-   Ao concluir o cadastro, deve ser enviado um convite ao orador.
-   A resposta `1` representa confirmação e a resposta `2` representa
    recusa, sujeitas à validação de identidade e contexto da conversa.
-   Sete dias antes da palestra deve ser avaliado o envio do lembrete.
-   O sistema deve impedir o envio duplicado de convites e lembretes.
-   As mensagens enviadas e recebidas devem possuir histórico.

### 3.3 Decisões de produto pendentes

O Analista deve levantar e registrar estas decisões antes da
implementação das funcionalidades afetadas:

  -----------------------------------------------------------------------
  Decisão                             Alternativas / ponto a definir
  ----------------------------------- -----------------------------------
  Banco de dados                      PostgreSQL ou SQL Server

  WhatsApp                            Evolution API ou WhatsApp Cloud API
                                      oficial

  Unicidade da agenda                 Uma palestra global por data ou uma
                                      por congregação/data

  Administração                       Uma congregação administradora ou
                                      várias congregações administradoras

  Fuso horário                        Fuso oficial de operação e regra de
                                      armazenamento

  Respostas WhatsApp                  Resposta numérica simples,
                                      palavra-chave ou link seguro

  Recusa                              Fluxo de cancelamento, substituição
                                      e notificação do administrador

  Lembrete                            Enviar apenas para pendentes ou
                                      também para confirmados

  Usuários                            Papéis, permissões e processo de
                                      criação de contas
  -----------------------------------------------------------------------

Não assumir respostas para decisões pendentes. Registrar a alternativa
recomendada, impactos e necessidade de aprovação humana em
`docs/decisions/`.

## 4. Papéis e responsabilidades dos agentes

### 4.1 Maestro --- Orquestrador

**Missão:** conduzir o workflow de ponta a ponta e preservar a
integridade do Harness.

Responsabilidades:

-   Ler este documento, o estado atual do projeto e a task ativa.
-   Selecionar o próximo agente conforme a fase.
-   Fornecer ao agente somente o contexto e o escopo necessários.
-   Verificar se as entradas e dependências estão disponíveis.
-   Cobrar relatório de saída padronizado.
-   Comparar entregas com critérios de aceite.
-   Encaminhar correções ao agente responsável.
-   Controlar o limite de iterações.
-   Atualizar `workflow-state.md`, `tasks.md` e relatórios.
-   Bloquear avanço diante de falhas críticas ou decisões pendentes.
-   Solicitar aprovação humana quando necessário.

O Maestro não deve implementar código por conta própria para contornar
uma falha de outro agente. Deve delegar a correção ao responsável,
exceto quando a configuração da plataforma exigir uma ação operacional
do próprio Maestro.

**Saída obrigatória:**
`docs/agent-runs/<fase>/<task-id>/maestro-report.md`.

### 4.2 Analista --- Requisitos e regras de negócio

**Missão:** transformar necessidades do produto em especificações
claras, verificáveis e rastreáveis.

Responsabilidades:

-   Analisar o escopo e identificar ambiguidades.
-   Mapear atores, jornadas, regras e exceções.
-   Definir critérios de aceite testáveis.
-   Identificar requisitos não funcionais.
-   Descrever contratos de integração e eventos.
-   Mapear riscos de segurança, privacidade e operação.
-   Registrar perguntas e decisões pendentes.
-   Manter rastreabilidade entre requisito, task e teste.

Restrições:

-   Não implementar funcionalidades.
-   Não inventar decisões de produto.
-   Não considerar requisito ambíguo como aprovado.
-   Não remover regras existentes sem autorização.

**Saídas:** - `requirements.md` - `acceptance-criteria.md` -
`business-rules.md` - `open-questions.md`, quando aplicável.

### 4.3 Engenheiro --- Arquitetura e implementação

**Missão:** implementar a task aprovada respeitando a arquitetura,
padrões e critérios de aceite.

Responsabilidades:

-   Consultar especificação, ADRs e padrões de código.
-   Projetar a solução dentro do escopo.
-   Implementar domínio, aplicação, infraestrutura e interface conforme
    a arquitetura aprovada.
-   Criar ou atualizar migrations.
-   Implementar logs, tratamento de erros e validações.
-   Criar testes unitários quando fizerem parte da task.
-   Atualizar documentação técnica.
-   Executar build e verificações locais.
-   Informar limitações, débitos técnicos e alterações de escopo.

Restrições:

-   Não alterar contratos públicos sem registrar impacto e obter
    aprovação.
-   Não adicionar dependências sem justificar.
-   Não armazenar segredos no código.
-   Não desabilitar testes ou regras de qualidade para obter build
    verde.
-   Não marcar a task como concluída.

**Saídas:** - Código e testes correspondentes. -
`implementation-report.md`. - Atualização de documentação e ADRs, quando
necessário.

### 4.4 Tester --- Qualidade e validação

**Missão:** verificar objetivamente se a implementação atende aos
requisitos e critérios de aceite.

Responsabilidades:

-   Criar ou revisar cenários de teste.
-   Executar testes unitários, integração e funcionais aplicáveis.
-   Validar regras de negócio e cenários negativos.
-   Verificar migrations e persistência.
-   Validar contratos e falhas de integrações.
-   Registrar comandos executados, resultados e evidências.
-   Abrir defeitos com passos para reprodução.

Restrições:

-   Não alterar implementação para esconder falhas.
-   Não aprovar testes que não foram executados.
-   Não substituir teste real por suposição.
-   Não aprovar com defeito crítico ou alto sem decisão explícita e
    registrada.

**Saída:** `test-report.md`.

### 4.5 Revisor --- Revisão independente

**Missão:** verificar qualidade técnica, segurança, arquitetura e
aderência ao escopo.

Responsabilidades:

-   Revisar diff e arquivos alterados.
-   Verificar separação de responsabilidades e dependências.
-   Avaliar legibilidade, manutenção e tratamento de erros.
-   Revisar autenticação, autorização, validação e exposição de dados.
-   Verificar riscos de SQL injection, XSS, CSRF, SSRF e vazamento de
    segredos, conforme aplicável.
-   Revisar idempotência e concorrência.
-   Conferir migrations, logs, observabilidade e documentação.
-   Verificar se os critérios de aceite estão cobertos.
-   Classificar achados por severidade.

Classificação:

-   **Crítico:** risco grave de segurança, perda/corrupção de dados ou
    bloqueio sistêmico.
-   **Alto:** falha relevante de regra, autorização, integridade ou
    funcionalidade.
-   **Médio:** problema de manutenção, robustez ou cobertura com impacto
    limitado.
-   **Baixo:** melhoria não bloqueante.

**Saída:** `review-report.md`.

## 5. Contrato de comunicação entre agentes

Toda chamada de agente deve receber:

1.  Identificador da fase.
2.  Identificador e título da task.
3.  Objetivo e escopo permitido.
4.  Contexto e arquivos que devem ser lidos.
5.  Dependências e decisões já aprovadas.
6.  Critérios de aceite.
7.  Restrições e ações proibidas.
8.  Formato esperado da entrega.

Toda chamada deve terminar com um relatório contendo:

``` markdown
# Agent Report

- Agent:
- Phase:
- Task:
- Status: COMPLETED | BLOCKED | NEEDS_CHANGES | FAILED

## Resumo
## Arquivos consultados
## Arquivos criados ou alterados
## Decisões tomadas
## Critérios de aceite atendidos
## Testes/comandos executados
## Evidências
## Riscos e pendências
## Próximo agente recomendado
```

O agente deve informar `BLOCKED` quando depender de decisão, acesso ou
informação que não possui. Não deve preencher lacunas com suposições.

## 6. Harness: máquina de estados

### 6.1 Estados de uma task

  Estado             Significado
  ------------------ ---------------------------------
  `BACKLOG`          Ainda não priorizada
  `READY`            Refinada e apta para execução
  `IN_PROGRESS`      Em execução
  `WAITING_REVIEW`   Implementação entregue
  `TESTING`          Validação em andamento
  `NEEDS_CHANGES`    Existem correções necessárias
  `BLOCKED`          Dependência ou decisão pendente
  `DONE`             Critérios atendidos e aprovados
  `CANCELLED`        Cancelada com justificativa

### 6.2 Transições permitidas

``` text
BACKLOG -> READY
READY -> IN_PROGRESS
IN_PROGRESS -> WAITING_REVIEW
WAITING_REVIEW -> TESTING
TESTING -> NEEDS_CHANGES
TESTING -> DONE
NEEDS_CHANGES -> IN_PROGRESS
qualquer estado não terminal -> BLOCKED
BLOCKED -> estado anterior, após resolver bloqueio
BACKLOG/READY -> CANCELLED, com justificativa
```

Somente o Maestro atualiza o estado oficial da task.

### 6.3 Loop de execução

Para cada task:

1.  Maestro valida se a task está `READY`.
2.  Analista detalha ou confirma os requisitos da task.
3.  Maestro valida a especificação.
4.  Engenheiro implementa.
5.  Tester executa os testes.
6.  Revisor realiza a revisão independente.
7.  Maestro verifica todos os relatórios e evidências.
8.  Se aprovado, marca `DONE` e libera a próxima task.
9.  Se houver falha, cria uma solicitação de correção direcionada ao
    agente responsável e retorna ao ponto necessário do fluxo.

**Limite padrão: três rodadas de correção por task**, sem contar a
primeira execução. Ao atingir o limite, a task fica `BLOCKED` e exige
análise humana. O limite não pode ser contornado reiniciando a contagem.

### 6.4 Condições de parada

O workflow deve parar imediatamente quando:

-   uma decisão de produto necessária estiver pendente;
-   uma dependência externa não estiver disponível;
-   houver risco de perda ou corrupção de dados;
-   houver falha crítica ou alta de segurança;
-   o agente não tiver acesso ou contexto suficiente;
-   os critérios de aceite não puderem ser comprovados;
-   três rodadas de correção forem consumidas;
-   houver conflito entre requisitos, ADRs e implementação;
-   for necessária aprovação humana para ação sensível.

O workflow pode ser encerrado normalmente quando:

-   todas as tasks obrigatórias da fase estiverem `DONE`;
-   todos os critérios de saída da fase forem atendidos;
-   relatórios e evidências estiverem registrados;
-   o Maestro atualizar o estado e registrar a decisão.

### 6.5 Condições de sucesso do projeto

O projeto só pode ser declarado pronto para release quando:

-   todas as fases obrigatórias estiverem concluídas;
-   build de Release estiver aprovado;
-   testes exigidos estiverem aprovados;
-   revisão não tiver achados críticos ou altos pendentes;
-   migrations estiverem documentadas e validadas;
-   Docker build estiver aprovado;
-   pipeline CI estiver verde;
-   configuração de produção e secrets estiver documentada;
-   backup, recuperação e estratégia de rollback estiverem definidos;
-   smoke tests do ambiente de homologação estiverem aprovados;
-   aprovação humana para publicação tiver sido registrada.

## 7. Estrutura de diretórios do repositório

``` text
AgendaPalestras/
├── .ai/
│   ├── HARNESS.md
│   ├── workflow-state.md
│   ├── agent-contract.md
│   └── prompts/
│       ├── maestro.md
│       ├── analista.md
│       ├── engenheiro.md
│       ├── tester.md
│       └── revisor.md
│
├── docs/
│   ├── product/
│   │   ├── vision.md
│   │   ├── requirements.md
│   │   ├── business-rules.md
│   │   └── acceptance-criteria.md
│   ├── architecture/
│   │   ├── architecture.md
│   │   ├── data-model.md
│   │   ├── integrations.md
│   │   └── security.md
│   ├── decisions/
│   │   └── ADR-0001-template.md
│   ├── phases/
│   │   ├── phase-00-foundation.md
│   │   ├── phase-01-discovery.md
│   │   ├── phase-02-architecture.md
│   │   ├── phase-03-foundation-code.md
│   │   ├── phase-04-core-domain.md
│   │   ├── phase-05-scheduling.md
│   │   ├── phase-06-whatsapp.md
│   │   ├── phase-07-security.md
│   │   ├── phase-08-quality.md
│   │   └── phase-09-delivery.md
│   └── agent-runs/
│       └── <phase>/<task-id>/
│           ├── maestro-report.md
│           ├── analysis-report.md
│           ├── implementation-report.md
│           ├── test-report.md
│           └── review-report.md
│
├── src/
│   ├── AgendaPalestras.Web/
│   ├── AgendaPalestras.Domain/
│   ├── AgendaPalestras.Application/
│   └── AgendaPalestras.Infrastructure/
│
├── tests/
│   ├── AgendaPalestras.UnitTests/
│   ├── AgendaPalestras.IntegrationTests/
│   └── AgendaPalestras.ArchitectureTests/
│
├── deploy/
│   └── docker/
│       └── README.md
│
├── .github/
│   └── workflows/
│       ├── ci.yml
│       └── cd.yml
│
├── .dockerignore
├── .gitignore
├── Dockerfile
├── docker-compose.yml
├── Directory.Build.props
├── global.json
├── README.md
└── AgendaPalestras.sln
```

Os arquivos de relatório devem ser criados conforme a execução. Não
criar relatórios fictícios ou antecipar status de sucesso.

## 8. Fases e tasks do desenvolvimento

Cada fase possui objetivo, tasks, dependências e critério de saída. O
Maestro não pode iniciar uma fase se as dependências obrigatórias não
estiverem concluídas.

### Fase 00 --- Preparação do Harness

**Objetivo:** preparar o processo multiagente e o repositório.

-   [ ] T00.01 Criar estrutura de diretórios.
-   [ ] T00.02 Criar contratos e prompts dos agentes.
-   [ ] T00.03 Criar `workflow-state.md`.
-   [ ] T00.04 Criar template de task e relatórios.
-   [ ] T00.05 Definir regras de branches e commits.
-   [ ] T00.06 Validar que todos os agentes conseguem ler e produzir os
    artefatos.

**Saída:** Harness operacional e primeira task registrada.

### Fase 01 --- Discovery e requisitos

**Objetivo:** fechar escopo, atores e regras de negócio.

-   [ ] T01.01 Consolidar visão e objetivos.
-   [ ] T01.02 Mapear atores e permissões.
-   [ ] T01.03 Especificar congregações.
-   [ ] T01.04 Especificar oradores.
-   [ ] T01.05 Especificar discursos.
-   [ ] T01.06 Especificar agenda e conflitos de data.
-   [ ] T01.07 Especificar convite, resposta e lembrete WhatsApp.
-   [ ] T01.08 Definir estados e transições.
-   [ ] T01.09 Definir requisitos não funcionais.
-   [ ] T01.10 Resolver decisões pendentes ou registrar bloqueios.

**Saída:** requisitos, regras e critérios de aceite aprovados.

### Fase 02 --- Arquitetura e design

**Objetivo:** definir arquitetura antes da implementação.

-   [ ] T02.01 Definir versão suportada do .NET.
-   [ ] T02.02 Definir arquitetura MVC e camadas.
-   [ ] T02.03 Definir banco e estratégia de migrations.
-   [ ] T02.04 Definir entidades, relacionamentos e constraints.
-   [ ] T02.05 Definir autenticação e autorização.
-   [ ] T02.06 Definir integração WhatsApp e webhooks.
-   [ ] T02.07 Definir processamento de tarefas agendadas.
-   [ ] T02.08 Definir logs, observabilidade e tratamento de erros.
-   [ ] T02.09 Criar ADRs necessários.

**Saída:** arquitetura, modelo de dados e decisões aprovados.

### Fase 03 --- Fundação técnica e DevOps

**Objetivo:** criar a solução compilável e o pipeline básico.

-   [ ] T03.01 Criar solution e projetos.
-   [ ] T03.02 Configurar referências e dependências entre camadas.
-   [ ] T03.03 Configurar nullable, analyzers e convenções.
-   [ ] T03.04 Configurar Entity Framework Core.
-   [ ] T03.05 Configurar banco local via Docker Compose.
-   [ ] T03.06 Criar Dockerfile multi-stage.
-   [ ] T03.07 Criar `.dockerignore`.
-   [ ] T03.08 Criar health checks.
-   [ ] T03.09 Criar GitHub Actions CI.
-   [ ] T03.10 Executar build e testes no pipeline.

**Saída:** aplicação inicial executável localmente e CI aprovado.

### Fase 04 --- Domínio e cadastros

**Objetivo:** entregar a base funcional do produto.

-   [ ] T04.01 Criar entidade Congregação.
-   [ ] T04.02 Criar entidade Orador.
-   [ ] T04.03 Criar entidade Discurso.
-   [ ] T04.04 Criar configurações e migrations.
-   [ ] T04.05 Implementar serviços e validações.
-   [ ] T04.06 Implementar CRUD de congregações.
-   [ ] T04.07 Implementar CRUD de oradores.
-   [ ] T04.08 Implementar CRUD de discursos.
-   [ ] T04.09 Implementar telas Razor e navegação.
-   [ ] T04.10 Criar testes unitários e de integração.

**Saída:** cadastros funcionais, regras validadas e persistência
aprovada.

### Fase 05 --- Agenda de palestras

**Objetivo:** implementar o agendamento e as regras de conflito.

-   [ ] T05.01 Criar entidade Agenda.
-   [ ] T05.02 Implementar relacionamento com congregação, orador e
    discurso.
-   [ ] T05.03 Implementar regra de unicidade da agenda conforme ADR
    aprovado.
-   [ ] T05.04 Implementar validação de orador e discurso ativos.
-   [ ] T05.05 Implementar criação, edição e cancelamento.
-   [ ] T05.06 Implementar consulta por período e calendário.
-   [ ] T05.07 Implementar status da confirmação.
-   [ ] T05.08 Implementar concorrência e tratamento de conflito.
-   [ ] T05.09 Criar testes de regras e cenários concorrentes.

**Saída:** agenda funcional, sem duplicidade e com regras cobertas.

### Fase 06 --- WhatsApp e automações

**Objetivo:** enviar convites, processar respostas e lembretes.

-   [ ] T06.01 Implementar contrato/abstração do provedor WhatsApp.
-   [ ] T06.02 Implementar adaptador do provedor aprovado.
-   [ ] T06.03 Configurar credenciais por secret/environment.
-   [ ] T06.04 Implementar envio de convite após cadastro.
-   [ ] T06.05 Implementar persistência e rastreio de mensagens.
-   [ ] T06.06 Implementar endpoint de webhook.
-   [ ] T06.07 Validar autenticidade do webhook.
-   [ ] T06.08 Correlacionar resposta com agenda e telefone.
-   [ ] T06.09 Processar confirmação e recusa de forma idempotente.
-   [ ] T06.10 Implementar job de lembrete sete dias antes.
-   [ ] T06.11 Impedir duplicidade de lembretes.
-   [ ] T06.12 Implementar retentativas e tratamento de falha.
-   [ ] T06.13 Testar respostas inválidas, repetidas e fora do prazo.

**Saída:** fluxo de comunicação testado e auditável.

### Fase 07 --- Autenticação e segurança

**Objetivo:** proteger o acesso e os dados.

-   [ ] T07.01 Implementar identidade e armazenamento seguro de senha.
-   [ ] T07.02 Implementar login e logout.
-   [ ] T07.03 Definir estratégia JWT compatível com MVC.
-   [ ] T07.04 Proteger cookies e tokens.
-   [ ] T07.05 Implementar autorização por papel/política.
-   [ ] T07.06 Proteger formulários contra CSRF.
-   [ ] T07.07 Validar entradas e saídas.
-   [ ] T07.08 Revisar segredos, logs e dados pessoais.
-   [ ] T07.09 Executar testes de autenticação e autorização.

**Saída:** acesso protegido e controles de segurança verificados.

### Fase 08 --- Qualidade e estabilização

**Objetivo:** validar o sistema de ponta a ponta.

-   [ ] T08.01 Executar testes unitários.
-   [ ] T08.02 Executar testes de integração.
-   [ ] T08.03 Executar testes de arquitetura.
-   [ ] T08.04 Validar jornadas completas.
-   [ ] T08.05 Validar falhas do provedor WhatsApp.
-   [ ] T08.06 Validar jobs e idempotência.
-   [ ] T08.07 Revisar logs e tratamento de exceções.
-   [ ] T08.08 Executar revisão de segurança.
-   [ ] T08.09 Corrigir defeitos e registrar evidências.

**Saída:** critérios de qualidade atendidos e defeitos bloqueantes
resolvidos.

### Fase 09 --- Entrega e CI/CD

**Objetivo:** preparar build, imagem e publicação controlada.

-   [ ] T09.01 Finalizar Dockerfile de produção.
-   [ ] T09.02 Finalizar Docker Compose de ambiente local/homologação.
-   [ ] T09.03 Configurar build e publicação de imagem no registry.
-   [ ] T09.04 Configurar GitHub Actions CD.
-   [ ] T09.05 Configurar GitHub Environments e secrets.
-   [ ] T09.06 Definir execução segura de migrations.
-   [ ] T09.07 Configurar health check e smoke tests.
-   [ ] T09.08 Documentar deploy e rollback.
-   [ ] T09.09 Executar homologação.
-   [ ] T09.10 Obter aprovação humana para produção.

**Saída:** entrega reproduzível, pipeline validado e release autorizado.

## 9. Padrões de implementação

### 9.1 Arquitetura sugerida

A solução deve manter dependências apontando para o núcleo:

``` text
AgendaPalestras.Domain
        ↑
AgendaPalestras.Application
        ↑
AgendaPalestras.Infrastructure

AgendaPalestras.Web -> Application
AgendaPalestras.Web -> Infrastructure (somente composição/DI)
```

Projetos:

-   `AgendaPalestras.Domain`: entidades, enums, regras e contratos do
    domínio.
-   `AgendaPalestras.Application`: casos de uso, DTOs, validações e
    interfaces.
-   `AgendaPalestras.Infrastructure`: EF Core, repositórios, WhatsApp e
    jobs.
-   `AgendaPalestras.Web`: MVC, Razor Views, autenticação, controllers e
    composição da aplicação.

Evitar abstrações desnecessárias. Criar interfaces quando houver
fronteira real, necessidade de substituição ou benefício claro de teste.

### 9.2 Padrões de código

-   Usar async/await para operações de I/O.
-   Propagar `CancellationToken` quando aplicável.
-   Aplicar validação no servidor.
-   Não expor entidades diretamente nas Views.
-   Utilizar DTOs/ViewModels.
-   Manter controllers finos.
-   Centralizar regras de negócio nos serviços/casos de uso ou domínio.
-   Usar migrations versionadas.
-   Não registrar tokens, senhas ou conteúdo sensível em logs.
-   Usar transações quando a consistência exigir.
-   Tratar concorrência no banco, não apenas na interface.

## 10. Docker

### 10.1 Estratégia

Usar Dockerfile multi-stage:

1.  **SDK:** restore, build e publish.
2.  **Runtime:** executar somente os artefatos publicados.
3.  Executar com usuário não-root quando suportado.
4.  Expor somente a porta necessária.
5.  Configurar `ASPNETCORE_URLS`.
6.  Não copiar `.env`, secrets ou arquivos locais para a imagem.
7.  Usar `.dockerignore`.
8.  Preferir imagem base oficial e versão suportada, fixando tags
    deliberadamente.
9.  Adotar health check na aplicação e no orquestrador.
10. Executar a aplicação sem privilégios desnecessários.

### 10.2 Modelo inicial de Dockerfile

O Engenheiro deve ajustar os nomes dos projetos e a versão do
SDK/runtime à decisão da Fase 02.

``` dockerfile
# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore AgendaPalestras.sln
RUN dotnet publish src/AgendaPalestras.Web/AgendaPalestras.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "AgendaPalestras.Web.dll"]
```

> A versão `8.0` é um exemplo inicial, não uma decisão definitiva. A
> versão suportada deve ser escolhida e registrada na Fase 02. O uso de
> `USER $APP_UID` deve ser validado na imagem base selecionada.

### 10.3 Docker Compose

O Compose local deverá orquestrar:

-   aplicação web;
-   banco de dados;
-   dependências locais necessárias.

Regras:

-   volumes persistentes para o banco;
-   health check do banco;
-   aplicação aguardando o banco ficar saudável;
-   credenciais fornecidas por variáveis de ambiente;
-   arquivo `.env.example` sem valores secretos;
-   nenhuma credencial real versionada.

Não utilizar o Compose local como substituto da configuração de
produção.

## 11. GitHub Actions --- CI

### 11.1 Gatilhos

-   Pull request para `main` e `develop`.
-   Push para `main` e `develop`.
-   Execução manual para diagnóstico.

### 11.2 Jobs mínimos

1.  Checkout.
2.  Setup da versão do .NET definida no projeto.
3.  Restore.
4.  Build Release.
5.  Testes unitários.
6.  Testes de integração com dependências controladas.
7.  Verificação de formatação.
8.  Publicação de resultados de teste.
9.  Build da imagem Docker em PR, sem publicar.
10. Publicação de artefatos úteis para diagnóstico.

### 11.3 Exemplo inicial de CI

``` yaml
name: CI

on:
  pull_request:
    branches: [main, develop]
  push:
    branches: [main, develop]

permissions:
  contents: read

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore
        run: dotnet restore AgendaPalestras.sln

      - name: Build
        run: dotnet build AgendaPalestras.sln -c Release --no-restore

      - name: Unit tests
        run: dotnet test tests/AgendaPalestras.UnitTests/AgendaPalestras.UnitTests.csproj -c Release --no-build

      - name: Integration tests
        run: dotnet test tests/AgendaPalestras.IntegrationTests/AgendaPalestras.IntegrationTests.csproj -c Release

      - name: Docker build
        run: docker build -t agenda-palestras:ci .
```

Este exemplo é um ponto de partida. O Engenheiro deve adequar versões,
projetos de teste, serviços auxiliares e publicação de resultados. As
versões de Actions devem ser revisadas e atualizadas de forma
controlada.

## 12. GitHub Actions --- CD

O CD deve ser separado do CI.

### 12.1 Fluxo recomendado

-   PR: validação, sem publicação.
-   Merge em `main`: build e publicação de imagem com tag imutável.
-   Homologação: deploy automático ou controlado, conforme ambiente.
-   Produção: aprovação manual via GitHub Environment.
-   Após deploy: health check e smoke tests.
-   Falha: interromper promoção e executar procedimento de rollback
    documentado.

### 12.2 Segurança do pipeline

-   Usar GitHub Environments.
-   Exigir reviewers para produção.
-   Usar permissões mínimas em `GITHUB_TOKEN`.
-   Usar secrets ou OIDC quando o provedor suportar.
-   Não utilizar credenciais de produção em PR de forks.
-   Fixar Actions em versões revisadas; para ambientes de maior
    exigência, avaliar pin por SHA.
-   Publicar imagem com tag baseada no commit SHA.
-   Não sobrescrever tags imutáveis.
-   Não executar migrations destrutivas automaticamente sem plano e
    aprovação.

### 12.3 Estratégia de migrations

-   Migrations versionadas no repositório.
-   Validar migrations no CI.
-   Fazer backup antes de alterações de risco.
-   Planejar compatibilidade entre versão anterior e nova.
-   Executar migrations como etapa controlada do deploy.
-   Não permitir que várias réplicas tentem aplicar migrations
    simultaneamente sem coordenação.
-   Documentar rollback ou estratégia de correção para cada migration
    relevante.

## 13. Definition of Ready (DoR)

Uma task só pode entrar em `READY` quando:

-   objetivo estiver claro;
-   escopo estiver delimitado;
-   dependências estiverem identificadas;
-   critérios de aceite forem verificáveis;
-   decisões necessárias estiverem aprovadas;
-   arquivos de referência estiverem disponíveis;
-   riscos relevantes estiverem registrados;
-   tamanho permitir execução e revisão controladas.

## 14. Definition of Done (DoD)

Uma task só pode entrar em `DONE` quando:

-   implementação estiver dentro do escopo;
-   critérios de aceite estiverem atendidos;
-   build relevante estiver aprovado;
-   testes aplicáveis estiverem executados e aprovados;
-   revisão estiver aprovada;
-   nenhum achado crítico/alto estiver pendente;
-   documentação estiver atualizada;
-   logs e tratamento de erros estiverem adequados;
-   relatórios dos agentes estiverem salvos;
-   Maestro tiver registrado a decisão.

Uma task documental ou de discovery pode ter critérios específicos de
DoD, definidos pelo Maestro e Analista antes da execução.

## 15. Templates operacionais

### 15.1 Task

``` markdown
# TASK-XX.XX — Título

- Phase:
- Status: BACKLOG
- Priority: HIGH | MEDIUM | LOW
- Owner agent:
- Dependencies:
- Requirement references:

## Objective

## Scope
### Included
### Excluded

## Business rules

## Acceptance criteria
- [ ] AC-01:
- [ ] AC-02:

## Technical notes

## Risks

## Evidence required

## Completion notes
```

### 15.2 Solicitação de correção

``` markdown
# Change Request — TASK-XX.XX

- Iteration: 1/3
- Responsible agent:
- Origin: TESTER | REVIEWER | MAESTRO
- Severity: CRITICAL | HIGH | MEDIUM | LOW

## Finding

## Evidence

## Expected behavior

## Required correction

## Acceptance criteria for correction

## Files in scope

## Restrictions
```

### 15.3 ADR

``` markdown
# ADR-XXXX — Título

- Status: PROPOSED | ACCEPTED | REJECTED | SUPERSEDED
- Date:
- Decision owner:

## Context

## Options considered

## Decision

## Consequences

## Security and operational impact
```

## 16. Estado persistente do workflow

O arquivo `.ai/workflow-state.md` é a fonte oficial para retomada de
execução.

Modelo inicial:

``` markdown
# Workflow State

- Project: Agenda de Palestras
- Current phase: 00
- Current task: T00.01
- Workflow status: READY
- Active agent: MAESTRO
- Correction iteration: 0/3
- Last completed task: none
- Blockers: none
- Last updated: YYYY-MM-DD HH:mm

## Current objective

Preparar a estrutura do Harness.

## Next action

Delegar T00.01 ao agente responsável.

## Approved decisions

Nenhuma decisão técnica definitiva registrada.

## Pending decisions

Consultar docs/product/open-questions.md.

## Evidence

Links e caminhos para relatórios, commits e execuções.
```

Ao iniciar qualquer sessão, o Maestro deve:

1.  Ler `.ai/HARNESS.md`.
2.  Ler `.ai/workflow-state.md`.
3.  Ler a fase e task ativa.
4.  Consultar relatórios anteriores e ADRs relacionados.
5.  Confirmar o estado real do repositório.
6.  Retomar do último estado confirmado, sem repetir tarefas concluídas.
7.  Se houver divergência entre estado declarado e evidência, bloquear e
    reconciliar antes de prosseguir.

## 17. Protocolo de início de sessão

Quando o usuário solicitar continuidade, o Maestro deve:

-   identificar se é início, retomada, correção ou encerramento;
-   confirmar o estado atual;
-   apresentar fase e task ativas;
-   identificar bloqueios;
-   executar somente a próxima ação autorizada;
-   informar o resultado e a próxima ação.

Comando sugerido:

``` text
MAESTRO, iniciar ou retomar o workflow do Agenda de Palestras.

Leia .ai/HARNESS.md, .ai/workflow-state.md,
a fase ativa, a task ativa e os relatórios anteriores.

Valide o estado real do repositório.
Não pule fases, não assuma decisões pendentes
e não execute mais de uma task sem autorização.

Conduza os agentes conforme seus contratos.
Ao final, atualize o estado e apresente evidências,
bloqueios e próxima ação.
```

## 18. Relatório de encerramento de fase

``` markdown
# Phase Completion Report

- Phase:
- Result: APPROVED | BLOCKED | REJECTED

## Tasks completed

## Tasks not completed

## Acceptance criteria

## Test and review evidence

## Architecture decisions

## Risks and technical debt

## Human approvals required

## Maestro decision

## Next phase
```

## 19. Regras finais do Harness

-   O Maestro é o único orquestrador.
-   Nenhum agente deve executar tarefas fora do escopo recebido.
-   Toda chamada deve produzir status e relatório.
-   Nenhuma task pode ser concluída sem evidências.
-   Correções têm limite global de três rodadas por task.
-   Bloqueios precisam de justificativa e responsável pela resolução.
-   Decisões importantes devem ser persistidas em ADRs.
-   O estado deve ser atualizado ao final de cada chamada e transição.
-   Não apagar relatórios anteriores; registrar novas iterações.
-   Não publicar em produção sem aprovação humana.
-   Não declarar o projeto concluído enquanto houver critérios
    obrigatórios pendentes.

------------------------------------------------------------------------

## 20. Próxima ação recomendada

O primeiro ciclo deve executar somente a **Fase 00 --- Preparação do
Harness**:

1.  Criar os cinco arquivos de agentes.
2.  Criar os contratos de comunicação.
3.  Criar o estado inicial.
4.  Criar os templates de task e relatórios.
5.  Validar o fluxo com uma task pequena, sem implementar
    funcionalidades do produto.

Depois disso, o Maestro inicia a Fase 01 e conduz a descoberta dos
requisitos e das decisões pendentes.

**Regra de ouro:** o Harness controla o processo; os agentes executam o
trabalho; as evidências comprovam o resultado; o usuário mantém a
decisão final sobre produto e produção.
