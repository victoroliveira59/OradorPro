namespace AgendaPalestras.Application.Services;

public record ResultadoEnvioMensagem(bool Sucesso, string? MessageId = null, string? Erro = null);

public interface IWhatsAppNotificationService
{
    Task<ResultadoEnvioMensagem> EnviarConvitePalestraAsync(Guid agendaId, CancellationToken cancellationToken = default);
    Task<ResultadoEnvioMensagem> EnviarLembrete7DiasAsync(Guid agendaId, CancellationToken cancellationToken = default);
    Task<ResultadoEnvioMensagem> EnviarTextoAsync(string telefone, string mensagem, Guid? agendaId = null, CancellationToken cancellationToken = default);
    Task<bool> ProcessarRespostaWebhookAsync(string telefone, string textoMensagem, string? messageId = null, CancellationToken cancellationToken = default);
}
