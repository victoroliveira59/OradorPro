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

---

## 4. Isolamento Multi-Tenant por Congregação e Usuário

- **RN-14 (Visibilidade e Gestão de Congregações):** O usuário regular (Admin/Coordenador) só visualiza, lista e gerencia a SUA própria congregação (vinculada ao seu usuário via `Usuario.CongregacaoId`) e congregações adicionais cadastradas por ele (`c.UsuarioCriadorId == usuario.Id`). Ele não visualiza nem altera congregações cadastradas por outros usuários.
- **RN-15 (Isolamento de Oradores):** O usuário regular só visualiza, lista e gerencia os oradores pertencentes à sua congregação ou criados por ele (`o.CongregacaoId == usuario.CongregacaoId || o.UsuarioCriadorId == usuario.Id || o.Congregacao.UsuarioCriadorId == usuario.Id`). Oradores pertencentes a outras congregações e criados por outros usuários ficam totalmente inacessíveis.
- **RN-16 (Agenda e Programação Restritas à Congregação):** O usuário regular só visualiza, cria, edita, cancela palestras e define designações de reunião para a congregação à qual pertence (`Agenda.CongregacaoId == usuario.CongregacaoId`). Tentativas de acessar ID de palestras de outra congregação devem ser bloqueadas com `404 Not Found` ou `403 Forbidden`.
- **RN-17 (Catálogo Oficial Público de Discursos S-34):** O catálogo de 194 temas de discursos públicos (S-34) é global e compartilhado para todos os usuários consultarem e utilizarem no repertório dos oradores e agendamentos. Apenas o perfil SuperAdmin possui permissão para incluir ou editar temas no catálogo oficial.
- **RN-18 (Visão Global do SuperAdmin):** O SuperAdmin possui privilégio irrestrito de visualizar e gerenciar congregações, oradores, agendas, programações e instâncias de WhatsApp de todos os usuários da plataforma.
