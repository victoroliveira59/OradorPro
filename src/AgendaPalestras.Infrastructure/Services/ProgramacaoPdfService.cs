using System.Globalization;
using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Application.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AgendaPalestras.Infrastructure.Services;

public class ProgramacaoPdfService : IProgramacaoPdfService
{
    static ProgramacaoPdfService()
    {
        // Define licença Community do QuestPDF para uso gratuito
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.UseSystemFonts = true;
        QuestPDF.Settings.ThrowOnMissingFontFamilies = false;
    }

    public byte[] GerarPdfProgramacaoFimDeSemana(
        string nomeCongregacao,
        int mes,
        int ano,
        IReadOnlyList<AgendaDto> agendamentos)
    {
        var cultura = new CultureInfo("pt-BR");
        var nomeMes = new DateTime(ano, mes, 1).ToString("MMMM", cultura);
        var mesFormatado = char.ToUpper(nomeMes[0]) + nomeMes.Substring(1);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                // Modo Retrato (Portrait) em folha A4 com margens otimizadas
                page.Size(PageSizes.A4);
                page.Margin(1.0f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9));

                // Cabeçalho do Documento
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(headerCol =>
                        {
                            headerCol.Item().Text("PROGRAMAÇÃO DE REUNIÃO DE FIM DE SEMANA")
                                .FontSize(15)
                                .ExtraBold()
                                .FontColor(Colors.Blue.Darken3);

                            headerCol.Item().Text(text =>
                            {
                                text.Span("Congregação: ").Bold().FontSize(9.5f);
                                text.Span(string.IsNullOrWhiteSpace(nomeCongregacao) ? "Geral / Todas" : nomeCongregacao).SemiBold().FontSize(9.5f);
                                text.Span("   |   Mês de Referência: ").Bold().FontSize(9.5f);
                                text.Span($"{mesFormatado} de {ano}").SemiBold().FontSize(9.5f);
                            });
                        });
                    });

                    col.Item().PaddingTop(4).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken2);
                    col.Item().PaddingBottom(8);
                });

                // Conteúdo Principal: Tabela de Programação em Formato Retrato
                page.Content().Table(table =>
                {
                    // Definição das Colunas para visualização retrato A4
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(75);   // Data e Horário
                        columns.RelativeColumn(2.5f); // Discurso Público (Nº e Tema na frente)
                        columns.RelativeColumn(2.0f); // Orador (Apenas Nome e Congregação)
                        columns.RelativeColumn(1.3f); // Presidente
                        columns.RelativeColumn(1.4f); // Leitor da Sentinela
                        columns.RelativeColumn(1.4f); // Oração Final com asterisco
                    });

                    // Cabeçalho da Tabela
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Data / Horário").Bold().FontSize(8.5f).FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Discurso Público").Bold().FontSize(8.5f).FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Orador").Bold().FontSize(8.5f).FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Presidente").Bold().FontSize(8.5f).FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Leitor de A Sentinela").Bold().FontSize(8.5f).FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Oração Final *").Bold().FontSize(8.5f).FontColor(Colors.White);
                    });

                    // Linhas da Tabela
                    if (agendamentos == null || agendamentos.Count == 0)
                    {
                        table.Cell().ColumnSpan(6).Padding(20).AlignCenter().Text("Nenhuma reunião agendada para este mês.")
                            .Italic().FontColor(Colors.Grey.Medium);
                    }
                    else
                    {
                        int index = 0;
                        foreach (var a in agendamentos.OrderBy(x => x.DataHora))
                        {
                            var bgColor = index % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                            // 1. Coluna Data / Hora
                            table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Column(c =>
                            {
                                c.Item().Text(a.DataHora.ToString("dd/MM/yyyy")).Bold().FontSize(8.5f);
                                c.Item().Text($"{a.DataHora:HH:mm} ({a.DataHora.ToString("dddd", cultura)})").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                            });

                            // 2. Coluna Discurso: Número e Tema na frente
                            table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(text =>
                            {
                                text.Span($"Nº {a.DiscursoNumero} — ").Bold().FontSize(8.5f).FontColor(Colors.Blue.Darken3);
                                text.Span(a.DiscursoTema).FontSize(8.5f);
                            });

                            // 3. Coluna Orador: Apenas Nome e Congregação (sem telefone)
                            table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Column(c =>
                            {
                                c.Item().Text(a.OradorNome).Bold().FontSize(8.5f);
                                if (!string.IsNullOrWhiteSpace(a.OradorCongregacaoOrigem))
                                {
                                    c.Item().Text(a.OradorCongregacaoOrigem).FontSize(8f).FontColor(Colors.Grey.Darken2);
                                }
                            });

                            // 4. Coluna Presidente
                            table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                .Text(string.IsNullOrWhiteSpace(a.PresidenteReuniao) ? "—" : a.PresidenteReuniao)
                                .FontSize(8.5f);

                            // 5. Coluna Leitor de A Sentinela
                            table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                .Text(string.IsNullOrWhiteSpace(a.LeitorSentinela) ? "—" : a.LeitorSentinela)
                                .FontSize(8.5f);

                            // 6. Coluna Oração Final
                            table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                .Text(string.IsNullOrWhiteSpace(a.OracaoFinal) ? "—" : a.OracaoFinal)
                                .FontSize(8.5f);

                            index++;
                        }
                    }
                });

                // Rodapé com Legenda do asterisco e numeração de páginas
                page.Footer().Column(col =>
                {
                    col.Item().PaddingBottom(4).Text("* Em caso do orador não permanecer até o final da reunião.")
                        .Italic().FontSize(8f).FontColor(Colors.Grey.Darken2);

                    col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                    col.Item().PaddingTop(3).Row(row =>
                    {
                        row.RelativeItem().Text(text =>
                        {
                            text.Span("Agenda de Palestras (OradorPro) — Gerado em: ").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                            text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(7.5f).Bold().FontColor(Colors.Grey.Darken1);
                        });

                        row.RelativeItem().AlignRight().Text(text =>
                        {
                            text.Span("Página ").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                            text.CurrentPageNumber().FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                            text.Span(" de ").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                            text.TotalPages().FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                        });
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}
