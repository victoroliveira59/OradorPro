using AgendaPalestras.Application.DTOs;
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
    }

    [Fact]
    public async Task ObterConfiguracao_SemBanco_DeveRetornarValoresDoConfiguration()
    {
        _mockConfigRepo.Setup(r => r.ObterValorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var service = new EvolutionApiService(new HttpClient(), _mockConfigRepo.Object, _config, _mockLogger.Object);
        var config = await service.ObterConfiguracaoAsync();

        Assert.Equal("http://localhost_simulado", config.ApiUrl);
        Assert.Equal("chave_teste_123", config.ApiKey);
        Assert.Equal("instancia-teste", config.InstanceName);
    }

    [Fact]
    public async Task SalvarConfiguracao_DevePersistirNoRepositorio()
    {
        var service = new EvolutionApiService(new HttpClient(), _mockConfigRepo.Object, _config, _mockLogger.Object);
        var novaConfig = new ConfiguracaoWhatsAppDto("https://evolution.api.com", "minha_chave", "minha_instancia");

        await service.SalvarConfiguracaoAsync(novaConfig);

        _mockConfigRepo.Verify(r => r.SalvarValorAsync(EvolutionApiService.ChaveApiUrl, "https://evolution.api.com", It.IsAny<CancellationToken>()), Times.Once);
        _mockConfigRepo.Verify(r => r.SalvarValorAsync(EvolutionApiService.ChaveApiKey, "minha_chave", It.IsAny<CancellationToken>()), Times.Once);
        _mockConfigRepo.Verify(r => r.SalvarValorAsync(EvolutionApiService.ChaveInstanceName, "minha_instancia", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObterStatusConexao_ModoSimulado_DeveRetornarConectado()
    {
        _mockConfigRepo.Setup(r => r.ObterValorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var service = new EvolutionApiService(new HttpClient(), _mockConfigRepo.Object, _config, _mockLogger.Object);
        var status = await service.ObterStatusConexaoAsync();

        Assert.True(status.Conectado);
        Assert.Equal("open", status.State);
    }

    [Fact]
    public async Task ObterOuGerarQrCode_ModoSimulado_DeveRetornarQrBase64()
    {
        _mockConfigRepo.Setup(r => r.ObterValorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var service = new EvolutionApiService(new HttpClient(), _mockConfigRepo.Object, _config, _mockLogger.Object);
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

        var service = new EvolutionApiService(new HttpClient(), _mockConfigRepo.Object, _config, _mockLogger.Object);
        var resultado = await service.EnviarTextoAsync("11988887777", "Olá!");

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.MessageId);
        Assert.StartsWith("sim_", resultado.MessageId);
    }

    [Fact]
    public async Task ConfigurarWebhook_ModoSimulado_DeveRetornarTrue()
    {
        _mockConfigRepo.Setup(r => r.ObterValorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var service = new EvolutionApiService(new HttpClient(), _mockConfigRepo.Object, _config, _mockLogger.Object);
        var resultado = await service.ConfigurarWebhookAsync("https://meu-dominio.com/api/webhooks/whatsapp");

        Assert.True(resultado);
    }
}
