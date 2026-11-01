using AgendaPalestras.Application.DTOs;

namespace AgendaPalestras.Application.Services;

public interface IAgendaService
{
    Task<AgendaDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AgendaDto>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim, Guid? congregacaoId = null, CancellationToken cancellationToken = default);
    Task<AgendaDto> CriarAsync(CriarAgendaDto dto, CancellationToken cancellationToken = default);
    Task<AgendaDto> AtualizarAsync(AtualizarAgendaDto dto, CancellationToken cancellationToken = default);
    Task ConfirmarAsync(Guid id, CancellationToken cancellationToken = default);
    Task RecusarAsync(Guid id, string? motivo = null, CancellationToken cancellationToken = default);
    Task CancelarAsync(CancelarAgendaDto dto, CancellationToken cancellationToken = default);
}
