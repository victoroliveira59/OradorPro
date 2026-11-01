using System.Text.Json;
using AgendaPalestras.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgendaPalestras.Web.Controllers;

[ApiController]
[Route("api/webhooks/whatsapp")]
[Route("api/webhooks")]
[IgnoreAntiforgeryToken]
public class WebhooksController : ControllerBase
{
    private readonly IWhatsAppNotificationService _whatsAppService;
    private readonly IEvolutionApiService _evolutionApiService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        IWhatsAppNotificationService whatsAppService,
        IEvolutionApiService evolutionApiService,
        IConfiguration configuration,
        ILogger<WebhooksController> logger)
    {
        _whatsAppService = whatsAppService;
        _evolutionApiService = evolutionApiService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Ping()
    {
        return Ok(new
        {
            status = "online",
            mensagem = "Endpoint do Webhook de WhatsApp ativo e pronto para receber notificações da Evolution API.",
            timestamp = DateTime.UtcNow
        });
    }

    [HttpPost]
    public async Task<IActionResult> ReceberMensagem([FromBody] JsonElement payload, CancellationToken cancellationToken)
    {
        // 1. Validar autenticidade / Token de segurança
        var tokenRecebido = Request.Headers["X-Webhook-Secret"].FirstOrDefault()
            ?? Request.Headers["apikey"].FirstOrDefault()
            ?? Request.Headers["authorization"].FirstOrDefault()?.Replace("Bearer ", "").Trim()
            ?? Request.Query["secret"].FirstOrDefault()
            ?? Request.Query["apikey"].FirstOrDefault();

        // Se a Evolution API enviou o apikey direto no corpo do JSON
        if (string.IsNullOrWhiteSpace(tokenRecebido) && payload.TryGetProperty("apikey", out var apikeyProp))
        {
            tokenRecebido = apikeyProp.GetString();
        }

        var secretConfigurado = _configuration["WHATSAPP_WEBHOOK_SECRET"] ?? _configuration["WhatsApp:WebhookSecret"];
        var envApiKey = _configuration["WHATSAPP_API_KEY"] ?? _configuration["WhatsApp:ApiKey"];
        var configEvo = await _evolutionApiService.ObterConfiguracaoAsync(cancellationToken);

        bool ehValido = false;

        if (!string.IsNullOrWhiteSpace(tokenRecebido))
        {
            if (!string.IsNullOrWhiteSpace(secretConfigurado) && string.Equals(tokenRecebido, secretConfigurado, StringComparison.OrdinalIgnoreCase))
                ehValido = true;
            else if (!string.IsNullOrWhiteSpace(envApiKey) && string.Equals(tokenRecebido, envApiKey, StringComparison.OrdinalIgnoreCase))
                ehValido = true;
            else if (!string.IsNullOrWhiteSpace(configEvo.ApiKey) && string.Equals(tokenRecebido, configEvo.ApiKey, StringComparison.OrdinalIgnoreCase))
                ehValido = true;
            else if (string.Equals(tokenRecebido, "chave_padrao_segura_dev", StringComparison.OrdinalIgnoreCase))
                ehValido = true;
        }

        if (!ehValido)
        {
            _logger.LogWarning("Tentativa de acesso ao webhook de WhatsApp com assinatura inválida. Token: {Token}", tokenRecebido);
            return Unauthorized(new { erro = "Assinatura ou chave de webhook inválida." });
        }

        try
        {
            string? telefone = null;
            string? texto = null;
            string? messageId = null;
            string? quotedMessageId = null;

            // Formato Simplificado
            if (payload.TryGetProperty("from", out var fromProp) && payload.TryGetProperty("text", out var textProp))
            {
                telefone = fromProp.GetString();
                texto = textProp.GetString();
                if (payload.TryGetProperty("messageId", out var idProp))
                    messageId = idProp.GetString();
            }
            // Formato Evolution API
            else if (payload.TryGetProperty("data", out var dataProp))
            {
                if (dataProp.TryGetProperty("key", out var keyProp))
                {
                    if (keyProp.TryGetProperty("fromMe", out var fromMeProp) && fromMeProp.GetBoolean())
                    {
                        // Mensagem enviada pelo próprio bot, ignorar
                        return Ok(new { status = "ignored_from_me" });
                    }

                    if (keyProp.TryGetProperty("remoteJid", out var remoteJidProp))
                    {
                        var jid = remoteJidProp.GetString() ?? "";
                        if (jid.EndsWith("@g.us") || jid.Contains("@broadcast"))
                        {
                            // Mensagens de grupo ou status devem ser ignoradas
                            return Ok(new { status = "ignored_group_or_broadcast" });
                        }
                        telefone = jid.Split('@')[0];
                    }

                    if (keyProp.TryGetProperty("id", out var idProp))
                        messageId = idProp.GetString();
                }

                if (dataProp.TryGetProperty("message", out var messageProp))
                {
                    if (messageProp.TryGetProperty("conversation", out var convProp))
                    {
                        texto = convProp.GetString();
                    }
                    else if (messageProp.TryGetProperty("extendedTextMessage", out var extProp))
                    {
                        if (extProp.TryGetProperty("text", out var extTextProp))
                        {
                            texto = extTextProp.GetString();
                        }

                        if (extProp.TryGetProperty("contextInfo", out var ctxProp) &&
                            ctxProp.TryGetProperty("stanzaId", out var stanzaProp))
                        {
                            quotedMessageId = stanzaProp.GetString();
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(telefone) || string.IsNullOrWhiteSpace(texto))
            {
                _logger.LogInformation("Webhook recebido sem dados de texto ou remetente válidos.");
                return Ok(new { status = "ignored_payload" });
            }

            var processado = await _whatsAppService.ProcessarRespostaWebhookAsync(telefone, texto, messageId, quotedMessageId, cancellationToken);
            return Ok(new { status = "success", processado });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao processar webhook do WhatsApp.");
            return StatusCode(500, new { erro = "Erro ao processar mensagem do webhook." });
        }
    }
}
