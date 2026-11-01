using AgendaPalestras.Domain.Entities;

namespace AgendaPalestras.Domain.Interfaces;

public interface IMensagemHistoricoRepository
{
    Task<MensagemHistorico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MensagemHistorico>> ObterPorAgendaIdAsync(Guid agendaId, CancellationToken cancellationToken = default);
    Task<bool> JaExisteIdentificadorProvedorAsync(string identificadorProvedor, CancellationToken cancellationToken = default);
    Task AdicionarAsync(MensagemHistorico mensagem, CancellationToken cancellationToken = default);
    Task AtualizarAsync(MensagemHistorico mensagem, CancellationToken cancellationToken = default);
}
