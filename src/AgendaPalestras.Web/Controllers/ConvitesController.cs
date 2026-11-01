using System.Security.Claims;
using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Application.Services;
using AgendaPalestras.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaPalestras.Web.Controllers;

[Authorize(Roles = "SuperAdmin")]
public class ConvitesController : Controller
{
    private readonly IConviteService _conviteService;

    public ConvitesController(IConviteService conviteService)
    {
        _conviteService = conviteService;
    }

    public async Task<IActionResult> Index()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var convites = await _conviteService.ListarTodosAsync(baseUrl);
        return View(convites);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Gerar(CriarConviteCadastroDto dto)
    {
        var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(claimId, out var usuarioId))
        {
            return Forbid();
        }

        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var convite = await _conviteService.GerarConviteAsync(usuarioId, dto, baseUrl);
            TempData["Success"] = "Link de cadastro gerado com sucesso!";
            TempData["LinkGerado"] = convite.LinkCadastro;
            TempData["EmailConvidado"] = convite.EmailConvidado ?? "Qualquer e-mail";
        }
        catch (DomainException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(Guid id)
    {
        try
        {
            await _conviteService.CancelarConviteAsync(id);
            TempData["Success"] = "Convite cancelado com sucesso!";
        }
        catch (DomainException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
