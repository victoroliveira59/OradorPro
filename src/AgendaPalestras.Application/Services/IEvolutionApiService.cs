using AgendaPalestras.Application.DTOs;

namespace AgendaPalestras.Application.Services;

public interface IEvolutionApiService
{
    Task<ConfiguracaoWhatsAppDto> ObterConfiguracaoAsync(CancellationToken cancellationToken = default);
    Task<ConfiguracaoWhatsAppDto?> ObterConfiguracaoPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task SalvarConfiguracaoAsync(ConfiguracaoWhatsAppDto dto, CancellationToken cancellationToken = default);
    Task SalvarConfiguracaoUsuarioAsync(SalvarInstanciaUsuarioDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UsuarioWhatsAppInstanciaDto>> ListarUsuariosInstanciasAsync(CancellationToken cancellationToken = default);
    Task<EvolutionConnectionStatusDto> ObterStatusConexaoAsync(string? instanceName = null, CancellationToken cancellationToken = default);
    Task<EvolutionQrCodeDto> ObterOuGerarQrCodeAsync(string? instanceName = null, CancellationToken cancellationToken = default);
    Task<bool> DesconectarAsync(string? instanceName = null, CancellationToken cancellationToken = default);
    Task<ResultadoEnvioMensagem> EnviarTextoAsync(string telefone, string mensagem, CancellationToken cancellationToken = default);
    Task<ResultadoEnvioMensagem> EnviarTextoAsync(string telefone, string mensagem, string? instanceName, CancellationToken cancellationToken = default);
    Task<bool> ConfigurarWebhookAsync(string webhookUrl, string? instanceName = null, CancellationToken cancellationToken = default);
    Task<bool> CriarInstanciaAsync(string instanceName, CancellationToken cancellationToken = default);
}
