using AgendaPalestras.Domain.Entities;

namespace AgendaPalestras.Domain.Interfaces;

public interface IOradorRepository
{
    Task<Orador?> ObterPorIdAsync(Guid id, bool incluirRepertorio = false, CancellationToken cancellationToken = default);
    Task<List<Orador>> ListarTodosAsync(Guid? congregacaoId = null, bool apenasAtivos = false, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Orador orador, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Orador orador, CancellationToken cancellationToken = default);
    Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExisteComTelefoneAsync(string telefone, Guid? ignorarId = null, CancellationToken cancellationToken = default);
}
