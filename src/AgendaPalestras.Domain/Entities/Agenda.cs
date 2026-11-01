using AgendaPalestras.Domain.Common;
using AgendaPalestras.Domain.Enums;
using AgendaPalestras.Domain.Exceptions;

namespace AgendaPalestras.Domain.Entities;

public class Agenda : BaseEntity
{
    public Guid CongregacaoId { get; private set; }
    public Congregacao Congregacao { get; private set; } = null!;

    public Guid OradorId { get; private set; }
    public Orador Orador { get; private set; } = null!;

    public Guid DiscursoId { get; private set; }
    public Discurso Discurso { get; private set; } = null!;

    public DateTime DataHora { get; private set; }
    public StatusPalestra Status { get; private set; } = StatusPalestra.Pendente;
    public string Observacoes { get; private set; } = string.Empty;
    public string? MotivoCancelamentoOuRecusa { get; private set; }

    public DateTime? DataConviteEnviado { get; private set; }
    public DateTime? DataResposta { get; private set; }
    public DateTime? DataLembreteEnviado { get; private set; }

    public string? PresidenteReuniao { get; private set; }
    public string? LeitorSentinela { get; private set; }
    public string? OracaoFinal { get; private set; }

    protected Agenda() { }

    public Agenda(
        Guid congregacaoId,
        Guid oradorId,
        Guid discursoId,
        DateTime dataHora,
        string observacoes = "")
    {
        if (congregacaoId == Guid.Empty)
            throw new DomainException("Congregação é obrigatória para o agendamento.");
        if (oradorId == Guid.Empty)
            throw new DomainException("Orador é obrigatório para o agendamento.");
        if (discursoId == Guid.Empty)
            throw new DomainException("Tema de discurso é obrigatório para o agendamento.");
        var dataHoraUtc = dataHora.Kind switch
        {
            DateTimeKind.Utc => dataHora,
            DateTimeKind.Local => dataHora.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dataHora, DateTimeKind.Utc)
        };

        if (dataHoraUtc <= DateTime.UtcNow.AddMinutes(-5))
            throw new DomainException("A data e hora do agendamento deve ser uma data futura.");

        CongregacaoId = congregacaoId;
        OradorId = oradorId;
        DiscursoId = discursoId;
        DataHora = dataHoraUtc;
        Observacoes = (observacoes ?? string.Empty).Trim();
        Status = StatusPalestra.Pendente;
    }

    public void Confirmar()
    {
        if (Status == StatusPalestra.Cancelado)
            throw new DomainException("Não é possível confirmar uma palestra que foi cancelada.");

        Status = StatusPalestra.Confirmado;
        DataResposta = DateTime.UtcNow;
    }

    public void Recusar(string? motivo = null)
    {
        if (Status == StatusPalestra.Cancelado)
            throw new DomainException("Não é possível recusar uma palestra que já está cancelada.");

        Status = StatusPalestra.Recusado;
        DataResposta = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(motivo))
            MotivoCancelamentoOuRecusa = motivo.Trim();
    }

    public void Cancelar(string? motivo = null)
    {
        if (Status == StatusPalestra.Cancelado)
            return;

        Status = StatusPalestra.Cancelado;
        if (!string.IsNullOrWhiteSpace(motivo))
            MotivoCancelamentoOuRecusa = motivo.Trim();
    }

    public void Reagendar(DateTime novaDataHora, Guid novoDiscursoId, string? observacoes = null)
    {
        if (Status == StatusPalestra.Cancelado)
            throw new DomainException("Não é possível reagendar uma palestra cancelada. Crie um novo agendamento.");
        if (novoDiscursoId == Guid.Empty)
            throw new DomainException("Tema de discurso é obrigatório.");

        var novaDataHoraUtc = novaDataHora.Kind switch
        {
            DateTimeKind.Utc => novaDataHora,
            DateTimeKind.Local => novaDataHora.ToUniversalTime(),
            _ => DateTime.SpecifyKind(novaDataHora, DateTimeKind.Utc)
        };

        if (novaDataHoraUtc <= DateTime.UtcNow.AddMinutes(-5))
            throw new DomainException("A nova data e hora deve ser no futuro.");

        DataHora = novaDataHoraUtc;
        DiscursoId = novoDiscursoId;
        if (observacoes != null)
            Observacoes = observacoes.Trim();

        // Ao alterar a data, volta para pendente para nova confirmação se já estava confirmada/recusada
        Status = StatusPalestra.Pendente;
        DataResposta = null;
        DataConviteEnviado = null;
        DataLembreteEnviado = null;
    }

    public void MarcarConviteEnviado(DateTime dataEnvio)
    {
        DataConviteEnviado = dataEnvio.Kind == DateTimeKind.Utc ? dataEnvio : dataEnvio.ToUniversalTime();
    }

    public void MarcarLembreteEnviado(DateTime dataEnvio)
    {
        DataLembreteEnviado = dataEnvio.Kind == DateTimeKind.Utc ? dataEnvio : dataEnvio.ToUniversalTime();
    }

    public void AtualizarDesignacoesReuniao(string? presidente, string? leitorSentinela, string? oracaoFinal)
    {
        PresidenteReuniao = string.IsNullOrWhiteSpace(presidente) ? null : presidente.Trim();
        LeitorSentinela = string.IsNullOrWhiteSpace(leitorSentinela) ? null : leitorSentinela.Trim();
        OracaoFinal = string.IsNullOrWhiteSpace(oracaoFinal) ? null : oracaoFinal.Trim();
        MarcarComoAtualizado();
    }
}
