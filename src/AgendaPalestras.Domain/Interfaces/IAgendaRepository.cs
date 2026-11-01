using AgendaPalestras.Domain.Entities;

namespace AgendaPalestras.Domain.Interfaces;

public interface IAgendaRepository
{
    Task<Agenda?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Agenda>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim, Guid? congregacaoId = null, CancellationToken cancellationToken = default);
    Task<bool> ExisteConflitoOradorAsync(Guid oradorId, DateTime dataHora, Guid? ignorarAgendaId = null, CancellationToken cancellationToken = default);
    Task<bool> ExisteConflitoCongregacaoAsync(Guid congregacaoId, DateTime dataHora, Guid? ignorarAgendaId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Agenda>> ObterPalestrasParaLembreteAsync(DateTime dataAlvoInicio, DateTime dataAlvoFim, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Agenda agenda, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Agenda agenda, CancellationToken cancellationToken = default);
}
