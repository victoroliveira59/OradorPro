using System.Globalization;
using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Application.Services;
using AgendaPalestras.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendaPalestras.Web.Controllers;

[Authorize]
public class ProgramacaoController : Controller
{
    private readonly IAgendaService _agendaService;
    private readonly ICongregacaoService _congregacaoService;
    private readonly IProgramacaoPdfService _pdfService;

    public ProgramacaoController(
        IAgendaService agendaService,
        ICongregacaoService congregacaoService,
        IProgramacaoPdfService pdfService)
    {
        _agendaService = agendaService;
        _congregacaoService = congregacaoService;
        _pdfService = pdfService;
    }

    private Guid? ObterCongregacaoIdUsuarioLogado()
    {
        var claim = User.FindFirst("CongregacaoId")?.Value;
        if (Guid.TryParse(claim, out var id)) return id;
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? ano, int? mes, Guid? congregacaoId)
    {
        var userCongId = ObterCongregacaoIdUsuarioLogado();
        if (userCongId.HasValue)
        {
            congregacaoId = userCongId.Value;
        }

        var dataReferencia = (ano.HasValue && mes.HasValue && mes.Value >= 1 && mes.Value <= 12)
            ? new DateTime(ano.Value, mes.Value, 1)
            : new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        var anoAtual = dataReferencia.Year;
        var mesAtual = dataReferencia.Month;

        var primeiroDiaMes = new DateTime(anoAtual, mesAtual, 1);
        var ultimoDiaMes = primeiroDiaMes.AddMonths(1).AddDays(-1);

        var inicioMesUtc = DateTime.SpecifyKind(primeiroDiaMes.Date, DateTimeKind.Utc);
        var fimMesUtc = DateTime.SpecifyKind(ultimoDiaMes.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);

        var congregacoes = userCongId.HasValue
            ? (await _congregacaoService.ObterPorIdAsync(userCongId.Value) is { } c ? new List<CongregacaoDto> { c } : new List<CongregacaoDto>())
            : await _congregacaoService.ListarTodasAsync();

        if (!congregacaoId.HasValue && congregacoes.Count == 1)
        {
            congregacaoId = congregacoes[0].Id;
        }

        var agendas = await _agendaService.ObterPorPeriodoAsync(inicioMesUtc, fimMesUtc, congregacaoId);

        var culturaPtBr = new CultureInfo("pt-BR");
        var nomeMes = primeiroDiaMes.ToString("MMMM", culturaPtBr);
        var nomeMesCapitalizado = char.ToUpper(nomeMes[0]) + nomeMes.Substring(1);

        var congregacaoSelecionada = congregacoes.FirstOrDefault(c => c.Id == congregacaoId);
        var nomeCongregacao = congregacaoSelecionada?.Nome ?? (congregacaoId.HasValue ? "Congregação Selecionada" : "Todas as Congregações");

        var viewModel = new ProgramacaoFimDeSemanaViewModel
        {
            Ano = anoAtual,
            Mes = mesAtual,
            NomeMesAno = $"{nomeMesCapitalizado} de {anoAtual}",
            CongregacaoId = congregacaoId,
            CongregacaoNome = nomeCongregacao,
            Congregacoes = congregacoes,
            Reunioes = agendas.OrderBy(a => a.DataHora).Select(a => new ItemProgramacaoSemanaViewModel
            {
                AgendaId = a.Id,
                DataHora = a.DataHora,
                DiscursoTema = a.DiscursoTema,
                DiscursoNumero = a.DiscursoNumero,
                OradorNome = a.OradorNome,
                OradorCongregacaoOrigem = a.OradorCongregacaoOrigem,
                OradorTelefone = a.OradorTelefone,
                CongregacaoNome = a.CongregacaoNome,
                StatusDescricao = a.StatusDescricao,
                PresidenteReuniao = a.PresidenteReuniao,
                LeitorSentinela = a.LeitorSentinela,
                OracaoFinal = a.OracaoFinal
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Salvar(SalvarProgramacaoReuniaoInputModel model)
    {
        if (model.Designacoes != null && model.Designacoes.Count > 0)
        {
            var dtos = model.Designacoes.Select(d => new AtualizarDesignacoesReuniaoDto(
                d.AgendaId,
                d.PresidenteReuniao,
                d.LeitorSentinela,
                d.OracaoFinal
            ));

            await _agendaService.SalvarDesignacoesEmLoteAsync(dtos);
            TempData["MensagemSucesso"] = "Designações da programação salvas com sucesso!";
        }

        return RedirectToAction(nameof(Index), new { ano = model.Ano, mes = model.Mes, congregacaoId = model.CongregacaoId });
    }

    [HttpGet]
    public async Task<IActionResult> ExportarPdf(int ano, int mes, Guid? congregacaoId)
    {
        var dataReferencia = (mes >= 1 && mes <= 12)
            ? new DateTime(ano, mes, 1)
            : new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        var primeiroDiaMes = new DateTime(dataReferencia.Year, dataReferencia.Month, 1);
        var ultimoDiaMes = primeiroDiaMes.AddMonths(1).AddDays(-1);

        var inicioMesUtc = DateTime.SpecifyKind(primeiroDiaMes.Date, DateTimeKind.Utc);
        var fimMesUtc = DateTime.SpecifyKind(ultimoDiaMes.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);

        var userCongId = ObterCongregacaoIdUsuarioLogado();
        if (userCongId.HasValue)
        {
            congregacaoId = userCongId.Value;
        }

        var congregacoes = userCongId.HasValue
            ? (await _congregacaoService.ObterPorIdAsync(userCongId.Value) is { } c ? new List<CongregacaoDto> { c } : new List<CongregacaoDto>())
            : await _congregacaoService.ListarTodasAsync();

        if (!congregacaoId.HasValue && congregacoes.Count == 1)
        {
            congregacaoId = congregacoes[0].Id;
        }

        var agendas = await _agendaService.ObterPorPeriodoAsync(inicioMesUtc, fimMesUtc, congregacaoId);

        var congregacaoSelecionada = congregacoes.FirstOrDefault(c => c.Id == congregacaoId);
        var nomeCongregacao = congregacaoSelecionada?.Nome ?? "Todas as Congregações";

        var pdfBytes = _pdfService.GerarPdfProgramacaoFimDeSemana(nomeCongregacao, dataReferencia.Month, dataReferencia.Year, agendas);

        var nomeArquivo = $"Programacao_FimDeSemana_{dataReferencia.Month:D2}_{dataReferencia.Year}.pdf";
        return File(pdfBytes, "application/pdf", nomeArquivo);
    }
}
