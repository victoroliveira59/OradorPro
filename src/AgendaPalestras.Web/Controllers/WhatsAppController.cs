using System.Security.Claims;
using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Application.Services;
using AgendaPalestras.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaPalestras.Web.Controllers;

[Authorize]
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

        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var usuarioId = ObterUsuarioLogadoId();

        var globalConfig = await _evolutionApiService.ObterConfiguracaoAsync();
        ConfiguracaoWhatsAppDto? userConfig = null;

        if (usuarioId.HasValue)
        {
            userConfig = await _evolutionApiService.ObterConfiguracaoPorUsuarioAsync(usuarioId.Value);
        }

        if (!isSuperAdmin)
        {
            if (userConfig == null || string.IsNullOrWhiteSpace(userConfig.InstanceName))
            {
                var vmNaoConfigurado = new WhatsAppConnectionViewModel
                {
                    Configuracao = new ConfiguracaoWhatsAppDto("", "", ""),
                    Status = new EvolutionConnectionStatusDto("", "unconfigured", false),
                    IsSuperAdmin = false,
                    InstanciaNaoConfigurada = true
                };
                return View(vmNaoConfigurado);
            }

            var statusUser = await _evolutionApiService.ObterStatusConexaoAsync(userConfig.InstanceName);
            var vmUser = new WhatsAppConnectionViewModel
            {
                Configuracao = userConfig,
                Status = statusUser,
                IsSuperAdmin = false,
                InstanciaNaoConfigurada = false
            };
            return View(vmUser);
        }

        // SuperAdmin
        var configEfetiva = userConfig ?? globalConfig;
        var statusSuper = await _evolutionApiService.ObterStatusConexaoAsync(configEfetiva.InstanceName);
        var usuariosInstancias = await _evolutionApiService.ListarUsuariosInstanciasAsync();

        var vmSuper = new WhatsAppConnectionViewModel
        {
            Configuracao = configEfetiva,
            Status = statusSuper,
            IsSuperAdmin = true,
            InstanciaNaoConfigurada = false,
            UsuariosInstancias = usuariosInstancias
        };

        return View(vmSuper);
    }

    [HttpGet]
    public async Task<IActionResult> Status(string? instanceName = null)
    {
        var instancia = await ResolverInstanciaAtivaAsync(instanceName);
        if (string.IsNullOrWhiteSpace(instancia))
        {
            return Json(new EvolutionConnectionStatusDto("", "unconfigured", false));
        }

        var status = await _evolutionApiService.ObterStatusConexaoAsync(instancia);
        return Json(status);
    }

    [HttpGet]
    public async Task<IActionResult> QrCode(string? instanceName = null)
    {
        var instancia = await ResolverInstanciaAtivaAsync(instanceName);
        if (string.IsNullOrWhiteSpace(instancia))
        {
            return Json(new EvolutionQrCodeDto("", null, null, 0));
        }

        var qr = await _evolutionApiService.ObterOuGerarQrCodeAsync(instancia);
        return Json(qr);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Conectar(string? instanceName = null)
    {
        var isSuperAdmin = User.IsInRole("SuperAdmin");
        var usuarioId = ObterUsuarioLogadoId();
        var instancia = await ResolverInstanciaAtivaAsync(instanceName);

        if (string.IsNullOrWhiteSpace(instancia))
        {
            ViewBag.ErrorMessage = "Sua sessão do WhatsApp ainda não foi provisionada pelo Super Administrador.";
            var vmVazio = new WhatsAppConnectionViewModel
            {
                Configuracao = new ConfiguracaoWhatsAppDto("", "", ""),
                Status = new EvolutionConnectionStatusDto("", "unconfigured", false),
                IsSuperAdmin = isSuperAdmin,
                InstanciaNaoConfigurada = true
            };
            return View("Index", vmVazio);
        }

        EvolutionQrCodeDto? qr = null;
        try
        {
            qr = await _evolutionApiService.ObterOuGerarQrCodeAsync(instancia);
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
            _logger.LogError(ex, "Erro ao conectar ao WhatsApp via Evolution API para instância {Instancia}", instancia);
            ViewBag.ErrorMessage = $"Erro ao tentar conectar: {ex.Message}";
        }

        var config = await _evolutionApiService.ObterConfiguracaoAsync();
        var status = await _evolutionApiService.ObterStatusConexaoAsync(instancia);

        var vm = new WhatsAppConnectionViewModel
        {
            Configuracao = new ConfiguracaoWhatsAppDto(config.ApiUrl, config.ApiKey, instancia),
            Status = status,
            QrCode = qr,
            IsSuperAdmin = isSuperAdmin,
            InstanciaNaoConfigurada = false,
            UsuariosInstancias = isSuperAdmin ? await _evolutionApiService.ListarUsuariosInstanciasAsync() : Array.Empty<UsuarioWhatsAppInstanciaDto>()
        };

        return View("Index", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desconectar(string? instanceName = null)
    {
        var instancia = await ResolverInstanciaAtivaAsync(instanceName);
        if (string.IsNullOrWhiteSpace(instancia))
        {
            TempData["Error"] = "Nenhuma instância ativa identificada para desconexão.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var sucesso = await _evolutionApiService.DesconectarAsync(instancia);
            if (sucesso)
            {
                TempData["Success"] = $"Sessão do WhatsApp ({instancia}) desconectada com sucesso.";
            }
            else
            {
                TempData["Error"] = "Falha ao desconectar da Evolution API. Verifique os logs.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desconectar instância {Instancia} da Evolution API", instancia);
            TempData["Error"] = $"Erro ao desconectar: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
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
            TempData["Success"] = "Configurações globais da Evolution API salvas com sucesso!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar configurações da Evolution API");
            TempData["Error"] = $"Erro ao salvar configurações: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarInstanciaUsuario(SalvarInstanciaUsuarioDto dto)
    {
        if (dto.UsuarioId == Guid.Empty)
        {
            TempData["Error"] = "Usuário inválido para vinculação de instância.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _evolutionApiService.SalvarConfiguracaoUsuarioAsync(dto);
            TempData["Success"] = "Instância do WhatsApp vinculada ao usuário com sucesso!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar instância do usuário {UsuarioId}", dto.UsuarioId);
            TempData["Error"] = $"Erro ao vincular instância: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CriarInstanciaEvolution(string instanceName)
    {
        if (string.IsNullOrWhiteSpace(instanceName))
        {
            TempData["Error"] = "Informe o nome da instância a ser criada.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var criada = await _evolutionApiService.CriarInstanciaAsync(instanceName.Trim());
            if (criada)
            {
                TempData["Success"] = $"Instância '{instanceName.Trim()}' provisionada com sucesso na Evolution API!";
            }
            else
            {
                TempData["Error"] = $"Não foi possível provisionar a instância '{instanceName.Trim()}'. Verifique a URL e a ApiKey nas configurações.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao provisionar instância {Instancia} na Evolution API", instanceName);
            TempData["Error"] = $"Exceção ao provisionar instância: {ex.Message}";
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

        var instancia = await ResolverInstanciaAtivaAsync(dto.InstanceName);
        if (string.IsNullOrWhiteSpace(instancia))
        {
            TempData["Error"] = "Sessão do WhatsApp não configurada para envio.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var resultado = await _evolutionApiService.EnviarTextoAsync(dto.Telefone, dto.Mensagem, instancia);
            if (resultado.Sucesso)
            {
                TempData["Success"] = $"Mensagem de teste enviada com sucesso pela instância '{instancia}'! (ID: {resultado.MessageId})";
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
    [Authorize(Roles = "SuperAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfigurarWebhook(string webhookUrl, string? instanceName = null)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            TempData["Error"] = "A URL do webhook é obrigatória.";
            return RedirectToAction(nameof(Index));
        }

        var instancia = await ResolverInstanciaAtivaAsync(instanceName);

        try
        {
            var sucesso = await _evolutionApiService.ConfigurarWebhookAsync(webhookUrl.Trim(), instancia);
            if (sucesso)
            {
                TempData["Success"] = $"Webhook configurado com sucesso na Evolution API para a instância '{instancia}'!";
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

    private Guid? ObterUsuarioLogadoId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(idClaim, out var id))
        {
            return id;
        }
        return null;
    }

    private async Task<string?> ResolverInstanciaAtivaAsync(string? parametroInstancia = null)
    {
        if (User.IsInRole("SuperAdmin") && !string.IsNullOrWhiteSpace(parametroInstancia))
        {
            return parametroInstancia.Trim();
        }

        var usuarioId = ObterUsuarioLogadoId();
        if (usuarioId.HasValue)
        {
            var userConfig = await _evolutionApiService.ObterConfiguracaoPorUsuarioAsync(usuarioId.Value);
            if (userConfig != null && !string.IsNullOrWhiteSpace(userConfig.InstanceName))
            {
                return userConfig.InstanceName.Trim();
            }
        }

        if (User.IsInRole("SuperAdmin"))
        {
            var global = await _evolutionApiService.ObterConfiguracaoAsync();
            return global.InstanceName.Trim();
        }

        return null;
    }
}
