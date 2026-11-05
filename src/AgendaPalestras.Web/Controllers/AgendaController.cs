using System.Security.Claims;
using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Application.Services;
using AgendaPalestras.Domain.Exceptions;
using AgendaPalestras.Web.Models;
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

    private bool PodeAcessarAgenda(AgendaDto agenda)
    {
        if (EhSuperAdmin()) return true;
        var userCongId = ObterCongregacaoIdUsuarioLogado();
        return userCongId.HasValue && agenda.CongregacaoId == userCongId.Value;
    }

    public async Task<IActionResult> Index(int? ano, int? mes, Guid? congregacaoId, string modo = "calendario")
    {
        var usuarioId = ObterUsuarioIdLogado();
        var userCongId = ObterCongregacaoIdUsuarioLogado();
        var ehSuperAdmin = EhSuperAdmin();
        var filtroCongregacao = ehSuperAdmin ? congregacaoId : userCongId;

        var dataReferencia = (ano.HasValue && mes.HasValue && mes.Value >= 1 && mes.Value <= 12)
            ? new DateTime(ano.Value, mes.Value, 1)
            : new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        var anoAtual = dataReferencia.Year;
        var mesAtual = dataReferencia.Month;

        var primeiroDiaMes = new DateTime(anoAtual, mesAtual, 1);
        var ultimoDiaMes = primeiroDiaMes.AddMonths(1).AddDays(-1);

        // Grade do calendário (domingo a sábado)
        var inicioGrade = primeiroDiaMes.AddDays(-(int)primeiroDiaMes.DayOfWeek);
        var diasAposFim = (6 - (int)ultimoDiaMes.DayOfWeek + 7) % 7;
        var fimGrade = ultimoDiaMes.AddDays(diasAposFim);

        // Garantir visualização consistente com pelo menos 5 semanas (35 dias)
        var totalDias = (fimGrade - inicioGrade).Days + 1;
        if (totalDias < 35)
        {
            fimGrade = fimGrade.AddDays(35 - totalDias);
        }

        var inicioGradeUtc = DateTime.SpecifyKind(inicioGrade.Date, DateTimeKind.Utc);
        var fimGradeUtc = DateTime.SpecifyKind(fimGrade.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);

        var agendas = await _agendaService.ObterPorPeriodoAsync(inicioGradeUtc, fimGradeUtc, filtroCongregacao);
        var congregacoes = await _congregacaoService.ListarPorUsuarioAsync(usuarioId, userCongId, ehSuperAdmin, apenasAtivas: true);

        var diasGrade = new List<AgendaDiaCalendarioDto>();
        for (var d = inicioGrade.Date; d <= fimGrade.Date; d = d.AddDays(1))
        {
            var dataDia = d.Date;
            var palestrasDoDia = agendas
                .Where(a => a.DataHora.Date == dataDia)
                .OrderBy(a => a.DataHora)
                .ToList();

            diasGrade.Add(new AgendaDiaCalendarioDto
            {
                Data = dataDia,
                PertenceAoMesAtual = d.Month == mesAtual,
                EhHoje = dataDia == DateTime.Today,
                Palestras = palestrasDoDia
            });
        }

        var culturaPtBr = new System.Globalization.CultureInfo("pt-BR");
        var nomeMes = primeiroDiaMes.ToString("MMMM", culturaPtBr);
        var nomeMesCapitalizado = char.ToUpper(nomeMes[0]) + nomeMes.Substring(1);

        var viewModel = new AgendaCalendarioViewModel
        {
            Ano = anoAtual,
            Mes = mesAtual,
            NomeMesAno = $"{nomeMesCapitalizado} de {anoAtual}",
            CongregacaoId = filtroCongregacao,
            ModoVisualizacao = modo,
            MesAnterior = primeiroDiaMes.AddMonths(-1),
            MesProximo = primeiroDiaMes.AddMonths(1),
            Congregacoes = congregacoes,
            Dias = diasGrade,
            TodasAgendas = agendas.OrderBy(a => a.DataHora).ToList()
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Create(string? data, Guid? congregacaoId)
    {
        var usuarioId = ObterUsuarioIdLogado();
        var userCongId = ObterCongregacaoIdUsuarioLogado();
        var ehSuperAdmin = EhSuperAdmin();
        var congIdFinal = ehSuperAdmin ? (congregacaoId ?? userCongId) : userCongId;

        await CarregarCombosAsync(usuarioId, userCongId, ehSuperAdmin);

        DateTime dataInicial = DateTime.Today.AddDays(7).AddHours(19);
        if (!string.IsNullOrWhiteSpace(data) && DateTime.TryParse(data, out var parsedDate))
        {
            dataInicial = parsedDate.Date.AddHours(19);
        }

        return View(new CriarAgendaDto(congIdFinal ?? Guid.Empty, Guid.Empty, Guid.Empty, dataInicial));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CriarAgendaDto dto)
    {
        var usuarioId = ObterUsuarioIdLogado();
        var userCongId = ObterCongregacaoIdUsuarioLogado();
        var ehSuperAdmin = EhSuperAdmin();

        if (!ehSuperAdmin && userCongId.HasValue)
        {
            dto = dto with { CongregacaoId = userCongId.Value };
        }
        else if (dto.CongregacaoId == Guid.Empty && userCongId.HasValue)
        {
            dto = dto with { CongregacaoId = userCongId.Value };
        }

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

        await CarregarCombosAsync(usuarioId, userCongId, ehSuperAdmin);
        return View(dto);
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var agenda = await _agendaService.ObterPorIdAsync(id);
        if (agenda == null) return NotFound();

        if (!PodeAcessarAgenda(agenda)) return Forbid();

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
        var agenda = await _agendaService.ObterPorIdAsync(dto.Id);
        if (agenda == null) return NotFound();

        if (!PodeAcessarAgenda(agenda)) return Forbid();

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

        var orador = await _oradorService.ObterPorIdAsync(agenda.OradorId);
        ViewBag.Orador = orador;
        ViewBag.Discursos = orador?.Repertorio ?? new List<DiscursoDto>();

        return View(dto);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var agenda = await _agendaService.ObterPorIdAsync(id);
        if (agenda == null) return NotFound();

        if (!PodeAcessarAgenda(agenda)) return Forbid();

        return View(agenda);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirmar(Guid id)
    {
        var agenda = await _agendaService.ObterPorIdAsync(id);
        if (agenda == null) return NotFound();

        if (!PodeAcessarAgenda(agenda)) return Forbid();

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
        var agenda = await _agendaService.ObterPorIdAsync(id);
        if (agenda == null) return NotFound();

        if (!PodeAcessarAgenda(agenda)) return Forbid();

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
        var agenda = await _agendaService.ObterPorIdAsync(id);
        if (agenda == null) return NotFound();

        if (!PodeAcessarAgenda(agenda)) return Forbid();

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

    [HttpGet]
    public async Task<IActionResult> VerificarDisponibilidade(Guid congregacaoId, string data)
    {
        if (congregacaoId == Guid.Empty || string.IsNullOrWhiteSpace(data) || !DateTime.TryParse(data, out var parsedDate))
        {
            return Json(new { disponivel = true });
        }

        var conflito = await _agendaService.ExisteConflitoCongregacaoAsync(congregacaoId, parsedDate);
        return Json(new
        {
            disponivel = !conflito,
            mensagem = conflito ? "A congregação selecionada já possui um discurso/orador agendado para este dia." : null
        });
    }

    private async Task CarregarCombosAsync(Guid usuarioId, Guid? userCongId, bool ehSuperAdmin)
    {
        ViewBag.Congregacoes = await _congregacaoService.ListarPorUsuarioAsync(usuarioId, userCongId, ehSuperAdmin, apenasAtivas: true);
        ViewBag.Oradores = await _oradorService.ListarPorUsuarioAsync(usuarioId, userCongId, ehSuperAdmin, apenasAtivos: true);
    }
}
