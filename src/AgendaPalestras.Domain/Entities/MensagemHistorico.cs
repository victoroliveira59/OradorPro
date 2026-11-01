using AgendaPalestras.Domain.Common;
using AgendaPalestras.Domain.Enums;
using AgendaPalestras.Domain.Exceptions;

namespace AgendaPalestras.Domain.Entities;

public class MensagemHistorico : BaseEntity
{
    public Guid? AgendaId { get; private set; }
    public Agenda? Agenda { get; private set; }

    public string Telefone { get; private set; } = string.Empty;
    public DirecaoMensagem Direcao { get; private set; }
    public TipoMensagem Tipo { get; private set; }
    public StatusMensagem Status { get; private set; }
    public string Conteudo { get; private set; } = string.Empty;
    public string? IdentificadorProvedor { get; private set; }
    public string? ErroDetalhe { get; private set; }
    public DateTime DataRegistro { get; private set; } = DateTime.UtcNow;

    protected MensagemHistorico() { }

    public MensagemHistorico(
        string telefone,
        DirecaoMensagem direcao,
        TipoMensagem tipo,
        StatusMensagem status,
        string conteudo,
        Guid? agendaId = null,
        string? identificadorProvedor = null,
        string? erroDetalhe = null)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            throw new DomainException("Telefone é obrigatório para registrar a mensagem.");
        if (string.IsNullOrWhiteSpace(conteudo))
            throw new DomainException("Conteúdo da mensagem é obrigatório.");

        Telefone = telefone.Trim();
        Direcao = direcao;
        Tipo = tipo;
        Status = status;
        Conteudo = conteudo.Trim();
        AgendaId = agendaId;
        IdentificadorProvedor = identificadorProvedor?.Trim();
        ErroDetalhe = erroDetalhe?.Trim();
        DataRegistro = DateTime.UtcNow;
    }

    public void AtualizarStatus(StatusMensagem novoStatus, string? erroDetalhe = null)
    {
        Status = novoStatus;
        if (erroDetalhe != null)
            ErroDetalhe = erroDetalhe.Trim();
    }
}
