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
    DateTime? DataLembreteEnviado)
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
            a.DataLembreteEnviado
        );
    }
}

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
