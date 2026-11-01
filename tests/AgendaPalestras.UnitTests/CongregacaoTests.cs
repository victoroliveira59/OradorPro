using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Exceptions;
using Xunit;

namespace AgendaPalestras.UnitTests;

public class CongregacaoTests
{
    [Fact]
    public void CriarCongregacao_ComDadosValidos_DeveInstanciarComSucesso()
    {
        var c = new Congregacao("Central", "São Paulo", "SP-01", "Domingo", "18:00");

        Assert.NotEqual(Guid.Empty, c.Id);
        Assert.Equal("Central", c.Nome);
        Assert.Equal("São Paulo", c.Cidade);
        Assert.True(c.Ativo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CriarCongregacao_ComNomeInvalido_DeveLancarExcecao(string nomeInvalido)
    {
        Assert.Throws<DomainException>(() => new Congregacao(nomeInvalido, "Cidade", "Circuito", "Domingo", "18:00"));
    }

    [Fact]
    public void DesativarCongregacao_DeveAlterarStatusParaInativo()
    {
        var c = new Congregacao("Jardim", "Campinas", "SP-02", "Sábado", "19:00");
        c.Desativar();

        Assert.False(c.Ativo);
        Assert.NotNull(c.AtualizadoEm);
    }
}
