using AgendaPalestras.Domain.Common;
using AgendaPalestras.Domain.Exceptions;

namespace AgendaPalestras.Domain.Entities;

public class Usuario : BaseEntity
{
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string HashSenha { get; private set; } = string.Empty;
    public string Papel { get; private set; } = "Admin";
    public Guid? CongregacaoId { get; private set; }
    public Congregacao? Congregacao { get; private set; }
    public bool Ativo { get; private set; } = true;

    // Construtor EF
    protected Usuario() { }

    public Usuario(string nome, string email, string hashSenha, string papel = "Admin", Guid? congregacaoId = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome do usuário não pode ser vazio.");
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email do usuário não pode ser vazio.");
        if (string.IsNullOrWhiteSpace(hashSenha))
            throw new DomainException("Hash de senha não pode ser vazio.");

        Nome = nome.Trim();
        Email = email.Trim().ToLowerInvariant();
        HashSenha = hashSenha;
        Papel = papel;
        CongregacaoId = congregacaoId;
        Ativo = true;
    }

    public void AssociarCongregacao(Guid congregacaoId)
    {
        if (congregacaoId == Guid.Empty)
            throw new DomainException("Congregação inválida.");

        CongregacaoId = congregacaoId;
        MarcarComoAtualizado();
    }

    public void AtualizarDados(string nome, string email)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome do usuário não pode ser vazio.");
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email do usuário não pode ser vazio.");

        Nome = nome.Trim();
        Email = email.Trim().ToLowerInvariant();
        MarcarComoAtualizado();
    }

    public void AlterarSenha(string novoHash)
    {
        if (string.IsNullOrWhiteSpace(novoHash))
            throw new DomainException("Novo hash de senha não pode ser vazio.");

        HashSenha = novoHash;
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
