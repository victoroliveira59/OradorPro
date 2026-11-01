using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Exceptions;
using Xunit;

namespace AgendaPalestras.UnitTests;

public class OradorTests
{
    [Fact]
    public void CriarOrador_ComDadosValidos_DeveFormatarTelefoneEInstanciar()
    {
        var congId = Guid.NewGuid();
        var orador = new Orador(congId, "Carlos Souza", "(11) 98888-7777", "carlos@email.com");

        Assert.NotEqual(Guid.Empty, orador.Id);
        Assert.Equal(congId, orador.CongregacaoId);
        Assert.Equal("Carlos Souza", orador.Nome);
        Assert.Equal("11988887777", orador.TelefoneWhatsapp);
        Assert.True(orador.Ativo);
        Assert.Empty(orador.OradorDiscursos);
    }

    [Fact]
    public void CriarOrador_SemCongregacao_DeveLancarExcecao()
    {
        Assert.Throws<DomainException>(() => new Orador(Guid.Empty, "Nome", "11988887777"));
    }

    [Fact]
    public void Orador_PodeTerMaisDeUmDiscursoNoRepertorio()
    {
        var congId = Guid.NewGuid();
        var orador = new Orador(congId, "Marcos Paulo", "11999998888");

        var discurso1Id = Guid.NewGuid();
        var discurso2Id = Guid.NewGuid();
        var discurso3Id = Guid.NewGuid();

        orador.AdicionarDiscurso(discurso1Id);
        orador.AdicionarDiscurso(discurso2Id);
        orador.AdicionarDiscurso(discurso3Id);

        Assert.Equal(3, orador.OradorDiscursos.Count);
        Assert.True(orador.PossuiDiscurso(discurso1Id));
        Assert.True(orador.PossuiDiscurso(discurso2Id));
        Assert.True(orador.PossuiDiscurso(discurso3Id));
    }

    [Fact]
    public void AdicionarDiscurso_NaoDevePermitirDuplicidadeNoMesmoOrador()
    {
        var orador = new Orador(Guid.NewGuid(), "Marcos Paulo", "11999998888");
        var discursoId = Guid.NewGuid();

        orador.AdicionarDiscurso(discursoId);
        orador.AdicionarDiscurso(discursoId); // Duplicado

        Assert.Single(orador.OradorDiscursos);
    }

    [Fact]
    public void RemoverDiscurso_DeveExcluirDoRepertorio()
    {
        var orador = new Orador(Guid.NewGuid(), "Marcos Paulo", "11999998888");
        var discursoId = Guid.NewGuid();

        orador.AdicionarDiscurso(discursoId);
        Assert.True(orador.PossuiDiscurso(discursoId));

        orador.RemoverDiscurso(discursoId);
        Assert.False(orador.PossuiDiscurso(discursoId));
        Assert.Empty(orador.OradorDiscursos);
    }

    [Fact]
    public void SincronizarRepertorio_DeveAtualizarDiscursosEmLote()
    {
        var orador = new Orador(Guid.NewGuid(), "Marcos Paulo", "11999998888");
        var d1 = Guid.NewGuid();
        var d2 = Guid.NewGuid();
        var d3 = Guid.NewGuid();

        orador.AdicionarDiscurso(d1);
        orador.AdicionarDiscurso(d2);

        // Novo conjunto desejado: d2 e d3 (d1 é removido, d3 é adicionado)
        orador.SincronizarRepertorio(new[] { d2, d3 });

        Assert.Equal(2, orador.OradorDiscursos.Count);
        Assert.False(orador.PossuiDiscurso(d1));
        Assert.True(orador.PossuiDiscurso(d2));
        Assert.True(orador.PossuiDiscurso(d3));
    }
}
