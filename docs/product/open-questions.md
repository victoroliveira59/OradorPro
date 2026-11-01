# Questões Abertas e Decisões de Produto Pendentes

> Alinhamento estrito com SPEC.md Seção 3.3.  
> O Analista e o Maestro NÃO devem assumir respostas por conta própria.  
> Cada item exige recomendação técnica, avaliação de impacto e deliberação formal humana antes da Fase de implementação correspondente.

---

## Matriz de Decisões Pendentes

| ID | Tema | Alternativas em Avaliação | Impacto Técnico / Negócio | Status | Decisão / ADR |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **QP-01** | **Banco de Dados** | **PostgreSQL** ou **SQL Server** | Afeta migrations EF Core, docker-compose, tipos de dados e custos de hospedagem. | PENDENTE | ADR-0002 |
| **QP-02** | **Provedor WhatsApp** | **Evolution API** (Self-hosted) ou **WhatsApp Cloud API** (Oficial Meta) | Afeta autenticação, custos por mensagem, estabilidade e requisitos de infraestrutura. | PENDENTE | ADR-0003 |
| **QP-03** | **Unicidade da Agenda** | Uma palestra global por data OU uma por congregação/data | Define constraints de banco e se o sistema permite orador palestrando de manhã e de noite. | PENDENTE | ADR-0004 |
| **QP-04** | **Modelo de Administração** | Usuário Admin único cria e gerencia múltiplas congregações e cadastra oradores | Arquitetura centralizada: Admin gerencia múltiplas congregações e seus respectivos oradores. | **RESOLVIDO** (Aprovado pelo Usuário) | ADR-0002 |
| **QP-05** | **Fuso Horário** | Armazenamento em UTC e conversão no cliente OU DateTimeOffset com fuso fixo de Brasília | Afeta validação de datas, agendamento de jobs e relatórios de calendário. | PENDENTE | Fase 02 |
| **QP-06** | **Padrão de Resposta WhatsApp** | Numérica simples (1/2), palavras-chave ("Sim"/"Não") ou link para confirmação web | Afeta complexidade do parser de webhook e taxa de erro dos oradores. | PENDENTE | Fase 01 / 06 |
| **QP-07** | **Fluxo de Recusa** | Notificar admin por e-mail/WhatsApp, abrir status para novo orador ou cancelar agenda | Afeta jornada operacional da congregação diante de recusas. | PENDENTE | Fase 01 |
| **QP-08** | **Destinatários do Lembrete** | Disparar lembrete de 7 dias apenas para confirmações pendentes OU também para confirmadas | Afeta volume de mensagens e experiência do orador. | PENDENTE | Fase 01 |
| **QP-09** | **Gestão de Usuários** | Criação pública com aprovação de admin OU apenas convite/cadastro por administrador | Afeta telas de login, rotinas de registro e modelo de segurança. | PENDENTE | Fase 01 / 07 |

---

## Procedimento de Resolução
1. O **Analista** detalha as alternativas na Fase 01 (Task `T01.10`).
2. O **Engenheiro** e o **Revisor** avaliam custos técnicos e impactos operacionais.
3. O **Maestro** submete a decisão para aprovação humana formal.
4. Uma vez aprovada, a decisão é persistida em `docs/decisions/` como ADR no estado `ACCEPTED`.
