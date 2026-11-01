# ADR-0009 — Conexão do WhatsApp do Admin via Evolution API e QR Code

- **Status:** ACCEPTED
- **Date:** 2026-11-01
- **Decision Owner:** Engenheiro / Arquiteto
- **Technical Context:** Fase 10 — Conexão WhatsApp Admin (Evolution API e QR Code)

---

## 1. Context (Contexto e Problema)
O sistema necessita enviar convites e lembretes automáticos para os oradores a partir do WhatsApp oficial do administrador da plataforma. Para isso, o administrador precisa conseguir conectar seu número pessoal ou institucional através de leitura de QR Code diretamente na interface web da aplicação, sem exigir configurações manuais em arquivos ou ferramentas externas de terceiros.

A plataforma utiliza a Evolution API v2 como gateway de WhatsApp, que fornece endpoints REST oficiais para ciclo de vida de instâncias, geração de QR Code, pareamento via código numérico e envio de mensagens.

## 2. Options Considered (Alternativas Avaliadas)
- **Opção 1 (Configuração estática em .env sem interface):** O administrador precisaria usar Postman ou painel externo para escanear o QR code e colar tokens no `.env`. Péssima experiência de usuário e inviável para administradores leigos.
- **Opção 2 (Módulo Nativo de Gerenciamento da Conexão WhatsApp na Plataforma):**
  - Endpoint `IEvolutionApiService` na camada de Aplicação e adaptadores na Infraestrutura.
  - Tela administrativa protegida por política `AdminOnly`.
  - Integração com a Evolution API v2 para criar a instância com engine `WHATSAPP-BAILEYS`, obter status (`/instance/connectionState/{instance}`), obter QR Code em base64 e código de pareamento (`/instance/connect/{instance}`), logout (`/instance/logout/{instance}`) e envio de texto v2 (`/message/sendText/{instance}`).
  - Persistência das credenciais da instância (URL, API Key e Nome da Instância) na tabela `ConfiguracoesSistema` com fallback transparente para variáveis de ambiente.

## 3. Decision (Decisão)
Adotar a **Opção 2**:
1. **Entidade `ConfiguracaoSistema`:** Tabela no PostgreSQL para armazenar parâmetros do sistema (URL da API, API Key e Instância ativa do WhatsApp).
2. **Serviço `EvolutionApiService`:**
   - Consulta de status de conexão: `GET /instance/connectionState/{instance}`.
   - Criação de instância sob demanda: `POST /instance/create` com `integration = "WHATSAPP-BAILEYS"` e `qrcode = true`.
   - Obtenção do QR Code e Pairing Code: `GET /instance/connect/{instance}`.
   - Desconexão segura: `DELETE /instance/logout/{instance}`.
   - Envio de mensagens formato Evolution API v2: `POST /message/sendText/{instance}` com payload `{ number, text, delay }`.
3. **Painel Web do Admin (`/WhatsApp`):**
   - Exibição de status em tempo real (Conectado / Desconectado / Aguardando QR Code).
   - Renderização visual do QR Code na tela com atualização automática (polling).
   - Formulário de configuração de credenciais da Evolution API.
   - Teste de envio interativo para validação imediata da conectividade.
4. **Automações de Convite e Lembrete:**
   - O `WhatsAppNotificationService` e o job diário `LembretePalestrasBackgroundJob` passam a utilizar a instância ativa do WhatsApp conectada pelo administrador.

## 4. Consequences (Consequências)
- **Positivas:** Experiência de uso fluida para o administrador, conexão em segundos via QR Code ou Pairing Code pelo celular, isolamento arquitetural Clean Architecture, alta testabilidade.
- **Negativas:** Exige sincronização de chamadas HTTP com a Evolution API e criação de tabela de configurações no banco.
