using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Application.Services;
using AgendaPalestras.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaPalestras.Web.Controllers;

[Authorize]
public class AgendaController : Controller
{
    private readonly IAgendaService _agendaService;
    private readonly ICongregacaoService _congregacaoService;
    private readonly IOradorService _oradorService;
    private readonly IDiscursoService _discursoService;
    private readonly IWhatsAppNotificationService _whatsAppService;

    public AgendaController(
        IAgendaService agendaService,
        ICongregacaoService congregacaoService,
        IOradorService oradorService,
        IDiscursoService discursoService,
        IWhatsAppNotificationService whatsAppService)
    {
        _agendaService = agendaService;
        _congregacaoService = congregacaoService;
        _oradorService = oradorService;
        _discursoService = discursoService;
        _whatsAppService = whatsAppService;
    }

    public async Task<IActionResult> Index(DateTime? inicio, DateTime? fim, Guid? congregacaoId)
    {
        var dataInicio = inicio ?? DateTime.Today.AddDays(-7);
        var dataFim = fim ?? DateTime.Today.AddMonths(2);

        ViewBag.DataInicio = dataInicio.ToString("yyyy-MM-dd");
        ViewBag.DataFim = dataFim.ToString("yyyy-MM-dd");
        ViewBag.CongregacaoId = congregacaoId;
        ViewBag.Congregacoes = await _congregacaoService.ListarTodasAsync();

        var agendas = await _agendaService.ObterPorPeriodoAsync(dataInicio, dataFim, congregacaoId);
        return View(agendas);
    }

    public async Task<IActionResult> Create()
    {
        await CarregarCombosAsync();
        return View(new CriarAgendaDto(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Today.AddDays(7).AddHours(19)));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CriarAgendaDto dto)
    {
        try
        {
            var criada = await _agendaService.CriarAsync(dto);
            try
            {
                await _whatsAppService.EnviarConvitePalestraAsync(criada.Id);
                TempData["Success"] = "Palestra agendada e convite WhatsApp enviado com sucesso!";
            }
            catch
            {
                TempData["Success"] = "Palestra agendada com sucesso!";
            }
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

        await CarregarCombosAsync();
        return View(dto);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var agenda = await _agendaService.ObterPorIdAsync(id);
        if (agenda == null) return NotFound();

        var orador = await _oradorService.ObterPorIdAsync(agenda.OradorId);
        ViewBag.Orador = orador;
        ViewBag.Discursos = orador?.Repertorio ?? new List<DiscursoDto>();

        var dto = new AtualizarAgendaDto(agenda.Id, agenda.DiscursoId, agenda.DataHora, agenda.Observacoes);
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AtualizarAgendaDto dto)
    {
        try
        {
            await _agendaService.AtualizarAsync(dto);
            TempData["Success"] = "Agendamento atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        var agenda = await _agendaService.ObterPorIdAsync(dto.Id);
        if (agenda != null)
        {
            var orador = await _oradorService.ObterPorIdAsync(agenda.OradorId);
            ViewBag.Orador = orador;
            ViewBag.Discursos = orador?.Repertorio ?? new List<DiscursoDto>();
        }

        return View(dto);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var agenda = await _agendaService.ObterPorIdAsync(id);
        if (agenda == null) return NotFound();

        return View(agenda);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirmar(Guid id)
    {
        try
        {
            await _agendaService.ConfirmarAsync(id);
            TempData["Success"] = "Palestra confirmada manualmente com sucesso!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReenviarConvite(Guid id)
    {
        var res = await _whatsAppService.EnviarConvitePalestraAsync(id);
        if (res.Sucesso)
            TempData["Success"] = "Convite de WhatsApp reenviado com sucesso!";
        else
            TempData["Error"] = $"Falha ao enviar WhatsApp: {res.Erro}";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(Guid id, string? motivo)
    {
        try
        {
            await _agendaService.CancelarAsync(new CancelarAgendaDto(id, motivo));
            TempData["Success"] = "Palestra cancelada com sucesso.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> DiscursosDoOrador(Guid oradorId)
    {
        var orador = await _oradorService.ObterPorIdAsync(oradorId);
        if (orador == null) return Json(new List<object>());

        var discursos = orador.Repertorio
            .Select(d => new { id = d.Id, texto = $"Nº {d.Numero} - {d.Tema}" })
            .ToList();

        return Json(discursos);
    }

    private async Task CarregarCombosAsync()
    {
        ViewBag.Congregacoes = await _congregacaoService.ListarTodasAsync();
        ViewBag.Oradores = (await _oradorService.ListarTodosAsync()).Where(o => o.Ativo).ToList();
    }
}
