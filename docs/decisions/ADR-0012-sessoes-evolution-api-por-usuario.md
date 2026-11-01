# ADR-0012 — Configuração Multi-usuário da Evolution API / Sessões por Usuário

- **Status:** ACCEPTED
- **Date:** 2026-11-01
- **Decision Owner:** Engenheiro / Analista / Maestro
- **Technical Context:** Fase 13 — Configuração e Gestão de Sessões da Evolution API por Usuário

---

## 1. Contexto e Problema
A plataforma OradorPro opera como um SaaS multi-usuário onde múltiplos coordenadores/administradores gerenciam suas respectivas congregações.
- Os usuários comuns não possuem (e não devem possuir) acesso direto ao servidor da Evolution API, nem conhecimento de URLs internas ou chaves mestras de API (Global ApiKey).
- Cada usuário/congregação deve ter sua própria sessão (instância) isolada no WhatsApp para envio de convites e recebimento de respostas de oradores.
- O Super Administrador é o responsável por provisionar a instância na Evolution API e associar o nome da sessão ao respectivo usuário no sistema.
- A tela de conexão do WhatsApp (com status e QR Code) só deve ser exibida/habilitada para o usuário caso o Super Admin já tenha configurado sua instância.
- Os parâmetros sensíveis (URL do servidor Evolution e Global ApiKey) e a configuração de webhooks devem ficar restritos exclusivamente à visão do Super Admin.

---

## 2. Decisão

1. **Modelagem de Dados (`Usuario`):**
   - Adicionar à entidade `Usuario`:
     - `EvolutionInstanceName` (varchar 100, opcional): Nome identificador único da instância na Evolution API.
     - `EvolutionApiUrl` (varchar 250, opcional): Sobrescrita da URL base caso utilize servidor dedicado (se nulo, usa a URL global do sistema).
     - `EvolutionApiKey` (varchar 250, opcional): Sobrescrita da chave de API (se nulo, usa a chave global do sistema).
   - Adicionar método de domínio `ConfigurarEvolution(instanceName, apiUrl, apiKey)`.
   - Gerar migration no EF Core.

2. **Serviço da Evolution API Multi-instância (`IEvolutionApiService`):**
   - Adicionar suporte a resolução de instância por usuário (`ObterConfiguracaoPorUsuarioAsync`).
   - Permitir que os métodos de conexão (`ObterStatusConexaoAsync`, `ObterOuGerarQrCodeAsync`, `DesconectarAsync`, `EnviarTextoAsync`) recebam o nome da instância específica do usuário autenticado.
   - Adicionar método `CriarInstanciaAsync` para permitir que o Super Admin crie a instância na Evolution API diretamente pela interface.

3. **Interface e Permissões no Web MVC (`WhatsAppController`):**
   - **Visão do Super Admin:**
     - Acesso aos parâmetros globais do servidor Evolution API e Webhook.
     - Acesso à gestão de usuários, permitindo visualizar todos os usuários, configurar suas instâncias do WhatsApp e acionar a criação na Evolution API.
   - **Visão do Usuário Regular (Admin):**
     - Ocultação total de campos sensíveis (URL e API Key).
     - Se possuir instância configurada: Exibe apenas o card com status da sua conexão, botão para Conectar / Gerar QR Code, Desconectar e disparo de mensagem de teste.
     - Se NÃO possuir instância configurada: Exibe alerta amigável informando que a sessão do WhatsApp aguarda ativação pelo Super Administrador.

---

## 3. Consequências
- **Positivas:**
  - Isolamento multi-tenant seguro das conexões de WhatsApp por congregação/usuário.
  - Segurança aprimorada: credenciais da Evolution API protegidas contra vazamento para usuários comuns.
  - Experiência do usuário simplificada e focada apenas no escaneamento do QR Code.
- **Negativas:**
  - Requer que o Super Admin configure a instância antes que o usuário possa conectar o WhatsApp.
