using System.Text;
using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Enums;
using AgendaPalestras.Infrastructure.Services;
using Xunit;

namespace AgendaPalestras.UnitTests;

public class ProgramacaoPdfServiceTests
{
    private readonly ProgramacaoPdfService _service;

    public ProgramacaoPdfServiceTests()
    {
        _service = new ProgramacaoPdfService();
    }

    [Fact]
    public void GerarPdfProgramacaoFimDeSemana_ComListaVazia_DeveGerarPdfValido()
    {
        // Arrange
        var nomeCongregacao = "Congregação Central";
        var mes = 11;
        var ano = 2026;
        var listaVazia = Array.Empty<AgendaDto>();

        // Act
        var bytes = _service.GerarPdfProgramacaoFimDeSemana(nomeCongregacao, mes, ano, listaVazia);

        // Assert
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 100);

        // Header padrão PDF deve iniciar com %PDF
        var header = Encoding.ASCII.GetString(bytes.Take(4).ToArray());
        Assert.Equal("%PDF", header);
    }

    [Fact]
    public void GerarPdfProgramacaoFimDeSemana_ComAgendamentos_DeveGerarPdfValido()
    {
        // Arrange
        var nomeCongregacao = "Belo Horizonte Sul";
        var mes = 12;
        var ano = 2026;

        var agendamentos = new List<AgendaDto>
        {
            new AgendaDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Belo Horizonte Sul",
                "Belo Horizonte",
                "MG-01",
                Guid.NewGuid(),
                "João Silva",
                "31999998888",
                "Central Contagem",
                Guid.NewGuid(),
                45,
                "Ache consolo para todo sofrimento",
                new DateTime(2026, 12, 6, 19, 0, 0),
                StatusPalestra.Confirmado,
                "Confirmado",
                "Obs teste",
                null,
                null,
                null,
                null,
                "Carlos Oliveira",
                "Marcos Souza",
                "Pedro Santos"),
            new AgendaDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Belo Horizonte Sul",
                "Belo Horizonte",
                "MG-01",
                Guid.NewGuid(),
                "Lucas Lima",
                "31988887777",
                "Betim",
                Guid.NewGuid(),
                92,
                "A religião tem futuro?",
                new DateTime(2026, 12, 13, 19, 0, 0),
                StatusPalestra.Pendente,
                "Pendente",
                null,
                null,
                null,
                null,
                null,
                "José Pereira",
                "Felipe Rocha",
                "Observação: Levar microfone volante")
        };

        // Act
        var bytes = _service.GerarPdfProgramacaoFimDeSemana(nomeCongregacao, mes, ano, agendamentos);

        // Assert
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 500);

        var header = Encoding.ASCII.GetString(bytes.Take(4).ToArray());
        Assert.Equal("%PDF", header);
    }

    [Fact]
    public void Agenda_AtualizarDesignacoesReuniao_DeveAtualizarPropriedades()
    {
        // Arrange
        var agenda = new Agenda(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(7));

        // Act
        agenda.AtualizarDesignacoesReuniao("Presidente Teste", "Leitor Teste", "Oracao Teste");

        // Assert
        Assert.Equal("Presidente Teste", agenda.PresidenteReuniao);
        Assert.Equal("Leitor Teste", agenda.LeitorSentinela);
        Assert.Equal("Oracao Teste", agenda.OracaoFinal);
        Assert.NotNull(agenda.AtualizadoEm);
    }
}
