# Modelo de Dados Conceitual e Lógico — Agenda de Palestras

> Alinhamento com SPEC.md Seção 3.1, 3.2 e Fase 02.  
> As definições físicas de banco (PostgreSQL vs SQL Server) serão consolidadas na Fase 02.

---

## 1. Diagrama Entidade-Relacionamento Conceitual

```mermaid
erDiagram
    USUARIO ||--o{ CONGREGACAO : "cria e gerencia"
    CONGREGACAO ||--o{ ORADOR : "possui"
    CONGREGACAO ||--o{ AGENDA : "hospeda"
    ORADOR ||--o{ AGENDA : "ministra"
    DISCURSO ||--o{ AGENDA : "tema de"
    ORADOR ||--o{ ORADOR_DISCURSO : "tem repertório de"
    DISCURSO ||--o{ ORADOR_DISCURSO : "habilitado em"
    AGENDA ||--o{ MENSAGEM_HISTORICO : "gera"

    USUARIO {
        uuid id PK
        string nome
        string email
        string hash_senha
        string papel
        boolean ativo
        datetime criado_em
    }

    CONGREGACAO {
        uuid id PK
        uuid usuario_criador_id FK
        string nome
        string cidade
        string circuito
        string dias_reuniao
        string horario_reuniao
        boolean ativo
        datetime criado_em
    }

    ORADOR {
        uuid id PK
        uuid congregacao_id FK
        string nome
        string telefone_whatsapp
        string email
        boolean ativo
        datetime criado_em
    }

    DISCURSO {
        uuid id PK
        int numero
        string tema
        boolean ativo
        datetime criado_em
    }

    ORADOR_DISCURSO {
        uuid id PK
        uuid orador_id FK
        uuid discurso_id FK
        datetime criado_em
    }

    AGENDA {
        uuid id PK
        uuid congregacao_id FK
        uuid orador_id FK
        uuid discurso_id FK
        datetime data_hora
        string status_confirmacao
        datetime data_convite_enviado
        datetime data_lembrete_enviado
        datetime criado_em
    }

    MENSAGEM_HISTORICO {
        uuid id PK
        uuid agenda_id FK
        string tipo_mensagem
        string telefone_destino
        string payload_enviado
        string payload_recebido
        string status_entrega
        datetime data_evento
    }
```

---

## 2. Invariantes de Banco de Dados

1. **Índice Único de Agenda:** Constraint única impedindo que o mesmo orador possua dois agendamentos no mesmo horário/dia.
2. **Número de Discurso Único:** Constraint única para o número do discurso.
3. **Telefone Padronizado:** Formato internacional E.164 (ex: `+5511999998888`) para compatibilidade com APIs de WhatsApp.
4. **Deleção Restrita:** `DeleteBehavior.Restrict` em todas as relações para prevenir perda inadvertida de histórico de agendamentos.
5. **Repertório Único por Orador:** Constraint única composta em `(orador_id, discurso_id)` na tabela `ORADOR_DISCURSO` para impedir duplicações no repertório do mesmo orador.
6. **Consistência de Repertório na Agenda:** Ao registrar uma agenda, o discurso selecionado deve constar no repertório ativo do orador escolhido.
