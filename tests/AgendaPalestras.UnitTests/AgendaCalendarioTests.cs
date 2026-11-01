using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Domain.Enums;
using Xunit;

namespace AgendaPalestras.UnitTests;

public class AgendaCalendarioTests
{
    [Fact]
    public void AgendaCalendarioViewModel_EstruturaDeDias_DeveIdentificarSePertenceAoMesEEhHoje()
    {
        var hoje = DateTime.Today;
        var diaAtual = new AgendaDiaCalendarioDto
        {
            Data = hoje,
            PertenceAoMesAtual = true,
            EhHoje = true
        };

        var outroDia = new AgendaDiaCalendarioDto
        {
            Data = hoje.AddMonths(-1),
            PertenceAoMesAtual = false,
            EhHoje = false
        };

        Assert.Equal(hoje.Day, diaAtual.Dia);
        Assert.True(diaAtual.EhHoje);
        Assert.True(diaAtual.PertenceAoMesAtual);
        Assert.False(diaAtual.TemPalestra);

        Assert.False(outroDia.EhHoje);
        Assert.False(outroDia.PertenceAoMesAtual);
    }

    [Fact]
    public void AgendaDiaCalendarioDto_ComPalestras_DeveIndicarTemPalestra()
    {
        var dia = new AgendaDiaCalendarioDto
        {
            Data = DateTime.Today,
            PertenceAoMesAtual = true
        };

        var palestra = new AgendaDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Congregação Teste",
            "Cidade Teste",
            "SP-01",
            Guid.NewGuid(),
            "Orador Teste",
            "11988887777",
            "Congregação Origem",
            Guid.NewGuid(),
            10,
            "Tema de Teste",
            DateTime.UtcNow.AddDays(2),
            StatusPalestra.Confirmado,
            "Confirmado",
            "Observações",
            null,
            null,
            null,
            null
        );

        dia.Palestras.Add(palestra);

        Assert.True(dia.TemPalestra);
        Assert.Single(dia.Palestras);
        Assert.Equal("Orador Teste", dia.Palestras[0].OradorNome);
    }
}
