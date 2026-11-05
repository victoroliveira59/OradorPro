using System.Security.Claims;
using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Application.Services;
using AgendaPalestras.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaPalestras.Web.Controllers;

[Authorize]
public class OradoresController : Controller
{
    private readonly IOradorService _oradorService;
    private readonly ICongregacaoService _congregacaoService;
    private readonly IDiscursoService _discursoService;

    public OradoresController(
        IOradorService oradorService,
        ICongregacaoService congregacaoService,
        IDiscursoService discursoService)
    {
        _oradorService = oradorService;
        _congregacaoService = congregacaoService;
        _discursoService = discursoService;
    }

    private Guid ObterUsuarioIdLogado()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(claim, out var id)) return id;
        return Guid.Empty;
    }

    private Guid? ObterCongregacaoIdUsuarioLogado()
    {
        var claim = User.FindFirst("CongregacaoId")?.Value;
        if (Guid.TryParse(claim, out var id)) return id;
        return null;
    }

    private bool EhSuperAdmin() => User.IsInRole("SuperAdmin");

    private async Task<bool> PodeGerenciarOradorAsync(OradorDto orador)
    {
        if (EhSuperAdmin()) return true;
        var usuarioId = ObterUsuarioIdLogado();
        var userCongId = ObterCongregacaoIdUsuarioLogado();

        if (userCongId.HasValue && orador.CongregacaoId == userCongId.Value) return true;
        if (orador.UsuarioCriadorId.HasValue && orador.UsuarioCriadorId.Value == usuarioId) return true;

        var congregacao = await _congregacaoService.ObterPorIdAsync(orador.CongregacaoId);
        if (congregacao != null && congregacao.UsuarioCriadorId.HasValue && congregacao.UsuarioCriadorId.Value == usuarioId)
            return true;

        return false;
    }

    public async Task<IActionResult> Index(Guid? congregacaoId = null)
    {
        var usuarioId = ObterUsuarioIdLogado();
        var userCongId = ObterCongregacaoIdUsuarioLogado();
        var ehSuperAdmin = EhSuperAdmin();

        ViewBag.Congregacoes = await _congregacaoService.ListarPorUsuarioAsync(usuarioId, userCongId, ehSuperAdmin, apenasAtivas: true);
        ViewBag.FiltroCongregacaoId = congregacaoId;

        var oradores = await _oradorService.ListarPorUsuarioAsync(usuarioId, userCongId, ehSuperAdmin, filtroCongregacaoId: congregacaoId);
        return View(oradores);
    }

    public async Task<IActionResult> Create()
    {
        var usuarioId = ObterUsuarioIdLogado();
        var userCongId = ObterCongregacaoIdUsuarioLogado();
        var ehSuperAdmin = EhSuperAdmin();

        ViewBag.Congregacoes = await _congregacaoService.ListarPorUsuarioAsync(usuarioId, userCongId, ehSuperAdmin, apenasAtivas: true);
        ViewBag.Discursos = await _discursoService.ListarTodosAsync(apenasAtivos: true);
        return View(new CriarOradorDto(userCongId ?? Guid.Empty, "", "", ""));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CriarOradorDto dto, List<Guid>? discursosIniciaisIds)
    {
        var usuarioId = ObterUsuarioIdLogado();
        var userCongId = ObterCongregacaoIdUsuarioLogado();
        var ehSuperAdmin = EhSuperAdmin();

        if (dto.CongregacaoId == Guid.Empty && string.IsNullOrWhiteSpace(dto.NovaCongregacaoNome) && userCongId.HasValue)
        {
            dto = dto with { CongregacaoId = userCongId.Value };
        }

        try
        {
            var dtoCompleto = dto with { DiscursosIniciaisIds = discursosIniciaisIds, UsuarioCriadorId = usuarioId };
            await _oradorService.CriarAsync(dtoCompleto);
            TempData["Success"] = "Orador cadastrado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        ViewBag.Congregacoes = await _congregacaoService.ListarPorUsuarioAsync(usuarioId, userCongId, ehSuperAdmin, apenasAtivas: true);
        ViewBag.Discursos = await _discursoService.ListarTodosAsync(apenasAtivos: true);
        return View(dto);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var orador = await _oradorService.ObterPorIdAsync(id);
        if (orador == null) return NotFound();

        if (!await PodeGerenciarOradorAsync(orador))
            return Forbid();

        var usuarioId = ObterUsuarioIdLogado();
        var userCongId = ObterCongregacaoIdUsuarioLogado();
        var ehSuperAdmin = EhSuperAdmin();

        ViewBag.Congregacoes = await _congregacaoService.ListarPorUsuarioAsync(usuarioId, userCongId, ehSuperAdmin, apenasAtivas: true);
        var dto = new AtualizarOradorDto(orador.Id, orador.CongregacaoId, orador.Nome, orador.TelefoneWhatsapp, orador.Email, orador.Ativo);
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AtualizarOradorDto dto)
    {
        var orador = await _oradorService.ObterPorIdAsync(dto.Id);
        if (orador == null) return NotFound();

        if (!await PodeGerenciarOradorAsync(orador))
            return Forbid();

        try
        {
            await _oradorService.AtualizarAsync(dto);
            TempData["Success"] = "Orador atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var usuarioId = ObterUsuarioIdLogado();
            var userCongId = ObterCongregacaoIdUsuarioLogado();
            var ehSuperAdmin = EhSuperAdmin();
            ViewBag.Congregacoes = await _congregacaoService.ListarPorUsuarioAsync(usuarioId, userCongId, ehSuperAdmin, apenasAtivas: true);
            return View(dto);
        }
    }

    public async Task<IActionResult> Repertorio(Guid id)
    {
        var orador = await _oradorService.ObterPorIdAsync(id);
        if (orador == null) return NotFound();

        if (!await PodeGerenciarOradorAsync(orador))
            return Forbid();

        ViewBag.Orador = orador;
        ViewBag.TodosDiscursos = await _discursoService.ListarTodosAsync(apenasAtivos: true);

        var discursosIdsSelecionados = orador.Repertorio.Select(r => r.Id).ToList();
        return View(new SincronizarRepertorioDto(orador.Id, discursosIdsSelecionados));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Repertorio(SincronizarRepertorioDto dto)
    {
        var orador = await _oradorService.ObterPorIdAsync(dto.OradorId);
        if (orador == null) return NotFound();

        if (!await PodeGerenciarOradorAsync(orador))
            return Forbid();

        try
        {
            await _oradorService.SincronizarRepertorioAsync(dto);
            TempData["Success"] = "Repertório de discursos do orador atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Repertorio), new { id = dto.OradorId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlternarStatus(Guid id)
    {
        var orador = await _oradorService.ObterPorIdAsync(id);
        if (orador == null) return NotFound();

        if (!await PodeGerenciarOradorAsync(orador))
            return Forbid();

        await _oradorService.AlternarStatusAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
