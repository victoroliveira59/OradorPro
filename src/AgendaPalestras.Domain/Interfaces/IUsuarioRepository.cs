using AgendaPalestras.Domain.Entities;

namespace AgendaPalestras.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExisteAdminAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Usuario>> ListarTodosAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
