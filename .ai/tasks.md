# Catálogo Mestre de Fases e Tarefas — Agenda de Palestras

> Registro central de todas as tarefas planejadas, seus responsáveis, dependências e status oficial.
> Apenas o Maestro possui autorização para transicionar os status das tarefas neste documento.

---

## Legenda de Status
- `BACKLOG`: Aguardando priorização ou início da fase.
- `READY`: Atende à DoR e apta para execução.
- `IN_PROGRESS`: Em execução pelo agente responsável.
- `WAITING_REVIEW`: Implementação pronta, aguardando validação/revisão.
- `TESTING`: Sob validação ativa pelo Tester.
- `NEEDS_CHANGES`: Com apontamentos; retornada para correção.
- `BLOCKED`: Bloqueada por dependência externa ou limite de correções.
- `DONE`: Concluída com evidências e DoD comprovados.
- `CANCELLED`: Cancelada justificadamente.

---

## Fase 00 — Preparação do Harness
*Objetivo: Estabelecer as regras, infraestrutura de agentes, contratos, skills e controle de estado do Harness.*

| ID | Título da Task | Responsável | Dependência | Status |
| :--- | :--- | :--- | :--- | :--- |
| **T00.01** | Criar e validar estrutura de diretórios, regras e Harness | Maestro | Nenhuma | `DONE` |
| **T00.02** | Criar contratos e definições operacionais dos 5 agentes | Maestro | T00.01 | `DONE` |
| **T00.03** | Inicializar controle de estado e logs (`workflow-state.md`) | Maestro | T00.01 | `DONE` |
| **T00.04** | Criar templates de task, relatórios, ADRs e change request | Maestro | T00.01 | `DONE` |
| **T00.05** | Definir regras de branches, commits e guardrails do Git | Maestro | T00.01 | `DONE` |
| **T00.06** | Validar leitura de artefatos e alinhar modelo com usuário | Maestro | T00.01..T00.05 | `DONE` |

---

## Fase 01 — Discovery e Requisitos
*Objetivo: Consolidar escopo, atores, jornadas, regras de negócio e critérios de aceite.*

| ID | Título da Task | Responsável | Dependência | Status |
| :--- | :--- | :--- | :--- | :--- |
| **T01.01** | Consolidar visão, público-alvo e objetivos do produto | Analista | Fase 00 | `DONE` |
| **T01.02** | Mapear atores, perfis e matriz de permissões | Analista | T01.01 | `DONE` |
| **T01.03** | Especificar módulo de Congregações e regras associadas | Analista | T01.02 | `DONE` |
| **T01.04** | Especificar módulo de Oradores e regras de vínculo | Analista | T01.03 | `DONE` |
| **T01.05** | Especificar módulo de Discursos e regras de status | Analista | T01.04 | `DONE` |
| **T01.06** | Especificar Agenda e regras de conflito e unicidade | Analista | T01.05 | `DONE` |
| **T01.07** | Especificar fluxo WhatsApp: convites, respostas e lembrete | Analista | T01.06 | `DONE` |
| **T01.08** | Definir máquina de estados da Agenda e transições válidas | Analista | T01.07 | `DONE` |
| **T01.09** | Definir requisitos não funcionais e conformidade de dados | Analista | T01.08 | `DONE` |
| **T01.10** | Mapear decisões pendentes e catalogar em open-questions.md | Analista | T01.09 | `DONE` |

---

## Fase 02 — Arquitetura e Design
*Objetivo: Definir a arquitetura da solução, tecnologias, integrações e registrar ADRs.*

| ID | Título da Task | Responsável | Dependência | Status |
| :--- | :--- | :--- | :--- | :--- |
| **T02.01** | Definir versão suportada do .NET e runtime oficial | Engenheiro | Fase 01 | `DONE` |
| **T02.02** | Definir arquitetura Clean Architecture MVC e convenções | Engenheiro | T02.01 | `DONE` |
| **T02.03** | Definir banco de dados (ADR) e estratégia de migrations | Engenheiro | T02.02 | `DONE` |
| **T02.04** | Modelar entidades, relacionamentos e constraints no domínio | Engenheiro | T02.03 | `DONE` |
| **T02.05** | Definir arquitetura de autenticação, cookies e políticas | Engenheiro | T02.04 | `DONE` |
| **T02.06** | Definir arquitetura da integração WhatsApp e webhooks | Engenheiro | T02.05 | `DONE` |
| **T02.07** | Definir processamento em background para lembretes | Engenheiro | T02.06 | `DONE` |
| **T02.08** | Definir observabilidade, logs estruturados e health checks | Engenheiro | T02.07 | `DONE` |
| **T02.09** | Formalizar e aprovar conjunto de ADRs arquiteturais | Engenheiro / Revisor | T02.01..T02.08 | `DONE` |

---

## Fase 03 — Fundação Técnica e DevOps
*Objetivo: Criar a solution .NET, estrutura de projetos compilável, docker e pipeline base.*

| ID | Título da Task | Responsável | Dependência | Status |
| :--- | :--- | :--- | :--- | :--- |
| **T03.01** | Criar solution .NET e projetos em src/ e tests/ | Engenheiro | Fase 02 | `DONE` |
| **T03.02** | Configurar referências entre camadas respeitando Clean Arch | Engenheiro | T03.01 | `DONE` |
| **T03.03** | Configurar Nullable, analyzers e Directory.Build.props | Engenheiro | T03.02 | `DONE` |
| **T03.04** | Configurar DbContext e persistência com EF Core | Engenheiro | T03.03 | `DONE` |
| **T03.05** | Configurar banco local via Docker Compose e .env.example | Engenheiro | T03.04 | `DONE` |
| **T03.06** | Criar Dockerfile multi-stage com usuário não-root | Engenheiro | T03.05 | `DONE` |
| **T03.07** | Criar .dockerignore adequado para .NET | Engenheiro | T03.06 | `DONE` |
| **T03.08** | Criar endpoint de Health Check (/healthz) | Engenheiro | T03.07 | `DONE` |
| **T03.09** | Criar workflow básico do GitHub Actions CI | Engenheiro | T03.08 | `DONE` |
| **T03.10** | Executar build e testes locais comprovando sanidade | Tester | T03.09 | `DONE` |

---

## Fase 04 — Domínio e Cadastros
*Objetivo: Implementar as entidades centrais, regras de cadastro e persistência.*

| ID | Título da Task | Responsável | Dependência | Status |
| :--- | :--- | :--- | :--- | :--- |
| **T04.01** | Criar entidade Congregação e validações de domínio | Engenheiro | Fase 03 | `DONE` |
| **T04.02** | Criar entidade Orador e regras de vínculo | Engenheiro | T04.01 | `DONE` |
| **T04.03** | Criar entidade Discurso e regras de status ativo | Engenheiro | T04.02 | `DONE` |
| **T04.04** | Criar mapeamentos Fluent API e migrations no EF Core | Engenheiro | T04.03 | `DONE` |
| **T04.05** | Implementar serviços de aplicação e DTOs de cadastro | Engenheiro | T04.04 | `DONE` |
| **T04.06** | Implementar CRUD e telas de Congregações | Engenheiro | T04.05 | `DONE` |
| **T04.07** | Implementar CRUD e telas de Oradores | Engenheiro | T04.06 | `DONE` |
| **T04.08** | Implementar CRUD e telas de Discursos | Engenheiro | T04.07 | `DONE` |
| **T04.09** | Implementar navegação e layout Razor responsivo | Engenheiro | T04.08 | `DONE` |
| **T04.10** | Criar e executar suite de testes de cadastros | Tester | T04.09 | `DONE` |

---

## Fase 05 — Agenda de Palestras
*Objetivo: Implementar o agendamento de palestras, regras de conflito e concorrência.*

| ID | Título da Task | Responsável | Dependência | Status |
| :--- | :--- | :--- | :--- | :--- |
| **T05.01** | Criar entidade Agenda e relacionamentos no domínio | Engenheiro | Fase 04 | `DONE` |
| **T05.02** | Implementar relacionamento com Congregação, Orador e Discurso | Engenheiro | T05.01 | `DONE` |
| **T05.03** | Implementar regra de unicidade da agenda conforme ADR aprovado | Engenheiro | T05.02 | `DONE` |
| **T05.04** | Implementar validação de orador ativo e discurso ativo | Engenheiro | T05.03 | `DONE` |
| **T05.05** | Implementar criação, edição e cancelamento de palestras | Engenheiro | T05.04 | `DONE` |
| **T05.06** | Implementar consulta por período, filtros e visão calendário | Engenheiro | T05.05 | `DONE` |
| **T05.07** | Implementar controle de status da confirmação da palestra | Engenheiro | T05.06 | `DONE` |
| **T05.08** | Implementar controle de concorrência e integridade no banco | Engenheiro | T05.07 | `DONE` |
| **T05.09** | Criar e executar suite de testes de conflito e concorrência | Tester | T05.08 | `DONE` |

---

## Fase 06 — WhatsApp e Automações
*Objetivo: Implementar integração com WhatsApp, envio de convites, webhook e lembretes.*

| ID | Título da Task | Responsável | Dependência | Status |
| :--- | :--- | :--- | :--- | :--- |
| **T06.01** | Criar contrato/abstração da interface do provedor WhatsApp | Engenheiro | Fase 05 | `DONE` |
| **T06.02** | Implementar adaptador do provedor de WhatsApp aprovado | Engenheiro | T06.01 | `DONE` |
| **T06.03** | Configurar injeção de credenciais seguras via environment | Engenheiro | T06.02 | `DONE` |
| **T06.04** | Implementar envio automático de convite ao criar palestra | Engenheiro | T06.03 | `DONE` |
| **T06.05** | Implementar persistência e rastreamento de histórico de mensagens | Engenheiro | T06.04 | `DONE` |
| **T06.06** | Implementar endpoint seguro receptor de webhooks | Engenheiro | T06.05 | `DONE` |
| **T06.07** | Validar autenticidade e assinatura do webhook recebido | Engenheiro | T06.06 | `DONE` |
| **T06.08** | Correlacionar resposta do webhook com agenda e telefone | Engenheiro | T06.07 | `DONE` |
| **T06.09** | Processar confirmação (1) e recusa (2) de forma idempotente | Engenheiro | T06.08 | `DONE` |
| **T06.10** | Implementar job agendado de lembrete 7 dias antes do evento | Engenheiro | T06.09 | `DONE` |
| **T06.11** | Implementar trava contra duplicidade de lembretes | Engenheiro | T06.10 | `DONE` |
| **T06.12** | Implementar retentativas (retry) e fallback de comunicação | Engenheiro | T06.11 | `DONE` |
| **T06.13** | Criar e executar testes de webhook, idempotência e bordas | Tester | T06.12 | `DONE` |

---

## Fase 07 — Autenticação e Segurança
*Objetivo: Implementar controle de acesso, perfis, proteção contra vulnerabilidades e auditoria.*

| ID | Título da Task | Responsável | Dependência | Status |
| :--- | :--- | :--- | :--- | :--- |
| **T07.01** | Implementar Identity/armazenamento seguro de credenciais com hash | Engenheiro | Fase 06 | `DONE` |
| **T07.02** | Implementar login, logout e bloqueio por tentativas excessivas | Engenheiro | T07.01 | `DONE` |
| **T07.03** | Configurar autenticação por Cookie seguro compatível com MVC | Engenheiro | T07.02 | `DONE` |
| **T07.04** | Proteger cookies (HttpOnly, Secure, SameSite) | Engenheiro | T07.03 | `DONE` |
| **T07.05** | Implementar autorização baseada em papéis e políticas (Claims/Roles) | Engenheiro | T07.04 | `DONE` |
| **T07.06** | Configurar validação anti-CSRF global em formulários POST | Engenheiro | T07.05 | `DONE` |
| **T07.07** | Configurar validação rigorosa de entradas e cabeçalhos de segurança | Engenheiro | T07.06 | `DONE` |
| **T07.08** | Revisar logs e garantir ausência de segredos e dados sensíveis | Revisor | T07.07 | `DONE` |
| **T07.09** | Executar suite de testes de autenticação e autorização | Tester | T07.08 | `DONE` |

---

## Fase 08 — Qualidade e Estabilização
*Objetivo: Executar validação de ponta a ponta, testes de integração, arquitetura e auditoria.*

| ID | Título da Task | Responsável | Dependência | Status |
| :--- | :--- | :--- | :--- | :--- |
| **T08.01** | Executar suite completa de testes unitários com cobertura | Tester | Fase 07 | `DONE` |
| **T08.02** | Executar suite completa de testes de integração com banco | Tester | T08.01 | `DONE` |
| **T08.03** | Executar testes de arquitetura com NetArchTest | Tester | T08.02 | `DONE` |
| **T08.04** | Validar jornadas completas de usuário de ponta a ponta | Tester | T08.03 | `DONE` |
| **T08.05** | Validar cenários de falha de provedor externo WhatsApp | Tester | T08.04 | `DONE` |
| **T08.06** | Validar concorrência, jobs em background e idempotência | Tester | T08.05 | `DONE` |
| **T08.07** | Auditar tratamento de exceções e observabilidade | Revisor | T08.06 | `DONE` |
| **T08.08** | Executar revisão completa de segurança (OWASP Top 10) | Revisor | T08.07 | `DONE` |
| **T08.09** | Corrigir todos os defeitos encontrados e registrar evidências | Engenheiro / Maestro | T08.08 | `DONE` |

---

## Fase 09 — Entrega e CI/CD
*Objetivo: Finalizar pipeline de build, publicação de imagem, testes de fumaça e release.*

| ID | Título da Task | Responsável | Dependência | Status |
| :--- | :--- | :--- | :--- | :--- |
| **T09.01** | Finalizar Dockerfile de produção otimizado e seguro | Engenheiro | Fase 08 | `DONE` |
| **T09.02** | Finalizar Docker Compose para ambiente de homologação | Engenheiro | T09.01 | `DONE` |
| **T09.03** | Configurar publicação de imagem com tag imutável (SHA) | Engenheiro | T09.02 | `DONE` |
| **T09.04** | Configurar pipeline de CD no GitHub Actions com aprovação | Engenheiro | T09.03 | `DONE` |
| **T09.05** | Configurar GitHub Environments e proteção de branches | Engenheiro | T09.04 | `DONE` |
| **T09.06** | Definir e testar rotina controlada de execução de migrations | Engenheiro | T09.05 | `DONE` |
| **T09.07** | Configurar monitoramento de saúde e smoke tests pós-deploy | Tester | T09.06 | `DONE` |
| **T09.08** | Documentar procedimentos operacionais de deploy e rollback | Engenheiro | T09.07 | `DONE` |
| **T09.09** | Executar validação de homologação (Staging) com evidências | Tester | T09.08 | `DONE` |
| **T09.10** | Obter aprovação humana para publicação em produção | Maestro | T09.09 | `DONE` |

---

## Fase 10 — Conexão WhatsApp Admin (Evolution API e QR Code)
*Objetivo: Integrar a plataforma com a Evolution API v2 para gerenciamento da conexão do WhatsApp do admin via QR Code, consulta de status e envio de lembretes automatizados.*

| ID | Título da Task | Responsável | Dependência | Status |
| :--- | :--- | :--- | :--- | :--- |
| **T10.01** | Criar entidade ConfiguracaoSistema e migration no banco | Engenheiro | Fase 09 | `DONE` |
| **T10.02** | Implementar IEvolutionApiService com endpoints oficiais da Evolution API v2 | Engenheiro | T10.01 | `DONE` |
| **T10.03** | Atualizar WhatsAppNotificationService para formato v2 e instância ativa | Engenheiro | T10.02 | `DONE` |
| **T10.04** | Criar WhatsAppController e View Razor com exibição de QR Code e status | Engenheiro | T10.03 | `DONE` |
| **T10.05** | Integrar link no menu de navegação e testes automatizados da conexão | Tester / Revisor | T10.04 | `DONE` |

---

## Fase 11 — Calendário Mensal e Bloqueio Diário de Agendamento
*Objetivo: Implementar visualização interativa em grade de calendário mensal com discursos completos, navegação entre meses e bloqueio estrito de múltiplos agendamentos no mesmo dia.*

| ID | Título da Task | Responsável | Dependência | Status |
| :--- | :--- | :--- | :--- | :--- |
| **T11.01** | Registrar ADR-0010 e refinar requisitos do Calendário e Bloqueio Diário | Analista | Fase 10 | `DONE` |
| **T11.02** | Implementar bloqueio diário de agendamento em AgendaRepository e AgendaService | Engenheiro | T11.01 | `DONE` |
| **T11.03** | Implementar ViewModel e endpoints de Calendário Mensal em AgendaController | Engenheiro | T11.02 | `DONE` |
| **T11.04** | Implementar interface do Calendário Mensal em Razor com discursos completos | Engenheiro | T11.03 | `DONE` |
| **T11.05** | Executar testes automatizados, coletar evidências e relatório de conclusão | Tester / Revisor | T11.04 | `DONE` |

---

## Fase 12 — Programação de Fim de Semana e Exportação em PDF
*Objetivo: Implementar menu de programação de reuniões de fim de semana, permitindo configurar Tema, Orador (Nome/Congregação), Presidente, Leitor de A Sentinela e Oração Final por semana, e gerar PDF mensal para impressão e distribuição.*

| ID | Título da Task | Responsável | Dependência | Status |
| :--- | :--- | :--- | :--- | :--- |
| **T12.01** | Registrar ADR-0011 e modelar campos de reunião na entidade Agenda e Migration | Engenheiro | Fase 11 | `DONE` |
| **T12.02** | Implementar DTOs e serviços de atualização das designações semanais | Engenheiro | T12.01 | `DONE` |
| **T12.03** | Implementar IProgramacaoPdfService com QuestPDF para geração de PDF de alta fidelidade | Engenheiro | T12.02 | `DONE` |
| **T12.04** | Criar ProgramacaoController e View Razor com formulário semanal e botão de PDF | Engenheiro | T12.03 | `DONE` |
| **T12.05** | Integrar link no menu _Layout, testes automatizados e relatório de conclusão | Tester / Revisor | T12.04 | `DONE` |

---

## Fase 13 — Layout e Responsividade Mobile
*Objetivo: Modernizar o layout da aplicação com foco em dispositivos móveis (smartphones e tablets), adaptando menu, calendários, tabelas, cards e formulários para experiência touch e telas compactas.*

| ID | Título da Task | Responsável | Dependência | Status |
| :--- | :--- | :--- | :--- | :--- |
| **T13.01** | Especificar requisitos de layout mobile, navegação responsiva e critérios de aceite | Analista | Fase 12 | `DONE` |
| **T13.02** | Implementar layout base responsivo (_Layout, site.css, navbar mobile e touch targets) | Engenheiro | T13.01 | `DONE` |
| **T13.03** | Implementar responsividade mobile da Agenda (calendário adaptativo e modo lista mobile) | Engenheiro | T13.02 | `DONE` |
| **T13.04** | Implementar responsividade de Programação, Cadastros (Oradores, Congregações, Discursos) e Convites | Engenheiro | T13.03 | `DONE` |
| **T13.05** | Executar testes de responsividade, suite de regressão e relatório de conclusão | Tester / Revisor | T13.04 | `DONE` |

---

## Fase 14 — Multi-Instâncias da Evolution API por Usuário
*Objetivo: Permitir que múltiplos coordenadores utilizem suas próprias sessões do WhatsApp sem acesso à Evolution API nem à ApiKey global. O Super Admin cria e vincula a instância para cada usuário, e a tela de conexão com QR Code passa a ser liberada para o respectivo usuário.*

| ID | Título da Task | Responsável | Dependência | Status |
| :--- | :--- | :--- | :--- | :--- |
| **T14.01** | Isolamento multi-tenant de instâncias da Evolution API, gestão pelo SuperAdmin e liberação condicional para usuários | Engenheiro / Revisor | Fase 13 | `DONE` |

---

## Fase 15 — Carga Oficial de Discursos Públicos (S-34)
*Objetivo: Efetuar a carga e cadastro completo de todos os 194 discursos públicos oficiais do catálogo S-34 no banco de dados PostgreSQL com idempotência.*

| ID | Título da Task | Responsável | Dependência | Status |
| :--- | :--- | :--- | :--- | :--- |
| **T15.01** | Extrair os 194 temas de discursos oficiais S-34, gerar script SQL com resolução de conflitos e executar carga no banco | Engenheiro / Maestro | Fase 14 | `DONE` |
