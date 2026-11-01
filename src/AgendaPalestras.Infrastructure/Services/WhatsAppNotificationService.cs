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

        var enderecoInfo = !string.IsNullOrWhiteSpace(agenda.Congregacao?.Endereco)
            ? $"\n📍 *Endereço:* {agenda.Congregacao.Endereco}"
            : "";
        var mapsInfo = !string.IsNullOrWhiteSpace(agenda.Congregacao?.LinkGoogleMaps)
            ? $"\n🗺 *Google Maps:* {agenda.Congregacao.LinkGoogleMaps}"
            : "";

        var texto = $"Olá, irmão {agenda.Orador.Nome}!\n\n" +
                    $"Você foi convidado para proferir o discurso público na *Congregação {agenda.Congregacao.Nome}*.\n\n" +
                    $"📅 *Data:* {agenda.DataHora:dd/MM/yyyy (dddd)}\n" +
                    $"⏰ *Horário:* {agenda.DataHora:HH:mm}{enderecoInfo}{mapsInfo}\n" +
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

        var enderecoLembrete = !string.IsNullOrWhiteSpace(agenda.Congregacao?.Endereco)
            ? $"\n📍 *Endereço:* {agenda.Congregacao.Endereco}"
            : "";
        var mapsLembrete = !string.IsNullOrWhiteSpace(agenda.Congregacao?.LinkGoogleMaps)
            ? $"\n🗺 *Google Maps:* {agenda.Congregacao.LinkGoogleMaps}"
            : "";

        var texto = $"Olá, irmão {agenda.Orador.Nome}!\n\n" +
                    $"Passando para lembrar do seu discurso confirmado daqui a 7 dias:\n\n" +
                    $"🏛 *Congregação:* {agenda.Congregacao.Nome}\n" +
                    $"📅 *Data:* {agenda.DataHora:dd/MM/yyyy (dddd)}\n" +
                    $"⏰ *Horário:* {agenda.DataHora:HH:mm}{enderecoLembrete}{mapsLembrete}\n" +
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
        var orador = oradores.FirstOrDefault(o => TelefonesSaoCompativeis(o.TelefoneWhatsapp, telefoneLimpo));

        if (orador == null)
        {
            // Remetente não é orador cadastrado: ignorar silenciosamente para não interferir nas conversas pessoais do admin
            _logger.LogInformation("Mensagem recebida de {Telefone}, mas nenhum orador cadastrado corresponde ao número.", telefone);
            return false;
        }

        // 3. Analisar intenção estrita (somente respostas em específico para confirmação ou recusa)
        bool isConfirmacao = IsRespostaConfirmacao(textoLimpo);
        bool isRecusa = IsRespostaRecusa(textoLimpo);

        // Se não for uma resposta em específico (ex: "bom dia", conversas comuns, outras dúvidas), desconsiderar silenciosamente!
        if (!isConfirmacao && !isRecusa)
        {
            _logger.LogInformation("Mensagem '{Texto}' de {Telefone} ({Orador}) desconsiderada: não corresponde a resposta de convite.", textoMensagem, telefone, orador.Nome);
            return false;
        }

        // 4. Localizar palestras que possuam convite enviado aguardando resposta
        var hoje = DateTime.Today;
        var futuras = await _agendaRepository.ObterPorPeriodoAsync(hoje, hoje.AddMonths(6), null, cancellationToken);
        var agendasPendentes = futuras
            .Where(a => a.OradorId == orador.Id && a.Status == StatusPalestra.Pendente && a.DataConviteEnviado.HasValue)
            .OrderBy(a => a.DataHora)
            .ToList();

        // Se o orador confirmou mas não há palestra pendente, verificar se estava como Recusada para reverter e confirmar
        if (agendasPendentes.Count == 0 && isConfirmacao)
        {
            agendasPendentes = futuras
                .Where(a => a.OradorId == orador.Id && a.Status == StatusPalestra.Recusado && a.DataConviteEnviado.HasValue)
                .OrderBy(a => a.DataHora)
                .ToList();
        }

        if (agendasPendentes.Count == 0)
        {
            // Orador não possui nenhum convite elegível aguardando resposta: desconsiderar silenciosamente
            _logger.LogInformation("Mensagem de {Telefone} ({Orador}) desconsiderada: nenhum convite pendente aguardando resposta.", telefone, orador.Nome);
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

        var numeroDestino = !string.IsNullOrWhiteSpace(telefone) ? telefone : orador.TelefoneWhatsapp;
        var congregacaoNome = agendaAlvo.Congregacao?.Nome ?? "designada";

        if (isConfirmacao)
        {
            agendaAlvo.Confirmar();
            await _agendaRepository.AtualizarAsync(agendaAlvo, cancellationToken);

            var temaInfo = agendaAlvo.Discurso != null ? $"\n📖 *Tema:* Nº {agendaAlvo.Discurso.Numero} — {agendaAlvo.Discurso.Tema}" : "";
            var enderecoInfo = !string.IsNullOrWhiteSpace(agendaAlvo.Congregacao?.Endereco)
                ? $"\n📍 *Endereço:* {agendaAlvo.Congregacao.Endereco}"
                : "";
            var mapsInfo = !string.IsNullOrWhiteSpace(agendaAlvo.Congregacao?.LinkGoogleMaps)
                ? $"\n🗺 *Google Maps:* {agendaAlvo.Congregacao.LinkGoogleMaps}"
                : "";

            var confirmacaoMsg = $"Olá, irmão *{orador.Nome}*! 😊\n\n" +
                                 $"Recebemos sua confirmação com muita alegria! Sua palestra na *Congregação {congregacaoNome}* para o dia *{agendaAlvo.DataHora:dd/MM/yyyy}* às *{agendaAlvo.DataHora:HH:mm}* está *CONFIRMADA* com sucesso.{temaInfo}{enderecoInfo}{mapsInfo}\n\n" +
                                 $"Muito obrigado por sua amorosa disposição em nos servir. Que Jeová abençoe ricamente sua preparação! Nos vemos em breve. 🤝";

            await EnviarTextoAsync(numeroDestino, confirmacaoMsg, agendaAlvo.Id, cancellationToken);
            return true;
        }
        else
        {
            agendaAlvo.Recusar("Recusado pelo orador via WhatsApp");
            await _agendaRepository.AtualizarAsync(agendaAlvo, cancellationToken);

            var recusaMsg = $"Olá, irmão *{orador.Nome}*! 🙏\n\n" +
                            $"Compreendemos perfeitamente a sua resposta. Registramos que você *não poderá* comparecer na *Congregação {congregacaoNome}* no dia *{agendaAlvo.DataHora:dd/MM/yyyy}*.\n\n" +
                            $"Agradecemos de coração por nos avisar com antecedência! Esperamos poder contar com você em uma próxima oportunidade. Desejamos ricas bênçãos para você e sua família. Tenha uma excelente semana! 🤝";

            await EnviarTextoAsync(numeroDestino, recusaMsg, agendaAlvo.Id, cancellationToken);
            return true;
        }
    }

    public static bool TelefonesSaoCompativeis(string? tel1, string? tel2)
    {
        if (string.IsNullOrWhiteSpace(tel1) || string.IsNullOrWhiteSpace(tel2))
            return false;

        var d1 = Regex.Replace(tel1, @"\D", "");
        var d2 = Regex.Replace(tel2, @"\D", "");

        if (d1 == d2) return true;
        if (d1.EndsWith(d2) || d2.EndsWith(d1)) return true;

        // Remover DDI 55 se houver
        var s1 = d1.StartsWith("55") && d1.Length >= 12 ? d1.Substring(2) : d1;
        var s2 = d2.StartsWith("55") && d2.Length >= 12 ? d2.Substring(2) : d2;

        // Remover zero inicial do DDD (ex: 031 -> 31)
        if (s1.StartsWith("0") && s1.Length >= 11) s1 = s1.Substring(1);
        if (s2.StartsWith("0") && s2.Length >= 11) s2 = s2.Substring(1);

        if (s1 == s2) return true;
        if (s1.EndsWith(s2) || s2.EndsWith(s1)) return true;

        // Compatibilidade Brasil 8 vs 9 dígitos após DDD (DDD igual + 8 últimos dígitos iguais)
        if (s1.Length >= 10 && s2.Length >= 10)
        {
            var ddd1 = s1.Substring(0, 2);
            var ddd2 = s2.Substring(0, 2);
            var final1 = s1.Substring(s1.Length - 8);
            var final2 = s2.Substring(s2.Length - 8);

            if (ddd1 == ddd2 && final1 == final2)
                return true;
        }

        return false;
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
               texto.Contains("confirmo") ||
               texto.Contains("confirmar") ||
               texto.Contains("pode confirmar") ||
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
               texto.Contains("não posso") ||
               texto.Contains("nao posso") ||
               texto.Contains("não poderei") ||
               texto.Contains("nao poderei") ||
               texto.Contains("infelizmente não") ||
               texto.Contains("infelizmente nao");
    }

    private static string FormatarTelefoneParaEnvio(string telefone)
    {
        var limpo = Regex.Replace(telefone ?? string.Empty, @"\D", "");
        if (!limpo.StartsWith("55") && limpo.Length >= 10 && limpo.Length <= 11)
            limpo = "55" + limpo;
        return limpo;
    }
}
