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
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                // Cabeçalho do Documento
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(headerCol =>
                        {
                            headerCol.Item().Text("PROGRAMAÇÃO DE REUNIÃO DE FIM DE SEMANA")
                                .FontSize(18)
                                .ExtraBold()
                                .FontColor(Colors.Blue.Darken3);

                            headerCol.Item().Text(text =>
                            {
                                text.Span("Congregação: ").Bold();
                                text.Span(string.IsNullOrWhiteSpace(nomeCongregacao) ? "Geral / Todas" : nomeCongregacao).SemiBold();
                                text.Span("   |   Mês de Referência: ").Bold();
                                text.Span($"{mesFormatado} de {ano}").SemiBold();
                            });
                        });
                    });

                    col.Item().PaddingTop(5).LineHorizontal(1.5f).LineColor(Colors.Blue.Darken2);
                    col.Item().PaddingBottom(10);
                });

                // Conteúdo Principal: Tabela de Programação
                page.Content().Table(table =>
                {
                    // Definição das Colunas
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(85);   // Data e Horário
                        columns.RelativeColumn(3);    // Tema e Nº do Discurso
                        columns.RelativeColumn(2.2f); // Orador e Congregação
                        columns.RelativeColumn(1.8f); // Presidente
                        columns.RelativeColumn(1.8f); // Leitor da Sentinela
                        columns.RelativeColumn(1.8f); // Oração Final
                    });

                    // Cabeçalho da Tabela
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken3).Padding(6).Text("Data / Horário").Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(6).Text("Discurso Público").Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(6).Text("Orador").Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(6).Text("Presidente").Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(6).Text("Leitor de A Sentinela").Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken3).Padding(6).Text("Oração Final").Bold().FontColor(Colors.White);
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

                            // Coluna Data / Hora
                            table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                            {
                                c.Item().Text(a.DataHora.ToString("dd/MM/yyyy")).Bold().FontSize(10);
                                c.Item().Text($"{a.DataHora:HH:mm} ({a.DataHora.ToString("dddd", cultura)})").FontSize(8.5f).FontColor(Colors.Grey.Darken1);
                            });

                            // Coluna Discurso
                            table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                            {
                                c.Item().Text($"Nº {a.DiscursoNumero}").Bold().FontSize(9).FontColor(Colors.Blue.Darken2);
                                c.Item().Text(a.DiscursoTema).SemiBold().FontSize(9.5f);
                            });

                            // Coluna Orador
                            table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(c =>
                            {
                                c.Item().Text(a.OradorNome).Bold().FontSize(9.5f);
                                if (!string.IsNullOrWhiteSpace(a.OradorCongregacaoOrigem))
                                {
                                    c.Item().Text($"Congr. {a.OradorCongregacaoOrigem}").FontSize(8.5f).FontColor(Colors.Grey.Darken2);
                                }
                                if (!string.IsNullOrWhiteSpace(a.OradorTelefone))
                                {
                                    c.Item().Text(a.OradorTelefone).FontSize(8).FontColor(Colors.Grey.Darken1);
                                }
                            });

                            // Coluna Presidente
                            table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6)
                                .Text(string.IsNullOrWhiteSpace(a.PresidenteReuniao) ? "—" : a.PresidenteReuniao)
                                .FontSize(9.5f);

                            // Coluna Leitor da Sentinela
                            table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6)
                                .Text(string.IsNullOrWhiteSpace(a.LeitorSentinela) ? "—" : a.LeitorSentinela)
                                .FontSize(9.5f);

                            // Coluna Oração Final
                            table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6)
                                .Text(string.IsNullOrWhiteSpace(a.OracaoFinal) ? "—" : a.OracaoFinal)
                                .FontSize(9.5f);

                            index++;
                        }
                    }
                });

                // Rodapé
                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                    col.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem().Text(text =>
                        {
                            text.Span("Agenda de Palestras (OradorPro) — Gerado em: ").FontSize(8).FontColor(Colors.Grey.Darken1);
                            text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                        });

                        row.RelativeItem().AlignRight().Text(text =>
                        {
                            text.Span("Página ").FontSize(8).FontColor(Colors.Grey.Darken1);
                            text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                            text.Span(" de ").FontSize(8).FontColor(Colors.Grey.Darken1);
                            text.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}
