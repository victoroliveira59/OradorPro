namespace AgendaPalestras.Domain.Interfaces;

public interface IConfiguracaoSistemaRepository
{
    Task<string?> ObterValorAsync(string chave, CancellationToken cancellationToken = default);
    Task SalvarValorAsync(string chave, string valor, CancellationToken cancellationToken = default);
    Task<Dictionary<string, string>> ObterTodasAsync(CancellationToken cancellationToken = default);
}
