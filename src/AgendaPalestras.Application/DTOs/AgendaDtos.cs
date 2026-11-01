using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Enums;

namespace AgendaPalestras.Application.DTOs;

public record AgendaDto(
    Guid Id,
    Guid CongregacaoId,
    string CongregacaoNome,
    string CongregacaoCidade,
    string CongregacaoCircuito,
    Guid OradorId,
    string OradorNome,
    string OradorTelefone,
    string OradorCongregacaoOrigem,
    Guid DiscursoId,
    int DiscursoNumero,
    string DiscursoTema,
    DateTime DataHora,
    StatusPalestra Status,
    string StatusDescricao,
    string Observacoes,
    string? MotivoCancelamentoOuRecusa,
    DateTime? DataConviteEnviado,
    DateTime? DataResposta,
    DateTime? DataLembreteEnviado,
    string? PresidenteReuniao = null,
    string? LeitorSentinela = null,
    string? OracaoFinal = null)
{
    public static AgendaDto FromDomain(Agenda a)
    {
        return new AgendaDto(
            a.Id,
            a.CongregacaoId,
            a.Congregacao?.Nome ?? string.Empty,
            a.Congregacao?.Cidade ?? string.Empty,
            a.Congregacao?.Circuito ?? string.Empty,
            a.OradorId,
            a.Orador?.Nome ?? string.Empty,
            a.Orador?.TelefoneWhatsapp ?? string.Empty,
            a.Orador?.Congregacao?.Nome ?? string.Empty,
            a.DiscursoId,
            a.Discurso?.Numero ?? 0,
            a.Discurso?.Tema ?? string.Empty,
            a.DataHora,
            a.Status,
            a.Status switch
            {
                StatusPalestra.Pendente => "Pendente",
                StatusPalestra.Confirmado => "Confirmado",
                StatusPalestra.Recusado => "Recusado",
                StatusPalestra.Cancelado => "Cancelado",
                _ => "Desconhecido"
            },
            a.Observacoes,
            a.MotivoCancelamentoOuRecusa,
            a.DataConviteEnviado,
            a.DataResposta,
            a.DataLembreteEnviado,
            a.PresidenteReuniao,
            a.LeitorSentinela,
            a.OracaoFinal
        );
    }
}

public record AtualizarDesignacoesReuniaoDto(
    Guid AgendaId,
    string? PresidenteReuniao,
    string? LeitorSentinela,
    string? OracaoFinal);

public record CriarAgendaDto(
    Guid CongregacaoId,
    Guid OradorId,
    Guid DiscursoId,
    DateTime DataHora,
    string? Observacoes = "");

public record AtualizarAgendaDto(
    Guid Id,
    Guid DiscursoId,
    DateTime DataHora,
    string? Observacoes = "");

public record CancelarAgendaDto(
    Guid Id,
    string? Motivo = "");

public class AgendaCalendarioDto
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public string NomeMesAno { get; set; } = string.Empty;
    public Guid? CongregacaoId { get; set; }
    public string ModoVisualizacao { get; set; } = "calendario";
    public DateTime MesAnterior { get; set; }
    public DateTime MesProximo { get; set; }
    public IReadOnlyList<CongregacaoDto> Congregacoes { get; set; } = new List<CongregacaoDto>();
    public IReadOnlyList<AgendaDiaCalendarioDto> Dias { get; set; } = new List<AgendaDiaCalendarioDto>();
    public IReadOnlyList<AgendaDto> TodasAgendas { get; set; } = new List<AgendaDto>();
}

public class AgendaDiaCalendarioDto
{
    public DateTime Data { get; set; }
    public int Dia => Data.Day;
    public bool PertenceAoMesAtual { get; set; }
    public bool EhHoje { get; set; }
    public bool TemPalestra => Palestras.Count > 0;
    public List<AgendaDto> Palestras { get; set; } = new();
}
