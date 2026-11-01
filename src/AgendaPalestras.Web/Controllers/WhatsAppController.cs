using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Application.Services;
using AgendaPalestras.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaPalestras.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
public class WhatsAppController : Controller
{
    private readonly IEvolutionApiService _evolutionApiService;
    private readonly ILogger<WhatsAppController> _logger;

    public WhatsAppController(
        IEvolutionApiService evolutionApiService,
        ILogger<WhatsAppController> logger)
    {
        _evolutionApiService = evolutionApiService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Limpa cookie de TempData para prevenir sobrecarga de cabeçalho HTTP
        Response.Cookies.Delete(".AspNetCore.Mvc.CookieTempDataProvider");

        var config = await _evolutionApiService.ObterConfiguracaoAsync();
        var status = await _evolutionApiService.ObterStatusConexaoAsync();

        var vm = new WhatsAppConnectionViewModel
        {
            Configuracao = config,
            Status = status,
            QrCode = null
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Status()
    {
        var status = await _evolutionApiService.ObterStatusConexaoAsync();
        return Json(status);
    }

    [HttpGet]
    public async Task<IActionResult> QrCode()
    {
        var qr = await _evolutionApiService.ObterOuGerarQrCodeAsync();
        return Json(qr);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Conectar()
    {
        var config = await _evolutionApiService.ObterConfiguracaoAsync();
        var status = await _evolutionApiService.ObterStatusConexaoAsync();
        EvolutionQrCodeDto? qr = null;

        try
        {
            qr = await _evolutionApiService.ObterOuGerarQrCodeAsync();
            if (string.IsNullOrEmpty(qr.QrCodeBase64) && string.IsNullOrEmpty(qr.PairingCode))
            {
                ViewBag.ErrorMessage = "Não foi possível obter o QR Code. Verifique se a Evolution API está em execução.";
            }
            else
            {
                ViewBag.SuccessMessage = "QR Code gerado com sucesso! Aponte a câmera do seu WhatsApp para escanear.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao conectar ao WhatsApp via Evolution API");
            ViewBag.ErrorMessage = $"Erro ao tentar conectar: {ex.Message}";
        }

        var vm = new WhatsAppConnectionViewModel
        {
            Configuracao = config,
            Status = status,
            QrCode = qr
        };

        return View("Index", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desconectar()
    {
        try
        {
            var sucesso = await _evolutionApiService.DesconectarAsync();
            if (sucesso)
            {
                TempData["Success"] = "Sessão do WhatsApp desconectada com sucesso.";
            }
            else
            {
                TempData["Error"] = "Falha ao desconectar da Evolution API. Verifique os logs.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desconectar da Evolution API");
            TempData["Error"] = $"Erro ao desconectar: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarConfiguracao(ConfiguracaoWhatsAppDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ApiUrl) || string.IsNullOrWhiteSpace(dto.InstanceName))
        {
            TempData["Error"] = "A URL da API e o Nome da Instância são obrigatórios.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _evolutionApiService.SalvarConfiguracaoAsync(dto);
            TempData["Success"] = "Configurações da Evolution API salvas com sucesso!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar configurações da Evolution API");
            TempData["Error"] = $"Erro ao salvar configurações: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestarEnvio(TestarEnvioWhatsAppDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Telefone) || string.IsNullOrWhiteSpace(dto.Mensagem))
        {
            TempData["Error"] = "Telefone e mensagem são obrigatórios para o teste de envio.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var resultado = await _evolutionApiService.EnviarTextoAsync(dto.Telefone, dto.Mensagem);
            if (resultado.Sucesso)
            {
                TempData["Success"] = $"Mensagem de teste enviada com sucesso! (ID: {resultado.MessageId})";
            }
            else
            {
                TempData["Error"] = $"Falha no envio da mensagem de teste: {resultado.Erro}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao testar envio pelo WhatsApp");
            TempData["Error"] = $"Exceção ao enviar teste: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfigurarWebhook(string webhookUrl)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            TempData["Error"] = "A URL do webhook é obrigatória.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var sucesso = await _evolutionApiService.ConfigurarWebhookAsync(webhookUrl.Trim());
            if (sucesso)
            {
                TempData["Success"] = "Webhook configurado com sucesso na Evolution API! Agora as mensagens do orador serão direcionadas para cá.";
            }
            else
            {
                TempData["Error"] = "Falha ao configurar webhook na Evolution API. Verifique a URL e as credenciais.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao configurar webhook");
            TempData["Error"] = $"Erro ao configurar webhook: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
