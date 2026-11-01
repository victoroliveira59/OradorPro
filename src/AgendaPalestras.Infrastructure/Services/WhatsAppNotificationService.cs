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
        string? quotedMessageId = null,
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

        if (string.IsNullOrWhiteSpace(telefoneLimpo) || string.IsNullOrWhiteSpace(textoLimpo))
            return false;

        // 2. Localizar orador
        var oradores = await _oradorRepository.ListarTodosAsync(null, false, cancellationToken);
        var orador = oradores.FirstOrDefault(o =>
            Regex.Replace(o.TelefoneWhatsapp, @"\D", "") == telefoneLimpo ||
            telefoneLimpo.EndsWith(Regex.Replace(o.TelefoneWhatsapp, @"\D", "")));

        if (orador == null)
        {
            // Remetente não é orador cadastrado: ignorar silenciosamente para não interferir nas conversas pessoais do admin
            return false;
        }

        // 3. Localizar palestras pendentes que possuam convite enviado aguardando resposta
        var hoje = DateTime.Today;
        var futuras = await _agendaRepository.ObterPorPeriodoAsync(hoje, hoje.AddMonths(6), null, cancellationToken);
        var agendasPendentes = futuras
            .Where(a => a.OradorId == orador.Id && a.Status == StatusPalestra.Pendente && a.DataConviteEnviado.HasValue)
            .OrderBy(a => a.DataHora)
            .ToList();

        if (agendasPendentes.Count == 0)
        {
            // Orador não possui nenhum convite pendente aguardando resposta: desconsiderar silenciosamente
            _logger.LogInformation("Mensagem de {Telefone} desconsiderada: nenhum convite pendente aguardando resposta.", telefone);
            return false;
        }

        // Se o orador respondeu citando a mensagem de convite específica, associar diretamente
        Agenda? agendaAlvo = null;
        if (!string.IsNullOrWhiteSpace(quotedMessageId))
        {
            var historicoConvite = await _historicoRepository.ObterPorIdentificadorProvedorAsync(quotedMessageId, cancellationToken);
            if (historicoConvite != null && historicoConvite.AgendaId.HasValue)
            {
                agendaAlvo = agendasPendentes.FirstOrDefault(a => a.Id == historicoConvite.AgendaId.Value);
            }
        }

        agendaAlvo ??= agendasPendentes.First();

        // 4. Analisar intenção estrita (somente respostas em específico para confirmação ou recusa)
        bool isConfirmacao = IsRespostaConfirmacao(textoLimpo);
        bool isRecusa = IsRespostaRecusa(textoLimpo);

        // Se não for uma resposta em específico (ex: "bom dia", conversas comuns, outras dúvidas), desconsiderar silenciosamente!
        if (!isConfirmacao && !isRecusa)
        {
            _logger.LogInformation("Mensagem '{Texto}' de {Telefone} desconsiderada: não corresponde a resposta de convite.", textoMensagem, telefone);
            return false;
        }

        // Registrar no histórico a mensagem recebida de resposta válida
        var historicoRecebido = new MensagemHistorico(
            telefoneLimpo,
            DirecaoMensagem.Recebimento,
            TipoMensagem.Resposta,
            StatusMensagem.Processado,
            textoMensagem ?? string.Empty,
            agendaAlvo.Id,
            messageId
        );
        await _historicoRepository.AdicionarAsync(historicoRecebido, cancellationToken);

        if (isConfirmacao)
        {
            agendaAlvo.Confirmar();
            await _agendaRepository.AtualizarAsync(agendaAlvo, cancellationToken);

            var confirmacaoMsg = $"Obrigado, irmão {orador.Nome}! Sua palestra para o dia {agendaAlvo.DataHora:dd/MM/yyyy} na Congregação {agendaAlvo.Congregacao?.Nome} foi *CONFIRMADA* com sucesso!";
            await EnviarTextoAsync(orador.TelefoneWhatsapp, confirmacaoMsg, agendaAlvo.Id, cancellationToken);
            return true;
        }
        else
        {
            agendaAlvo.Recusar("Recusado pelo orador via WhatsApp");
            await _agendaRepository.AtualizarAsync(agendaAlvo, cancellationToken);

            var recusaMsg = $"Compreendido, irmão {orador.Nome}. Registramos a recusa do convite para o dia {agendaAlvo.DataHora:dd/MM/yyyy}. Agradecemos por nos avisar!";
            await EnviarTextoAsync(orador.TelefoneWhatsapp, recusaMsg, agendaAlvo.Id, cancellationToken);
            return true;
        }
    }

    private static bool IsRespostaConfirmacao(string texto)
    {
        return texto == "1" ||
               texto == "1." ||
               texto == "1 -" ||
               texto == "sim" ||
               texto == "confirmar" ||
               texto == "confirmo" ||
               texto == "confirmado" ||
               texto.StartsWith("1 ") ||
               texto.StartsWith("1-") ||
               texto == "ok, confirmo" ||
               texto == "pode confirmar";
    }

    private static bool IsRespostaRecusa(string texto)
    {
        return texto == "2" ||
               texto == "2." ||
               texto == "2 -" ||
               texto == "não" ||
               texto == "nao" ||
               texto == "recusar" ||
               texto == "recuso" ||
               texto == "recusado" ||
               texto.StartsWith("2 ") ||
               texto.StartsWith("2-") ||
               texto == "não posso" ||
               texto == "nao posso";
    }

    private static string FormatarTelefoneParaEnvio(string telefone)
    {
        var limpo = Regex.Replace(telefone ?? string.Empty, @"\D", "");
        if (!limpo.StartsWith("55") && limpo.Length >= 10 && limpo.Length <= 11)
            limpo = "55" + limpo;
        return limpo;
    }
}
