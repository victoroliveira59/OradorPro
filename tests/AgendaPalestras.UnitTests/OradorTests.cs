using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Exceptions;
using AgendaPalestras.Domain.Interfaces;
using Moq;
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

    [Fact]
    public async Task CriarAsync_ComNovaCongregacaoNome_DeveCriarCongregacaoEOrador()
    {
        var mockOradorRepo = new Moq.Mock<IOradorRepository>();
        var mockCongRepo = new Moq.Mock<ICongregacaoRepository>();
        var mockDiscursoRepo = new Moq.Mock<IDiscursoRepository>();
        var validator = new AgendaPalestras.Application.Validators.CriarOradorValidator();

        mockCongRepo.Setup(r => r.ListarTodasAsync(false, Moq.It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Congregacao>());

        var service = new AgendaPalestras.Application.Services.OradorService(
            mockOradorRepo.Object,
            mockCongRepo.Object,
            mockDiscursoRepo.Object,
            validator
        );

        var dto = new AgendaPalestras.Application.DTOs.CriarOradorDto(
            Guid.Empty,
            "Irmão Visitante",
            "11988887777",
            "visitante@email.com",
            NovaCongregacaoNome: "Congregação Floresta"
        );

        var resultado = await service.CriarAsync(dto);

        Assert.NotNull(resultado);
        Assert.Equal("Irmão Visitante", resultado.Nome);
        Assert.Equal("Congregação Floresta", resultado.CongregacaoNome);

        mockCongRepo.Verify(r => r.AdicionarAsync(
            Moq.It.Is<Congregacao>(c => c.Nome == "Congregação Floresta"),
            Moq.It.IsAny<CancellationToken>()), Moq.Times.Once);

        mockOradorRepo.Verify(r => r.AdicionarAsync(
            Moq.It.Is<Orador>(o => o.Nome == "Irmão Visitante"),
            Moq.It.IsAny<CancellationToken>()), Moq.Times.Once);
    }
}
