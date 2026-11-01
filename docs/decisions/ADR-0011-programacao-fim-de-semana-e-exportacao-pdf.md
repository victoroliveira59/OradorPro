# ADR-0011 — Programação de Fim de Semana e Exportação em PDF

- **Status:** ACCEPTED
- **Date:** 2026-11-01
- **Decision Owner:** Engenheiro / Analista
- **Technical Context:** Fase 12 — Programação de Fim de Semana e Exportação em PDF

---

## 1. Contexto e Problema
A congregação precisa organizar semanalmente a escala completa da reunião de fim de semana, que envolve não apenas o orador e o discurso público, mas também as partes complementares da reunião:
1. Tema do Discurso
2. Orador (Nome e Congregação de Origem)
3. Presidente da Reunião
4. Leitor de A Sentinela
5. Oração Final (e observações)

Além da tela de edição rápida dessas designações por semana no mês, a congregação necessita de um documento formatado em PDF intitulado "Programação da Reunião de Fim de Semana", pronto para impressão e envio aos publicadores e anciãos.

## 2. Decisão
1. **Extensão do Modelo de Dados (`Agenda`):**
   - Adicionar à entidade `Agenda` os campos:
     - `PresidenteReuniao` (string, opcional, max 150)
     - `LeitorSentinela` (string, opcional, max 150)
     - `OracaoFinal` (string, opcional, max 150)
   - Disponibilizar método de domínio `AtualizarDesignacoesReuniao(presidente, leitorSentinela, oracaoFinal)`.
   - Gerar migration no EF Core para atualização do schema.

2. **Biblioteca de Geração de PDF:**
   - Adotar **QuestPDF** (Community License) na camada de infraestrutura (`AgendaPalestras.Infrastructure`).
   - Criar serviço `IProgramacaoPdfService` na camada de Application com implementação `ProgramacaoPdfService` em Infrastructure.
   - O documento PDF terá formato A4 (ou Paisagem para tabela limpa), cabeçalho institucional com o nome da congregação, mês/ano, e tabela semanal contendo: Data/Hora, Discurso (Nº e Tema), Orador (Nome e Congregação), Presidente, Leitor e Oração Final.

3. **Menu e Interface Web (`ProgramacaoController`):**
   - Criar menu dedicado "Programação de Fim de Semana" no navbar.
   - Permitir selecionar Congregação e Mês/Ano.
   - Apresentar cada reunião do mês em cards/tabela editável para preenchimento rápido de Presidente, Leitor e Oração.
   - Botão de ação "Exportar PDF" que gera o download do arquivo `Programacao_FimDeSemana_[Mes]_[Ano].pdf`.

## 3. Consequências
- **Positivas:**
  - Atende integralmente à necessidade operacional dos coordenadores de reunião.
  - PDF gerado com alta fidelidade visual e layout profissional via QuestPDF sem dependências externas de browser ou wkhtmltopdf.
- **Negativas:**
  - Adiciona dependência de QuestPDF no projeto de infraestrutura.
