using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Interfaces;
using AgendaPalestras.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgendaPalestras.UnitTests;

public class EvolutionApiServiceTests
{
    private readonly Mock<IConfiguracaoSistemaRepository> _mockConfigRepo = new();
    private readonly Mock<IUsuarioRepository> _mockUsuarioRepo = new();
    private readonly Mock<ILogger<EvolutionApiService>> _mockLogger = new();
    private readonly IConfiguration _config;

    public EvolutionApiServiceTests()
    {
        var settings = new Dictionary<string, string?>
        {
            { "WHATSAPP_API_URL", "http://localhost_simulado" },
            { "WHATSAPP_API_KEY", "chave_teste_123" },
            { "WHATSAPP_INSTANCE", "instancia-teste" }
        };
        _config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        _mockUsuarioRepo.Setup(r => r.ListarTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Usuario>());
    }

    private EvolutionApiService CriarService() =>
        new(new HttpClient(), _mockConfigRepo.Object, _mockUsuarioRepo.Object, _config, _mockLogger.Object);

    [Fact]
    public async Task ObterConfiguracao_SemBanco_DeveRetornarValoresDoConfiguration()
    {
        _mockConfigRepo.Setup(r => r.ObterValorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var service = CriarService();
        var config = await service.ObterConfiguracaoAsync();

        Assert.Equal("http://localhost_simulado", config.ApiUrl);
        Assert.Equal("chave_teste_123", config.ApiKey);
        Assert.Equal("instancia-teste", config.InstanceName);
    }

    [Fact]
    public async Task SalvarConfiguracao_DevePersistirNoRepositorio()
    {
        var service = CriarService();
        var novaConfig = new ConfiguracaoWhatsAppDto("https://evolution.api.com", "minha_chave", "minha_instancia");

        await service.SalvarConfiguracaoAsync(novaConfig);

        _mockConfigRepo.Verify(r => r.SalvarValorAsync(EvolutionApiService.ChaveApiUrl, "https://evolution.api.com", It.IsAny<CancellationToken>()), Times.Once);
        _mockConfigRepo.Verify(r => r.SalvarValorAsync(EvolutionApiService.ChaveApiKey, "minha_chave", It.IsAny<CancellationToken>()), Times.Once);
        _mockConfigRepo.Verify(r => r.SalvarValorAsync(EvolutionApiService.ChaveInstanceName, "minha_instancia", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObterConfiguracaoPorUsuario_ComInstancia_DeveRetornarConfiguracaoDoUsuario()
    {
        var usuario = new Usuario("Irmão Silva", "silva@email.com", "hash", "Admin");
        usuario.ConfigurarEvolution("instancia-silva");
        _mockUsuarioRepo.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var service = CriarService();
        var config = await service.ObterConfiguracaoPorUsuarioAsync(usuario.Id);

        Assert.NotNull(config);
        Assert.Equal("instancia-silva", config.InstanceName);
        Assert.Equal("http://localhost_simulado", config.ApiUrl);
        Assert.Equal("chave_teste_123", config.ApiKey);
    }

    [Fact]
    public async Task ObterConfiguracaoPorUsuario_SemInstancia_DeveRetornarNull()
    {
        var usuario = new Usuario("Irmão Sem Instância", "sem@email.com", "hash", "Admin");
        _mockUsuarioRepo.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var service = CriarService();
        var config = await service.ObterConfiguracaoPorUsuarioAsync(usuario.Id);

        Assert.Null(config);
    }

    [Fact]
    public async Task SalvarConfiguracaoUsuario_DeveAtualizarUsuario()
    {
        var usuario = new Usuario("Irmão Coordenador", "coord@email.com", "hash", "Admin");
        _mockUsuarioRepo.Setup(r => r.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        var service = CriarService();
        var dto = new SalvarInstanciaUsuarioDto(usuario.Id, "oradorpro-coord-vn");

        await service.SalvarConfiguracaoUsuarioAsync(dto);

        Assert.Equal("oradorpro-coord-vn", usuario.EvolutionInstanceName);
        Assert.True(usuario.TemWhatsAppConfigurado);
        _mockUsuarioRepo.Verify(r => r.AtualizarAsync(usuario, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListarUsuariosInstancias_DeveRetornarTodosUsuarios()
    {
        var u1 = new Usuario("Admin 1", "admin1@email.com", "hash", "SuperAdmin");
        var u2 = new Usuario("Admin 2", "admin2@email.com", "hash", "Admin");
        u2.ConfigurarEvolution("instancia-2");

        _mockUsuarioRepo.Setup(r => r.ListarTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Usuario> { u1, u2 });

        var service = CriarService();
        var lista = await service.ListarUsuariosInstanciasAsync();

        Assert.Equal(2, lista.Count);
        Assert.False(lista[0].TemWhatsAppConfigurado);
        Assert.True(lista[1].TemWhatsAppConfigurado);
        Assert.Equal("instancia-2", lista[1].InstanceName);
    }

    [Fact]
    public async Task CriarInstancia_ModoSimulado_DeveRetornarTrue()
    {
        var service = CriarService();
        var sucesso = await service.CriarInstanciaAsync("nova-instancia");

        Assert.True(sucesso);
    }

    [Fact]
    public async Task ObterStatusConexao_ModoSimulado_DeveRetornarConectado()
    {
        _mockConfigRepo.Setup(r => r.ObterValorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var service = CriarService();
        var status = await service.ObterStatusConexaoAsync();

        Assert.True(status.Conectado);
        Assert.Equal("open", status.State);
    }

    [Fact]
    public async Task ObterOuGerarQrCode_ModoSimulado_DeveRetornarQrBase64()
    {
        _mockConfigRepo.Setup(r => r.ObterValorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var service = CriarService();
        var qr = await service.ObterOuGerarQrCodeAsync();

        Assert.NotNull(qr.QrCodeBase64);
        Assert.StartsWith("data:image/png;base64", qr.QrCodeBase64);
        Assert.Equal("SIM-1234", qr.PairingCode);
    }

    [Fact]
    public async Task EnviarTexto_ModoSimulado_DeveRetornarSucessoComIdSimulado()
    {
        _mockConfigRepo.Setup(r => r.ObterValorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var service = CriarService();
        var resultado = await service.EnviarTextoAsync("11988887777", "Olá!", "instancia-especifica");

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.MessageId);
        Assert.StartsWith("sim_", resultado.MessageId);
    }

    [Fact]
    public async Task ConfigurarWebhook_ModoSimulado_DeveRetornarTrue()
    {
        _mockConfigRepo.Setup(r => r.ObterValorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var service = CriarService();
        var resultado = await service.ConfigurarWebhookAsync("https://meu-dominio.com/api/webhooks/whatsapp");

        Assert.True(resultado);
    }
}
