using AgendaPalestras.Application.DTOs;

namespace AgendaPalestras.Application.Services;

public interface IConviteService
{
    Task<ConviteCadastroDto> GerarConviteAsync(Guid usuarioCriadorId, CriarConviteCadastroDto dto, string baseUrl, CancellationToken cancellationToken = default);
    Task<List<ConviteCadastroDto>> ListarTodosAsync(string baseUrl, CancellationToken cancellationToken = default);
    Task<ConviteCadastroDto?> ObterPorCodigoAsync(string codigo, string baseUrl, CancellationToken cancellationToken = default);
    Task<bool> ValidarConviteAsync(string codigo, CancellationToken cancellationToken = default);
    Task CancelarConviteAsync(Guid id, CancellationToken cancellationToken = default);
}
