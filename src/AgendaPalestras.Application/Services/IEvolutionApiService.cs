using AgendaPalestras.Application.DTOs;

namespace AgendaPalestras.Application.Services;

public interface IEvolutionApiService
{
    Task<ConfiguracaoWhatsAppDto> ObterConfiguracaoAsync(CancellationToken cancellationToken = default);
    Task SalvarConfiguracaoAsync(ConfiguracaoWhatsAppDto dto, CancellationToken cancellationToken = default);
    Task<EvolutionConnectionStatusDto> ObterStatusConexaoAsync(CancellationToken cancellationToken = default);
    Task<EvolutionQrCodeDto> ObterOuGerarQrCodeAsync(CancellationToken cancellationToken = default);
    Task<bool> DesconectarAsync(CancellationToken cancellationToken = default);
    Task<ResultadoEnvioMensagem> EnviarTextoAsync(string telefone, string mensagem, CancellationToken cancellationToken = default);
    Task<bool> ConfigurarWebhookAsync(string webhookUrl, CancellationToken cancellationToken = default);
}
