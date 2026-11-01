using AgendaPalestras.Application.Services;
using AgendaPalestras.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgendaPalestras.Infrastructure.BackgroundJobs;

public class Lembrete7DiasBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Lembrete7DiasBackgroundService> _logger;
    private readonly TimeSpan _intervalo = TimeSpan.FromHours(1);

    public Lembrete7DiasBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<Lembrete7DiasBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Serviço de Lembretes de 7 Dias do WhatsApp iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessarLembretesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar ciclo de lembretes de 7 dias.");
            }

            try
            {
                await Task.Delay(_intervalo, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Serviço de Lembretes de 7 Dias encerrado.");
    }

    public async Task ProcessarLembretesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var agendaRepository = scope.ServiceProvider.GetRequiredService<IAgendaRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<IWhatsAppNotificationService>();

        // Busca palestras que ocorrem daqui a 7 dias
        var dataAlvo = DateTime.UtcNow.Date.AddDays(7);
        var dataAlvoInicio = dataAlvo;
        var dataAlvoFim = dataAlvo.AddDays(1).AddTicks(-1);

        var palestras = await agendaRepository.ObterPalestrasParaLembreteAsync(dataAlvoInicio, dataAlvoFim, cancellationToken);
        _logger.LogInformation("Verificação de lembretes: {Total} palestras encontradas para o dia alvo {DataAlvo:dd/MM/yyyy}.", palestras.Count, dataAlvo);

        foreach (var palestra in palestras)
        {
            try
            {
                await notificationService.EnviarLembrete7DiasAsync(palestra.Id, cancellationToken);
                _logger.LogInformation("Lembrete disparado com sucesso para a palestra {AgendaId} (Orador: {Orador}).", palestra.Id, palestra.Orador?.Nome);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao disparar lembrete para a palestra {AgendaId}", palestra.Id);
            }
        }
    }
}
