# Especificação de Requisitos — Layout e Responsividade Mobile (Fase 13)

> Documento de referência para modernização da interface do usuário com foco em dispositivos móveis (smartphones e tablets).

---

## 1. Visão Geral e Objetivos

O sistema **Agenda de Palestras** é utilizado frequentemente por coordenadores e anciãos em trânsito através de smartphones (iOS e Android). O objetivo desta fase é proporcionar uma experiência de usuário (UX) de alta qualidade em telas compactas (320px a 768px), eliminando barras de rolagem horizontal desnecessárias, amontoamento de botões e tabelas ilegíveis em telas estreitas.

---

## 2. Requisitos de Interface por Módulo

### 2.1. Layout Global e Navegação (`_Layout.cshtml` e `site.css`)
- **RF-MOB-01 (Viewport & Touch):** Garantir meta tag de viewport configurada e áreas de toque mínimas de 44x44px nos links e botões interativos.
- **RF-MOB-02 (Navbar Responsiva):** O menu superior deve colapsar suavemente em telas menores que `lg`/`md`. Links com ícones visíveis e alinhamento vertical confortável.
- **RF-MOB-03 (Espaçamento de Contêiner):** Em telas pequenas, utilizar padding reduzido (`px-2 px-sm-3`) para maximizar a área útil de visualização.
- **RF-MOB-04 (Tipografia Fluida):** Títulos principais (`h1`, `h2`) e subtítulos devem reduzir proporcionalmente no mobile para evitar quebras de linha que empurrem o conteúdo.

### 2.2. Agenda de Palestras (`Agenda/Index.cshtml` e `Agenda/Details.cshtml`)
- **RF-MOB-05 (Calendário Adaptativo no Mobile):**
  - Em telas `>= 768px`: manter a grade mensal tradicional de 7 colunas.
  - Em telas `< 768px`: renderizar automaticamente uma **visão de cartões por dia/semana** ou timeline vertical, onde cada evento agendado exibe card completo com tema, orador, congregação, status e botões de ação direta.
- **RF-MOB-06 (Filtros Empilhados):** Seletores de Mês, Ano e Congregação devem ocupar 100% da largura em mobile (`col-12 col-sm-auto`), com navegação rápida de mês anterior/próximo acessível via toque.
- **RF-MOB-07 (Ações de Detalhes):** Em `Agenda/Details.cshtml`, os botões de ação ("Reenviar Convite", "Confirmar", "Reagendar", "Cancelar") devem se dispor em blocos empilhados ou largura total no mobile para facilitar o toque com o polegar.

### 2.3. Programação de Fim de Semana (`Programacao/Index.cshtml`)
- **RF-MOB-08 (Formulário Semanal em Coluna):** Em telas `< 992px`, os campos de designação (Presidente, Leitor, Oração) devem empilhar verticalmente em `col-12`, facilitando a digitação no teclado virtual sem deslocamento de tela.
- **RF-MOB-09 (Barra de Ação Fixa/Destacada):** Botão "Salvar Todas as Designações" e botão "Exportar PDF" visíveis e acessíveis sem rolagem excessiva.

### 2.4. Cadastros de Congregações, Oradores, Discursos e Convites
- **RF-MOB-10 (Tabelas com Modo Cartão Mobile):**
  - Implementar visualização híbrida: em telas mobile (`d-md-none`), exibir os itens como cartões individuais com cabeçalho, badges de status e botões de ação em largura total. Em desktop (`d-none d-md-block`), manter as tabelas completas.
- **RF-MOB-11 (Ações Rápidas de Contato):** No card de Oradores, botão direto para iniciar conversa no WhatsApp (`wa.me`) com destaque verde.

### 2.5. Formulários de Criação e Edição
- **RF-MOB-12 (Formulários Verticais):** Inputs com espaçamento confortável (`mb-3`), sem grids horizontais comprimidos.
- **RF-MOB-13 (Botões de Ação):** Botões "Salvar", "Cancelar" e "Voltar" ocupando largura total no mobile (`d-grid gap-2 d-sm-flex`).

---

## 3. Critérios de Aceite (DoD da Fase)

1. **CA-01:** Nenhuma página da aplicação apresenta overflow horizontal indesejado em viewport de 360px a 414px.
2. **CA-02:** Menu navbar abre e fecha corretamente no mobile com todos os links acessíveis (Agenda, Programação, Congregações, Oradores, Discursos, WhatsApp, Convites).
3. **CA-03:** A tela da Agenda possui visualização em cartões/timeline em telas móveis com indicação clara dos discursos agendados.
4. **CA-04:** Formulário de programação de fim de semana é facilmente preenchível em dispositivo móvel com campos verticais.
5. **CA-05:** Oradores, congregações e discursos contam com cartões mobile touch-friendly para consulta e edição.
6. **CA-06:** Todos os 92 testes unitários e de integração existentes continuam passando sem regressão.
