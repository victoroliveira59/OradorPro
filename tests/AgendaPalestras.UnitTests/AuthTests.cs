using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Application.Services;
using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Interfaces;
using AgendaPalestras.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgendaPalestras.UnitTests;

public class AuthTests
{
    private readonly PasswordHasherService _hasher = new();
    private readonly Mock<IUsuarioRepository> _mockUsuarioRepo = new();
    private readonly Mock<ILogger<AuthService>> _mockLogger = new();
    private readonly IConfiguration _config;

    public AuthTests()
    {
        var settings = new Dictionary<string, string?>
        {
            { "ADMIN_DEFAULT_EMAIL", "admin@agendapalestras.local" },
            { "ADMIN_DEFAULT_PASSWORD", "Admin@123456" }
        };
        _config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
    }

    [Fact]
    public void HashPassword_DeveGerarHashDiferenteDaSenhaOriginal()
    {
        var senha = "SenhaSegura!123";
        var hash = _hasher.HashPassword(senha);

        Assert.NotNull(hash);
        Assert.NotEqual(senha, hash);
        Assert.Contains(".", hash);
    }

    [Fact]
    public void VerifyPassword_ComSenhaCorreta_DeveRetornarTrue()
    {
        var senha = "MinhaSenhaSuperSecreta@99";
        var hash = _hasher.HashPassword(senha);

        var valida = _hasher.VerifyPassword(senha, hash);
        Assert.True(valida);
    }

    [Fact]
    public void VerifyPassword_ComSenhaIncorreta_DeveRetornarFalse()
    {
        var senha = "SenhaCorreta@123";
        var hash = _hasher.HashPassword(senha);

        var valida = _hasher.VerifyPassword("SenhaErrada@999", hash);
        Assert.False(valida);
    }

    [Fact]
    public async Task AutenticarAsync_ComCredenciaisValidas_DeveRetornarUsuarioAutenticado()
    {
        var email = "coordenador@agenda.com";
        var senha = "Coordenador@123";
        var hash = _hasher.HashPassword(senha);

        var usuario = new Usuario("Irmão Coordenador", email, hash, "Admin");

        _mockUsuarioRepo.Setup(r => r.ObterPorEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var service = new AuthService(_mockUsuarioRepo.Object, _hasher, _config, _mockLogger.Object);

        var dto = new LoginDto(email, senha);
        var autenticado = await service.AutenticarAsync(dto);

        Assert.NotNull(autenticado);
        Assert.Equal(usuario.Id, autenticado.Id);
        Assert.Equal("Irmão Coordenador", autenticado.Nome);
        Assert.Equal(email, autenticado.Email);
        Assert.Equal("Admin", autenticado.Papel);
    }

    [Fact]
    public async Task AutenticarAsync_ComUsuarioInativo_DeveRetornarNull()
    {
        var email = "inativo@agenda.com";
        var senha = "Senha@123";
        var hash = _hasher.HashPassword(senha);

        var usuario = new Usuario("Usuário Desativado", email, hash, "Admin");
        usuario.Desativar();

        _mockUsuarioRepo.Setup(r => r.ObterPorEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var service = new AuthService(_mockUsuarioRepo.Object, _hasher, _config, _mockLogger.Object);

        var dto = new LoginDto(email, senha);
        var autenticado = await service.AutenticarAsync(dto);

        Assert.Null(autenticado);
    }

    [Fact]
    public async Task InicializarAdminPadraoAsync_QuandoNaoExisteAdmin_DeveCriarAdmin()
    {
        _mockUsuarioRepo.Setup(r => r.ExisteAdminAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var service = new AuthService(_mockUsuarioRepo.Object, _hasher, _config, _mockLogger.Object);

        await service.InicializarAdminPadraoAsync();

        _mockUsuarioRepo.Verify(r => r.AdicionarAsync(
            It.Is<Usuario>(u => u.Email == "admin@agendapalestras.local" && u.Papel == "Admin"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
