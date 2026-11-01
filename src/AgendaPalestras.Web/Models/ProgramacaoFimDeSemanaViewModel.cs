using AgendaPalestras.Application.DTOs;

namespace AgendaPalestras.Web.Models;

public class ProgramacaoFimDeSemanaViewModel
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public string NomeMesAno { get; set; } = string.Empty;
    public Guid? CongregacaoId { get; set; }
    public string CongregacaoNome { get; set; } = string.Empty;

    public IReadOnlyList<CongregacaoDto> Congregacoes { get; set; } = new List<CongregacaoDto>();
    public List<ItemProgramacaoSemanaViewModel> Reunioes { get; set; } = new();
}

public class ItemProgramacaoSemanaViewModel
{
    public Guid AgendaId { get; set; }
    public DateTime DataHora { get; set; }
    public string DiscursoTema { get; set; } = string.Empty;
    public int DiscursoNumero { get; set; }
    public string OradorNome { get; set; } = string.Empty;
    public string OradorCongregacaoOrigem { get; set; } = string.Empty;
    public string OradorTelefone { get; set; } = string.Empty;
    public string CongregacaoNome { get; set; } = string.Empty;
    public string StatusDescricao { get; set; } = string.Empty;

    public string? PresidenteReuniao { get; set; }
    public string? LeitorSentinela { get; set; }
    public string? OracaoFinal { get; set; }
}

public class SalvarProgramacaoReuniaoInputModel
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public Guid? CongregacaoId { get; set; }
    public List<ItemDesignacaoInputModel> Designacoes { get; set; } = new();
}

public class ItemDesignacaoInputModel
{
    public Guid AgendaId { get; set; }
    public string? PresidenteReuniao { get; set; }
    public string? LeitorSentinela { get; set; }
    public string? OracaoFinal { get; set; }
}
