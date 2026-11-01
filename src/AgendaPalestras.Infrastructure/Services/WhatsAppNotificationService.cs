using System.Net.Http.Json;
using System.Text.RegularExpressions;
using AgendaPalestras.Application.Services;
using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Enums;
using AgendaPalestras.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgendaPalestras.Infrastructure.Services;

public class WhatsAppNotificationService : IWhatsAppNotificationService
{
    private readonly IAgendaRepository _agendaRepository;
    private readonly IOradorRepository _oradorRepository;
    private readonly IMensagemHistoricoRepository _historicoRepository;
    private readonly IEvolutionApiService _evolutionApiService;
    private readonly ILogger<WhatsAppNotificationService> _logger;

    public WhatsAppNotificationService(
        IAgendaRepository agendaRepository,
        IOradorRepository oradorRepository,
        IMensagemHistoricoRepository historicoRepository,
        IEvolutionApiService evolutionApiService,
        ILogger<WhatsAppNotificationService> logger)
    {
        _agendaRepository = agendaRepository;
        _oradorRepository = oradorRepository;
        _historicoRepository = historicoRepository;
        _evolutionApiService = evolutionApiService;
        _logger = logger;
    }

    public async Task<ResultadoEnvioMensagem> EnviarConvitePalestraAsync(Guid agendaId, CancellationToken cancellationToken = default)
    {
        var agenda = await _agendaRepository.ObterPorIdAsync(agendaId, cancellationToken);
        if (agenda == null)
            return new ResultadoEnvioMensagem(false, null, "Agendamento não encontrado.");

        var texto = $"Olá, irmão {agenda.Orador.Nome}!\n\n" +
                    $"Você foi convidado para proferir o discurso público na *Congregação {agenda.Congregacao.Nome}*.\n\n" +
                    $"📅 *Data:* {agenda.DataHora:dd/MM/yyyy (dddd)}\n" +
                    $"⏰ *Horário:* {agenda.DataHora:HH:mm}\n" +
                    $"📖 *Tema:* Nº {agenda.Discurso.Numero} — {agenda.Discurso.Tema}\n\n" +
                    $"Por favor, responda:\n" +
                    $"*1* para CONFIRMAR\n" +
                    $"*2* para RECUSAR";

        var resultado = await EnviarTextoAsync(agenda.Orador.TelefoneWhatsapp, texto, agenda.Id, cancellationToken);

        if (resultado.Sucesso)
        {
            agenda.MarcarConviteEnviado(DateTime.UtcNow);
            await _agendaRepository.AtualizarAsync(agenda, cancellationToken);

            var historico = new MensagemHistorico(
                agenda.Orador.TelefoneWhatsapp,
                DirecaoMensagem.Envio,
                TipoMensagem.Convite,
                StatusMensagem.Enviado,
                texto,
                agenda.Id,
                resultado.MessageId
            );
            await _historicoRepository.AdicionarAsync(historico, cancellationToken);
        }

        return resultado;
    }

    public async Task<ResultadoEnvioMensagem> EnviarLembrete7DiasAsync(Guid agendaId, CancellationToken cancellationToken = default)
    {
        var agenda = await _agendaRepository.ObterPorIdAsync(agendaId, cancellationToken);
        if (agenda == null)
            return new ResultadoEnvioMensagem(false, null, "Agendamento não encontrado.");

        if (agenda.DataLembreteEnviado.HasValue)
        {
            _logger.LogInformation("Lembrete para agenda {AgendaId} já foi enviado previamente em {Data}.", agenda.Id, agenda.DataLembreteEnviado);
            return new ResultadoEnvioMensagem(true, "JA_ENVIADO");
        }

        var texto = $"Olá, irmão {agenda.Orador.Nome}!\n\n" +
                    $"Passando para lembrar do seu discurso confirmado daqui a 7 dias:\n\n" +
                    $"🏛 *Congregação:* {agenda.Congregacao.Nome}\n" +
                    $"📅 *Data:* {agenda.DataHora:dd/MM/yyyy (dddd)}\n" +
                    $"⏰ *Horário:* {agenda.DataHora:HH:mm}\n" +
                    $"📖 *Tema:* Nº {agenda.Discurso.Numero} — {agenda.Discurso.Tema}\n\n" +
                    $"Que tenha uma excelente reunião!";

        var resultado = await EnviarTextoAsync(agenda.Orador.TelefoneWhatsapp, texto, agenda.Id, cancellationToken);

        if (resultado.Sucesso)
        {
            agenda.MarcarLembreteEnviado(DateTime.UtcNow);
            await _agendaRepository.AtualizarAsync(agenda, cancellationToken);

            var historico = new MensagemHistorico(
                agenda.Orador.TelefoneWhatsapp,
                DirecaoMensagem.Envio,
                TipoMensagem.Lembrete,
                StatusMensagem.Enviado,
                texto,
                agenda.Id,
                resultado.MessageId
            );
            await _historicoRepository.AdicionarAsync(historico, cancellationToken);
        }

        return resultado;
    }

    public async Task<ResultadoEnvioMensagem> EnviarTextoAsync(
        string telefone,
        string mensagem,
        Guid? agendaId = null,
        CancellationToken cancellationToken = default)
    {
        return await _evolutionApiService.EnviarTextoAsync(telefone, mensagem, cancellationToken);
    }

    public async Task<bool> ProcessarRespostaWebhookAsync(
        string telefone,
        string textoMensagem,
        string? messageId = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Idempotência por MessageId
        if (!string.IsNullOrWhiteSpace(messageId))
        {
            var jaExiste = await _historicoRepository.JaExisteIdentificadorProvedorAsync(messageId, cancellationToken);
            if (jaExiste)
            {
                _logger.LogInformation("Webhook descartado: MessageId {MessageId} já foi processado anteriormente.", messageId);
                return true;
            }
        }

        var telefoneLimpo = Regex.Replace(telefone ?? string.Empty, @"\D", "");
        var textoLimpo = (textoMensagem ?? string.Empty).Trim().ToLowerInvariant();

        // 2. Localizar orador
        var oradores = await _oradorRepository.ListarTodosAsync(null, false, cancellationToken);
        var orador = oradores.FirstOrDefault(o =>
            Regex.Replace(o.TelefoneWhatsapp, @"\D", "") == telefoneLimpo ||
            telefoneLimpo.EndsWith(Regex.Replace(o.TelefoneWhatsapp, @"\D", "")));

        if (orador == null)
        {
            _logger.LogWarning("Webhook recebido de número desconhecido: {Telefone}", telefone);
            return false;
        }

        // 3. Localizar palestras pendentes do orador
        var hoje = DateTime.Today;
        var futuras = await _agendaRepository.ObterPorPeriodoAsync(hoje, hoje.AddMonths(6), null, cancellationToken);
        var agendaPendente = futuras
            .Where(a => a.OradorId == orador.Id && a.Status == StatusPalestra.Pendente)
            .OrderBy(a => a.DataHora)
            .FirstOrDefault();

        // Registrar no histórico a mensagem recebida
        var historicoRecebido = new MensagemHistorico(
            telefoneLimpo,
            DirecaoMensagem.Recebimento,
            TipoMensagem.Resposta,
            StatusMensagem.Processado,
            textoMensagem ?? string.Empty,
            agendaPendente?.Id,
            messageId
        );
        await _historicoRepository.AdicionarAsync(historicoRecebido, cancellationToken);

        if (agendaPendente == null)
        {
            await EnviarTextoAsync(orador.TelefoneWhatsapp,
                $"Olá, irmão {orador.Nome}! Não localizamos nenhuma palestra pendente de resposta para seu número no momento.",
                null, cancellationToken);
            return true;
        }

        // 4. Analisar intenção (1 = Confirmação, 2 = Recusa)
        if (textoLimpo == "1" || textoLimpo.Contains("confirmar") || textoLimpo.Contains("confirmo") || textoLimpo == "sim")
        {
            agendaPendente.Confirmar();
            await _agendaRepository.AtualizarAsync(agendaPendente, cancellationToken);

            var confirmacaoMsg = $"Obrigado, irmão {orador.Nome}! Sua palestra para o dia {agendaPendente.DataHora:dd/MM/yyyy} na Congregação {agendaPendente.Congregacao?.Nome} foi *CONFIRMADA* com sucesso!";
            await EnviarTextoAsync(orador.TelefoneWhatsapp, confirmacaoMsg, agendaPendente.Id, cancellationToken);
            return true;
        }
        else if (textoLimpo == "2" || textoLimpo.Contains("recusar") || textoLimpo.Contains("recuso") || textoLimpo == "não" || textoLimpo == "nao")
        {
            agendaPendente.Recusar("Recusado pelo orador via WhatsApp");
            await _agendaRepository.AtualizarAsync(agendaPendente, cancellationToken);

            var recusaMsg = $"Compreendido, irmão {orador.Nome}. Registramos a recusa do convite para o dia {agendaPendente.DataHora:dd/MM/yyyy}. Agradecemos por nos avisar!";
            await EnviarTextoAsync(orador.TelefoneWhatsapp, recusaMsg, agendaPendente.Id, cancellationToken);
            return true;
        }
        else
        {
            var ajudaMsg = "Mensagem não compreendida. Por favor, responda apenas:\n*1* para CONFIRMAR a palestra\n*2* para RECUSAR o convite";
            await EnviarTextoAsync(orador.TelefoneWhatsapp, ajudaMsg, agendaPendente.Id, cancellationToken);
            return true;
        }
    }

    private static string FormatarTelefoneParaEnvio(string telefone)
    {
        var limpo = Regex.Replace(telefone ?? string.Empty, @"\D", "");
        if (!limpo.StartsWith("55") && limpo.Length >= 10 && limpo.Length <= 11)
            limpo = "55" + limpo;
        return limpo;
    }
}
