using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Application.Services;
using AgendaPalestras.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgendaPalestras.Infrastructure.Services;

public class EvolutionApiService : IEvolutionApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguracaoSistemaRepository _configRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EvolutionApiService> _logger;

    public const string ChaveApiUrl = "WhatsApp_ApiUrl";
    public const string ChaveApiKey = "WhatsApp_ApiKey";
    public const string ChaveInstanceName = "WhatsApp_InstanceName";

    public EvolutionApiService(
        HttpClient httpClient,
        IConfiguracaoSistemaRepository configRepository,
        IConfiguration configuration,
        ILogger<EvolutionApiService> logger)
    {
        _httpClient = httpClient;
        _configRepository = configRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ConfiguracaoWhatsAppDto> ObterConfiguracaoAsync(CancellationToken cancellationToken = default)
    {
        var dbUrl = await _configRepository.ObterValorAsync(ChaveApiUrl, cancellationToken);
        var dbKey = await _configRepository.ObterValorAsync(ChaveApiKey, cancellationToken);
        var dbInst = await _configRepository.ObterValorAsync(ChaveInstanceName, cancellationToken);

        var apiUrl = dbUrl ?? _configuration["WHATSAPP_API_URL"] ?? _configuration["WhatsApp:ApiUrl"] ?? "http://localhost:8080";
        var apiKey = dbKey ?? _configuration["WHATSAPP_API_KEY"] ?? _configuration["WhatsApp:ApiKey"] ?? "";
        var instance = dbInst ?? _configuration["WHATSAPP_INSTANCE"] ?? _configuration["WhatsApp:InstanceName"] ?? "oradorpro-admin";

        return new ConfiguracaoWhatsAppDto(apiUrl.TrimEnd('/'), apiKey.Trim(), instance.Trim());
    }

    public async Task SalvarConfiguracaoAsync(ConfiguracaoWhatsAppDto dto, CancellationToken cancellationToken = default)
    {
        await _configRepository.SalvarValorAsync(ChaveApiUrl, dto.ApiUrl.TrimEnd('/'), cancellationToken);
        await _configRepository.SalvarValorAsync(ChaveApiKey, dto.ApiKey.Trim(), cancellationToken);
        await _configRepository.SalvarValorAsync(ChaveInstanceName, dto.InstanceName.Trim(), cancellationToken);
    }

    public async Task<EvolutionConnectionStatusDto> ObterStatusConexaoAsync(CancellationToken cancellationToken = default)
    {
        var config = await ObterConfiguracaoAsync(cancellationToken);

        if (IsModoSimulado(config.ApiUrl))
        {
            return new EvolutionConnectionStatusDto(
                config.InstanceName,
                "open",
                true,
                "5511999999999",
                "Admin Simulado"
            );
        }

        try
        {
            var url = $"{config.ApiUrl}/instance/connectionState/{config.InstanceName}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("apikey", config.ApiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new EvolutionConnectionStatusDto(config.InstanceName, "not_created", false);
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Falha ao obter status da Evolution API: {StatusCode}", response.StatusCode);
                return new EvolutionConnectionStatusDto(config.InstanceName, "error", false);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            string state = "close";
            if (root.TryGetProperty("instance", out var instanceElement))
            {
                if (instanceElement.TryGetProperty("state", out var stateElement))
                {
                    state = stateElement.GetString() ?? "close";
                }
            }

            bool conectado = string.Equals(state, "open", StringComparison.OrdinalIgnoreCase);
            return new EvolutionConnectionStatusDto(config.InstanceName, state, conectado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro de comunicação ao consultar status da Evolution API");
            return new EvolutionConnectionStatusDto(config.InstanceName, "offline", false);
        }
    }

    public async Task<EvolutionQrCodeDto> ObterOuGerarQrCodeAsync(CancellationToken cancellationToken = default)
    {
        var config = await ObterConfiguracaoAsync(cancellationToken);

        if (IsModoSimulado(config.ApiUrl))
        {
            // Retorna um QR code simulado transparente para testes em dev
            return new EvolutionQrCodeDto(
                config.InstanceName,
                "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==",
                "SIM-1234",
                1
            );
        }

        try
        {
            // 1. Verifica se a instância existe
            var statusUrl = $"{config.ApiUrl}/instance/connectionState/{config.InstanceName}";
            var statusReq = new HttpRequestMessage(HttpMethod.Get, statusUrl);
            statusReq.Headers.Add("apikey", config.ApiKey);
            var statusResp = await _httpClient.SendAsync(statusReq, cancellationToken);

            if (statusResp.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Cria a instância via POST /instance/create
                var createUrl = $"{config.ApiUrl}/instance/create";
                var createReq = new HttpRequestMessage(HttpMethod.Post, createUrl);
                createReq.Headers.Add("apikey", config.ApiKey);
                createReq.Content = JsonContent.Create(new
                {
                    instanceName = config.InstanceName,
                    integration = "WHATSAPP-BAILEYS",
                    qrcode = true
                });

                var createResp = await _httpClient.SendAsync(createReq, cancellationToken);
                if (!createResp.IsSuccessStatusCode)
                {
                    var err = await createResp.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Erro ao criar instância Evolution API: {Error}", err);
                }
            }

            // 2. Chama GET /instance/connect/{instance} para obter o QR Code
            var connectUrl = $"{config.ApiUrl}/instance/connect/{config.InstanceName}";
            var connectReq = new HttpRequestMessage(HttpMethod.Get, connectUrl);
            connectReq.Headers.Add("apikey", config.ApiKey);

            var connectResp = await _httpClient.SendAsync(connectReq, cancellationToken);
            if (!connectResp.IsSuccessStatusCode)
            {
                var err = await connectResp.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Erro ao obter connect/qrcode da Evolution API: {Error}", err);
                return new EvolutionQrCodeDto(config.InstanceName, null, null, 0);
            }

            var content = await connectResp.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            string? qrBase64 = null;
            string? pairingCode = null;
            int count = 1;

            if (root.TryGetProperty("base64", out var base64El))
            {
                qrBase64 = base64El.GetString();
            }
            else if (root.TryGetProperty("code", out var codeEl))
            {
                var codeVal = codeEl.GetString();
                if (!string.IsNullOrEmpty(codeVal) && codeVal.StartsWith("data:image"))
                {
                    qrBase64 = codeVal;
                }
            }

            if (root.TryGetProperty("pairingCode", out var pairEl))
            {
                pairingCode = pairEl.GetString();
            }

            if (root.TryGetProperty("count", out var countEl))
            {
                count = countEl.GetInt32();
            }

            return new EvolutionQrCodeDto(config.InstanceName, qrBase64, pairingCode, count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção ao gerar QR code na Evolution API");
            return new EvolutionQrCodeDto(config.InstanceName, null, null, 0);
        }
    }

    public async Task<bool> DesconectarAsync(CancellationToken cancellationToken = default)
    {
        var config = await ObterConfiguracaoAsync(cancellationToken);

        if (IsModoSimulado(config.ApiUrl))
        {
            _logger.LogInformation("[WHATSAPP SIMULADO] Instância {Instance} desconectada.", config.InstanceName);
            return true;
        }

        try
        {
            var logoutUrl = $"{config.ApiUrl}/instance/logout/{config.InstanceName}";
            var request = new HttpRequestMessage(HttpMethod.Delete, logoutUrl);
            request.Headers.Add("apikey", config.ApiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desconectar instância {Instance} na Evolution API", config.InstanceName);
            return false;
        }
    }

    public async Task<ResultadoEnvioMensagem> EnviarTextoAsync(string telefone, string mensagem, CancellationToken cancellationToken = default)
    {
        var config = await ObterConfiguracaoAsync(cancellationToken);
        var numeroFormatado = FormatarTelefone(telefone);

        if (IsModoSimulado(config.ApiUrl))
        {
            var fakeId = $"sim_{Guid.NewGuid():N}";
            _logger.LogInformation("[WHATSAPP SIMULADO] Para: {Telefone} | Msg: {Mensagem}", numeroFormatado, mensagem);
            return new ResultadoEnvioMensagem(true, fakeId);
        }

        try
        {
            var sendUrl = $"{config.ApiUrl}/message/sendText/{config.InstanceName}";
            var request = new HttpRequestMessage(HttpMethod.Post, sendUrl);
            request.Headers.Add("apikey", config.ApiKey);

            // Evolution API v2: { number: "...", text: "...", delay: 1200 }
            request.Content = JsonContent.Create(new
            {
                number = numeroFormatado,
                text = mensagem,
                delay = 1200
            });

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var messageId = $"evo_{Guid.NewGuid():N}";
                try
                {
                    using var doc = JsonDocument.Parse(content);
                    if (doc.RootElement.TryGetProperty("key", out var keyEl) &&
                        keyEl.TryGetProperty("id", out var idEl))
                    {
                        messageId = idEl.GetString() ?? messageId;
                    }
                }
                catch { }

                return new ResultadoEnvioMensagem(true, messageId);
            }

            var erro = $"Falha ao enviar mensagem via Evolution API: {response.StatusCode}";
            _logger.LogError(erro);
            return new ResultadoEnvioMensagem(false, null, erro);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção ao disparar texto para {Telefone}", numeroFormatado);
            return new ResultadoEnvioMensagem(false, null, ex.Message);
        }
    }

    public async Task<bool> ConfigurarWebhookAsync(string webhookUrl, CancellationToken cancellationToken = default)
    {
        var config = await ObterConfiguracaoAsync(cancellationToken);

        if (IsModoSimulado(config.ApiUrl))
        {
            _logger.LogInformation("[WHATSAPP SIMULADO] Webhook configurado para {Url}", webhookUrl);
            return true;
        }

        try
        {
            var setUrl = $"{config.ApiUrl}/webhook/set/{config.InstanceName}";
            var request = new HttpRequestMessage(HttpMethod.Post, setUrl);
            request.Headers.Add("apikey", config.ApiKey);

            var body = new
            {
                enabled = true,
                url = webhookUrl,
                webhookByEvents = false,
                webhookBase64 = false,
                events = new[]
                {
                    "MESSAGES_UPSERT"
                }
            };

            request.Content = JsonContent.Create(body);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Webhook configurado com sucesso na Evolution API para instância {Instance}: {Url}", config.InstanceName, webhookUrl);
                return true;
            }

            var erroCorpo = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Falha ao configurar webhook na Evolution API ({StatusCode}): {Erro}", response.StatusCode, erroCorpo);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção ao configurar webhook na Evolution API para {Url}", webhookUrl);
            return false;
        }
    }

    private static bool IsModoSimulado(string? apiUrl)
    {
        return string.IsNullOrWhiteSpace(apiUrl) ||
               apiUrl.Contains("localhost_simulado", StringComparison.OrdinalIgnoreCase) ||
               apiUrl.Contains("simulado", StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatarTelefone(string telefone)
    {
        var limpo = Regex.Replace(telefone ?? string.Empty, @"\D", "");
        if (!limpo.StartsWith("55") && limpo.Length >= 10 && limpo.Length <= 11)
            limpo = "55" + limpo;
        return limpo;
    }
}
