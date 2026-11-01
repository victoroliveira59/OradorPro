using AgendaPalestras.Domain.Common;
using AgendaPalestras.Domain.Exceptions;

namespace AgendaPalestras.Domain.Entities;

public class ConviteCadastro : BaseEntity
{
    public string Codigo { get; private set; } = string.Empty;
    public string? EmailConvidado { get; private set; }
    public Guid CriadoPorUsuarioId { get; private set; }
    public Usuario CriadoPorUsuario { get; private set; } = null!;
    public DateTime DataExpiracao { get; private set; }
    public bool Usado { get; private set; }
    public DateTime? DataUso { get; private set; }
    public Guid? UsuarioCriadoId { get; private set; }
    public Usuario? UsuarioCriado { get; private set; }

    protected ConviteCadastro() { }

    public ConviteCadastro(Guid criadoPorUsuarioId, string? emailConvidado = null, int diasValidade = 7, string? codigoPersonalizado = null)
    {
        if (criadoPorUsuarioId == Guid.Empty)
            throw new DomainException("Usuário criador do convite é obrigatório.");

        CriadoPorUsuarioId = criadoPorUsuarioId;
        EmailConvidado = string.IsNullOrWhiteSpace(emailConvidado) ? null : emailConvidado.Trim().ToLowerInvariant();
        DataExpiracao = DateTime.UtcNow.AddDays(diasValidade > 0 ? diasValidade : 7);
        Codigo = !string.IsNullOrWhiteSpace(codigoPersonalizado)
            ? codigoPersonalizado
            : Guid.NewGuid().ToString("N");
        Usado = false;
    }

    public bool EstaValido()
    {
        return !Usado && DateTime.UtcNow <= DataExpiracao;
    }

    public void MarcarComoUsado(Guid usuarioCriadoId)
    {
        if (Usado)
            throw new DomainException("Este link de convite já foi utilizado.");

        if (DateTime.UtcNow > DataExpiracao)
            throw new DomainException("Este link de convite expirou.");

        if (usuarioCriadoId == Guid.Empty)
            throw new DomainException("Usuário criado inválido.");

        Usado = true;
        DataUso = DateTime.UtcNow;
        UsuarioCriadoId = usuarioCriadoId;
        MarcarComoAtualizado();
    }

    public void Cancelar()
    {
        if (Usado)
            throw new DomainException("Não é possível cancelar um convite já utilizado.");

        DataExpiracao = DateTime.UtcNow.AddMinutes(-1);
        MarcarComoAtualizado();
    }
}
