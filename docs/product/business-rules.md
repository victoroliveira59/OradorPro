# Regras de Negócio — Agenda de Palestras

> Alinhamento com SPEC.md Seção 3.2.  
> As regras abaixo são invariantes e devem ser respeitadas em todas as camadas.

---

## 1. Cadastro e Entidades

- **RN-01 (Vínculo de Congregação e Orador):** Uma congregação pode possuir vários oradores cadastrados, porém cada orador pertence a exatamente uma única congregação de origem. O cadastro é administrado centralmente pelo usuário Admin.
- **RN-02 (Orador Inativo):** Um orador com status inativo não pode ser selecionado para novos agendamentos de palestras.
- **RN-03 (Discurso Inativo):** Um tema de discurso com status inativo não pode ser vinculado a uma nova palestra.
- **RN-03A (Repertório de Discursos do Orador):** Um orador pode estar habilitado para um ou múltiplos temas de discurso (repertório). Ao agendar uma palestra na congregação para aquele orador, o sistema deve permitir a seleção apenas dos discursos ativos que constam no repertório do orador selecionado.
- **RN-13 (Administração Centralizada):** O sistema opera sob administração centralizada: um usuário administrador autenticado cria e gerencia múltiplas congregações no banco de dados e cadastra os oradores vinculados a cada congregação.

---

## 2. Agendamento e Conflitos

- **RN-04 (Composição da Agenda):** O cadastro da agenda exige obrigatoriamente a associação de: Data/Hora, Congregação anfitriã, Orador convidado e Discurso a ser proferido.
- **RN-05 (Regra de Unicidade de Agenda):** O sistema deve impedir conflito de agenda (um orador não pode proferir duas palestras no mesmo dia/horário em congregações diferentes, e uma congregação não pode ter duas palestras simultâneas). A granularidade exata (global vs por congregação) será formalizada no ADR-0004.
- **RN-06 (Concorrência de Agendamento):** O sistema deve utilizar controle de concorrência pessimista ou otimista com constraint única no banco de dados para evitar reservas duplicadas no mesmo milissegundo.

---

## 3. Comunicação via WhatsApp e Automações

- **RN-07 (Disparo de Convite):** Concluído o cadastro da palestra com sucesso, o sistema deve registrar e enfileirar o envio imediato do convite via WhatsApp para o telefone do orador.
- **RN-08 (Semântica da Resposta do Orador):**
  - A resposta `1` (ou texto equivalente pré-definido) representa Confirmação da palestra.
  - A resposta `2` (ou texto equivalente pré-definido) representa Recusa da palestra.
  - Respostas fora desse padrão devem ser registradas no histórico e marcadas para análise manual pelo administrador.
- **RN-09 (Validação de Contexto do Webhook):** Uma mensagem recebida via webhook só altera o status da agenda se o telefone remetente coincidir com o telefone do orador agendado e a palestra estiver aguardando confirmação.
- **RN-10 (Janela de Lembrete de 7 Dias):** Exatamente sete dias antes da data da palestra, um job em background deve avaliar a necessidade de disparo de mensagem de lembrete.
- **RN-11 (Prevenção de Duplicidade de Envios):** O sistema deve garantir idempotência estrita: é expressamente proibido enviar convites duplicados ou lembretes repetidos para o mesmo evento.
- **RN-12 (Auditoria e Histórico de Comunicação):** Toda interação (convite enviado, resposta recebida, webhook com erro, lembrete emitido) deve ser gravada em tabela de histórico com payload, data/hora e status.
