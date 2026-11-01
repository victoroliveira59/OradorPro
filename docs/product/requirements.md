# Especificação Inicial de Requisitos — Agenda de Palestras

> Alinhamento com SPEC.md Seção 3.1.  
> Refinamento formal será conduzido pelo Analista na Fase 01.

---

## 1. Requisitos Funcionais (RF)

| ID | Nome do Requisito | Descrição Resumida |
| :--- | :--- | :--- |
| **RF-01** | Autenticação de Usuários | O sistema deve autenticar usuários com login e senha protegida por hash forte. |
| **RF-02** | Autorização por Papel | O sistema deve restringir o acesso a telas e operações com base no perfil do usuário e congregação. |
| **RF-03** | Cadastro de Congregações | Usuário Admin gerencia múltiplas congregações com nome, endereço, dias e horários padrão de reuniões. |
| **RF-04** | Cadastro de Oradores | Manutenção de oradores sob cada congregação com nome, telefone (WhatsApp) e status (ativo/inativo). |
| **RF-05** | Repertório de Discursos do Orador | Cadastro do catálogo de discursos e associação de múltiplos discursos ao repertório de cada orador. |
| **RF-06** | Agenda de Palestras | Agendamento associando data/hora, congregação anfitriã, orador e um dos discursos de seu repertório. |
| **RF-07** | Prevenção de Conflitos | Bloqueio de agendamento caso haja colisão de datas/orador conforme regra de negócio aprovada. |
| **RF-08** | Envio de Convite WhatsApp | Disparo automático de convite via WhatsApp para o orador após a criação da agenda. |
| **RF-09** | Recepção de Resposta | Endpoint webhook para capturar resposta do orador (1 para confirmação, 2 para recusa). |
| **RF-10** | Lembrete Automático | Job em background para envio de lembrete 7 dias antes do evento. |
| **RF-11** | Histórico de Mensagens | Registro auditável de todas as mensagens enviadas e recebidas com timestamps e status. |
| **RF-12** | Dashboard Operacional | Painel com próximas palestras, confirmações pendentes e alertas de recusa. |

---

## 2. Requisitos Não Funcionais (RNF)

| ID | Requisito Não Funcional | Critério de Aceitação |
| :--- | :--- | :--- |
| **RNF-01** | Desempenho | Tempo de resposta de páginas MVC < 500ms em condições normais de rede local. |
| **RNF-02** | Segurança de Segredos | Nenhuma credencial ou segredo em repositório; suporte a Docker Secrets e variáveis de ambiente. |
| **RNF-03** | Proteção OWASP | Aplicação protegida contra SQLi, XSS, CSRF e Broken Access Control. |
| **RNF-04** | Idempotência | Reprocessamento de webhook de WhatsApp não duplica confirmações ou envios. |
| **RNF-05** | Portabilidade | Execução via contêineres Docker em Linux com usuário não-root. |
