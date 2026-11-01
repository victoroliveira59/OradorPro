using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Exceptions;
using Xunit;

namespace AgendaPalestras.UnitTests;

public class DiscursoTests
{
    [Fact]
    public void CriarDiscurso_ComDadosValidos_DeveInstanciarComSucesso()
    {
        var d = new Discurso(1, "Quem é Deus?");

        Assert.NotEqual(Guid.Empty, d.Id);
        Assert.Equal(1, d.Numero);
        Assert.Equal("Quem é Deus?", d.Tema);
        Assert.True(d.Ativo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CriarDiscurso_ComNumeroInvalido_DeveLancarExcecao(int numeroInvalido)
    {
        Assert.Throws<DomainException>(() => new Discurso(numeroInvalido, "Tema Teste"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CriarDiscurso_ComTemaVazio_DeveLancarExcecao(string temaInvalido)
    {
        Assert.Throws<DomainException>(() => new Discurso(10, temaInvalido));
    }
}
