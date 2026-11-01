using AgendaPalestras.Domain.Entities;

namespace AgendaPalestras.Domain.Interfaces;

public interface IConviteCadastroRepository
{
    Task<ConviteCadastro?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ConviteCadastro?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<List<ConviteCadastro>> ListarTodosAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(ConviteCadastro convite, CancellationToken cancellationToken = default);
    Task AtualizarAsync(ConviteCadastro convite, CancellationToken cancellationToken = default);
}
