using AgendaPalestras.Domain.Common;
using AgendaPalestras.Domain.Exceptions;

namespace AgendaPalestras.Domain.Entities;

public class Discurso : BaseEntity
{
    public int Numero { get; private set; }
    public string Tema { get; private set; } = string.Empty;
    public bool Ativo { get; private set; } = true;

    private readonly List<OradorDiscurso> _oradorDiscursos = new();
    public IReadOnlyCollection<OradorDiscurso> OradorDiscursos => _oradorDiscursos.AsReadOnly();

    protected Discurso() { }

    public Discurso(int numero, string tema)
    {
        if (numero <= 0)
            throw new DomainException("Número do discurso deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(tema))
            throw new DomainException("Tema do discurso é obrigatório.");

        Numero = numero;
        Tema = tema.Trim();
        Ativo = true;
    }

    public void Atualizar(int numero, string tema)
    {
        if (numero <= 0)
            throw new DomainException("Número do discurso deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(tema))
            throw new DomainException("Tema do discurso é obrigatório.");

        Numero = numero;
        Tema = tema.Trim();
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
