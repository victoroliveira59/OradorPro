using AgendaPalestras.Domain.Entities;

namespace AgendaPalestras.Domain.Interfaces;

public interface IDiscursoRepository
{
    Task<Discurso?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Discurso?> ObterPorNumeroAsync(int numero, CancellationToken cancellationToken = default);
    Task<List<Discurso>> ListarTodosAsync(bool apenasAtivos = false, CancellationToken cancellationToken = default);
    Task<List<Discurso>> ListarPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Discurso discurso, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Discurso discurso, CancellationToken cancellationToken = default);
    Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default);
}
