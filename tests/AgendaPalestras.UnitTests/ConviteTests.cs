using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Application.Services;
using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Exceptions;
using AgendaPalestras.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgendaPalestras.UnitTests;

public class ConviteTests
{
    private readonly Mock<IConviteCadastroRepository> _mockConviteRepo = new();
    private readonly Mock<IUsuarioRepository> _mockUsuarioRepo = new();
    private readonly Mock<ILogger<ConviteService>> _mockLogger = new();

    private ConviteService CriarService() =>
        new(_mockConviteRepo.Object, _mockUsuarioRepo.Object, _mockLogger.Object);

    [Fact]
    public void ConviteCadastro_QuandoCriado_DeveEstarValidoENaoUsado()
    {
        var criadorId = Guid.NewGuid();
        var convite = new ConviteCadastro(criadorId, "teste@email.com", 7);

        Assert.NotNull(convite.Codigo);
        Assert.NotEmpty(convite.Codigo);
        Assert.False(convite.Usado);
        Assert.True(convite.EstaValido());
        Assert.Equal("teste@email.com", convite.EmailConvidado);
    }

    [Fact]
    public void ConviteCadastro_MarcarComoUsado_DeveAtualizarStatusEDataUso()
    {
        var convite = new ConviteCadastro(Guid.NewGuid(), "teste@email.com", 7);
        var novoUsuarioId = Guid.NewGuid();

        convite.MarcarComoUsado(novoUsuarioId);

        Assert.True(convite.Usado);
        Assert.NotNull(convite.DataUso);
        Assert.Equal(novoUsuarioId, convite.UsuarioCriadoId);
        Assert.False(convite.EstaValido());
    }

    [Fact]
    public void ConviteCadastro_QuandoJaUsado_MarcarComoUsadoDeveLancarExcecao()
    {
        var convite = new ConviteCadastro(Guid.NewGuid(), "teste@email.com", 7);
        convite.MarcarComoUsado(Guid.NewGuid());

        Assert.Throws<DomainException>(() => convite.MarcarComoUsado(Guid.NewGuid()));
    }

    [Fact]
    public void ConviteCadastro_Cancelar_DeveExpirarConvite()
    {
        var convite = new ConviteCadastro(Guid.NewGuid(), "teste@email.com", 7);
        convite.Cancelar();

        Assert.False(convite.EstaValido());
    }

    [Fact]
    public async Task GerarConviteAsync_ComDadosValidos_DeveSalvarERetornarDtoComLink()
    {
        var admin = new Usuario("Super Admin", "admin@teste.com", "hash", "SuperAdmin");
        _mockUsuarioRepo.Setup(r => r.ObterPorIdAsync(admin.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        var service = CriarService();

        var dto = new CriarConviteCadastroDto("convidado@teste.com", 5);
        var resultado = await service.GerarConviteAsync(admin.Id, dto, "https://meusite.com");

        Assert.NotNull(resultado);
        Assert.Equal("convidado@teste.com", resultado.EmailConvidado);
        Assert.Contains("https://meusite.com/Auth/Register?token=", resultado.LinkCadastro);
        Assert.False(resultado.Usado);

        _mockConviteRepo.Verify(r => r.AdicionarAsync(
            It.Is<ConviteCadastro>(c => c.EmailConvidado == "convidado@teste.com"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidarConviteAsync_ComCodigoValido_DeveRetornarTrue()
    {
        var convite = new ConviteCadastro(Guid.NewGuid(), null, 7, "tokenValido");
        _mockConviteRepo.Setup(r => r.ObterPorCodigoAsync("tokenValido", It.IsAny<CancellationToken>()))
            .ReturnsAsync(convite);

        var service = CriarService();

        var valido = await service.ValidarConviteAsync("tokenValido");
        Assert.True(valido);
    }

    [Fact]
    public async Task ValidarConviteAsync_ComCodigoInexistente_DeveRetornarFalse()
    {
        _mockConviteRepo.Setup(r => r.ObterPorCodigoAsync("naoExiste", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConviteCadastro?)null);

        var service = CriarService();

        var valido = await service.ValidarConviteAsync("naoExiste");
        Assert.False(valido);
    }
}
