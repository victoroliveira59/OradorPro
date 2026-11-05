using AgendaPalestras.Domain.Common;
using AgendaPalestras.Domain.Exceptions;

namespace AgendaPalestras.Domain.Entities;

public class Orador : BaseEntity
{
    public Guid CongregacaoId { get; private set; }
    public Congregacao Congregacao { get; private set; } = null!;
    public Guid? UsuarioCriadorId { get; private set; }

    public string Nome { get; private set; } = string.Empty;
    public string TelefoneWhatsapp { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool Ativo { get; private set; } = true;

    private readonly List<OradorDiscurso> _oradorDiscursos = new();
    public IReadOnlyCollection<OradorDiscurso> OradorDiscursos => _oradorDiscursos.AsReadOnly();

    protected Orador() { }

    public Orador(Guid congregacaoId, string nome, string telefoneWhatsapp, string email = "", Guid? usuarioCriadorId = null)
    {
        if (congregacaoId == Guid.Empty)
            throw new DomainException("Congregação de origem é obrigatória para o orador.");
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome do orador é obrigatório.");
        if (string.IsNullOrWhiteSpace(telefoneWhatsapp))
            throw new DomainException("Telefone do WhatsApp é obrigatório.");

        CongregacaoId = congregacaoId;
        Nome = nome.Trim();
        TelefoneWhatsapp = FormatarTelefone(telefoneWhatsapp);
        Email = (email ?? string.Empty).Trim().ToLowerInvariant();
        UsuarioCriadorId = usuarioCriadorId;
        Ativo = true;
    }

    public void Atualizar(Guid congregacaoId, string nome, string telefoneWhatsapp, string email)
    {
        if (congregacaoId == Guid.Empty)
            throw new DomainException("Congregação de origem é obrigatória para o orador.");
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome do orador é obrigatório.");
        if (string.IsNullOrWhiteSpace(telefoneWhatsapp))
            throw new DomainException("Telefone do WhatsApp é obrigatório.");

        CongregacaoId = congregacaoId;
        Nome = nome.Trim();
        TelefoneWhatsapp = FormatarTelefone(telefoneWhatsapp);
        Email = (email ?? string.Empty).Trim().ToLowerInvariant();
        MarcarComoAtualizado();
    }

    public void AdicionarDiscurso(Guid discursoId)
    {
        if (discursoId == Guid.Empty)
            throw new DomainException("Id do discurso inválido.");

        if (!_oradorDiscursos.Any(od => od.DiscursoId == discursoId))
        {
            _oradorDiscursos.Add(new OradorDiscurso(Id, discursoId));
            MarcarComoAtualizado();
        }
    }

    public void RemoverDiscurso(Guid discursoId)
    {
        var item = _oradorDiscursos.FirstOrDefault(od => od.DiscursoId == discursoId);
        if (item != null)
        {
            _oradorDiscursos.Remove(item);
            MarcarComoAtualizado();
        }
    }

    public void SincronizarRepertorio(IEnumerable<Guid> discursosIds)
    {
        var idsDesejados = discursosIds.Where(id => id != Guid.Empty).ToHashSet();

        // Remove os que não estão na lista
        _oradorDiscursos.RemoveAll(od => !idsDesejados.Contains(od.DiscursoId));

        // Adiciona os novos
        foreach (var id in idsDesejados)
        {
            if (!_oradorDiscursos.Any(od => od.DiscursoId == id))
            {
                _oradorDiscursos.Add(new OradorDiscurso(Id, id));
            }
        }
        MarcarComoAtualizado();
    }

    public bool PossuiDiscurso(Guid discursoId)
    {
        return _oradorDiscursos.Any(od => od.DiscursoId == discursoId);
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

    private static string FormatarTelefone(string telefone)
    {
        var apenasDigitos = new string(telefone.Where(char.IsDigit).ToArray());
        if (apenasDigitos.Length < 10)
            throw new DomainException("Telefone deve conter DDD e no mínimo 10 dígitos.");

        return apenasDigitos;
    }
}
