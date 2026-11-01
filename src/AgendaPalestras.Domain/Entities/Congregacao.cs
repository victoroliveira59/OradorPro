using AgendaPalestras.Domain.Common;
using AgendaPalestras.Domain.Exceptions;

namespace AgendaPalestras.Domain.Entities;

public class Congregacao : BaseEntity
{
    public Guid? UsuarioCriadorId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Cidade { get; private set; } = string.Empty;
    public string Circuito { get; private set; } = string.Empty;
    public string DiasReuniao { get; private set; } = string.Empty;
    public string HorarioReuniao { get; private set; } = string.Empty;
    public string? Endereco { get; private set; }
    public string? LinkGoogleMaps { get; private set; }
    public bool Ativo { get; private set; } = true;

    // Coleções de navegação
    private readonly List<Orador> _oradores = new();
    public IReadOnlyCollection<Orador> Oradores => _oradores.AsReadOnly();

    protected Congregacao() { }

    public Congregacao(
        string nome,
        string cidade,
        string circuito,
        string diasReuniao,
        string horarioReuniao,
        Guid? usuarioCriadorId = null,
        string? endereco = null,
        string? linkGoogleMaps = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome da congregação é obrigatório.");

        Nome = nome.Trim();
        Cidade = (cidade ?? string.Empty).Trim();
        Circuito = (circuito ?? string.Empty).Trim();
        DiasReuniao = (diasReuniao ?? string.Empty).Trim();
        HorarioReuniao = (horarioReuniao ?? string.Empty).Trim();
        Endereco = string.IsNullOrWhiteSpace(endereco) ? null : endereco.Trim();
        LinkGoogleMaps = string.IsNullOrWhiteSpace(linkGoogleMaps) ? null : linkGoogleMaps.Trim();
        UsuarioCriadorId = usuarioCriadorId;
        Ativo = true;
    }

    public void Atualizar(
        string nome,
        string cidade,
        string circuito,
        string diasReuniao,
        string horarioReuniao,
        string? endereco = null,
        string? linkGoogleMaps = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome da congregação é obrigatório.");

        Nome = nome.Trim();
        Cidade = (cidade ?? string.Empty).Trim();
        Circuito = (circuito ?? string.Empty).Trim();
        DiasReuniao = (diasReuniao ?? string.Empty).Trim();
        HorarioReuniao = (horarioReuniao ?? string.Empty).Trim();
        Endereco = string.IsNullOrWhiteSpace(endereco) ? null : endereco.Trim();
        LinkGoogleMaps = string.IsNullOrWhiteSpace(linkGoogleMaps) ? null : linkGoogleMaps.Trim();
        MarcarComoAtualizado();
    }

    public void Ativar()
    {
        Ativo = true;
        MarcarComoAtualizado();
    }

    public void Desativar()
    {
        Ativo = false;
        MarcarComoAtualizado();
    }
}
