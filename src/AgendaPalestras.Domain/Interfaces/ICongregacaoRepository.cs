using AgendaPalestras.Domain.Entities;

namespace AgendaPalestras.Domain.Interfaces;

public interface ICongregacaoRepository
{
    Task<Congregacao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Congregacao>> ListarTodasAsync(bool apenasAtivas = false, CancellationToken cancellationToken = default);
    Task<List<Congregacao>> ListarPorUsuarioAsync(Guid usuarioId, Guid? congregacaoId, bool ehSuperAdmin, bool apenasAtivas = false, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Congregacao congregacao, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Congregacao congregacao, CancellationToken cancellationToken = default);
    Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExisteComNomeAsync(string nome, Guid? ignorarId = null, CancellationToken cancellationToken = default);
}
