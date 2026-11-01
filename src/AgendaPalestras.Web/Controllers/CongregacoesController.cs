using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Application.Services;
using AgendaPalestras.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaPalestras.Web.Controllers;

[Authorize]
public class CongregacoesController : Controller
{
    private readonly ICongregacaoService _service;

    public CongregacoesController(ICongregacaoService service)
    {
        _service = service;
    }

    private Guid? ObterCongregacaoIdUsuarioLogado()
    {
        var claim = User.FindFirst("CongregacaoId")?.Value;
        if (Guid.TryParse(claim, out var id)) return id;
        return null;
    }

    public async Task<IActionResult> Index()
    {
        var congregacoes = await _service.ListarTodasAsync();
        return View(congregacoes);
    }

    public IActionResult Create()
    {
        return View(new CriarCongregacaoDto("", "", "", "", ""));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CriarCongregacaoDto dto)
    {
        try
        {
            await _service.CriarAsync(dto);
            TempData["Success"] = "Congregação cadastrada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return View(dto);
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var userCongId = ObterCongregacaoIdUsuarioLogado();
        if (userCongId.HasValue && userCongId.Value != id)
        {
            return RedirectToAction(nameof(Edit), new { id = userCongId.Value });
        }

        var c = await _service.ObterPorIdAsync(id);
        if (c == null) return NotFound();

        var dto = new AtualizarCongregacaoDto(c.Id, c.Nome, c.Cidade, c.Circuito, c.DiasReuniao, c.HorarioReuniao, c.Ativo, c.Endereco, c.LinkGoogleMaps);
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AtualizarCongregacaoDto dto)
    {
        try
        {
            await _service.AtualizarAsync(dto);
            TempData["Success"] = "Congregação atualizada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlternarStatus(Guid id)
    {
        await _service.AlternarStatusAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
