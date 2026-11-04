using AgendaPalestras.Application.Services;
using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Enums;
using AgendaPalestras.Domain.Interfaces;
using AgendaPalestras.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgendaPalestras.UnitTests;

public class WhatsAppTests
{
    private readonly Mock<IAgendaRepository> _mockAgendaRepo = new();
    private readonly Mock<IOradorRepository> _mockOradorRepo = new();
    private readonly Mock<IMensagemHistoricoRepository> _mockHistoricoRepo = new();
    private readonly Mock<IEvolutionApiService> _mockEvolutionApi = new();
    private readonly Mock<ILogger<WhatsAppNotificationService>> _mockLogger = new();

    public WhatsAppTests()
    {
        _mockEvolutionApi.Setup(e => e.EnviarTextoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoEnvioMensagem(true, "sim_test_123"));
        _mockEvolutionApi.Setup(e => e.EnviarTextoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoEnvioMensagem(true, "sim_test_123"));
    }

    private WhatsAppNotificationService CriarService() =>
        new(
            _mockAgendaRepo.Object,
            _mockOradorRepo.Object,
            _mockHistoricoRepo.Object,
            _mockEvolutionApi.Object,
            _mockLogger.Object
        );

    [Fact]
    public async Task EnviarConvitePalestra_DeveAtualizarDataConviteEGravarHistorico()
    {
        var cong = new Congregacao("Congregacao Teste", "Campinas", "SP", "Sabado 19h", "19:00");
        var orador = new Orador(cong.Id, "Orador Valido", "11999998888");
        var discurso = new Discurso(15, "Por que confiar na Bíblia");
        var agenda = new Agenda(cong.Id, orador.Id, discurso.Id, DateTime.UtcNow.AddDays(10));

        // Injeta objetos de navegação via reflexão ou propriedades mockadas
        typeof(Agenda).GetProperty("Congregacao")?.SetValue(agenda, cong);
        typeof(Agenda).GetProperty("Orador")?.SetValue(agenda, orador);
        typeof(Agenda).GetProperty("Discurso")?.SetValue(agenda, discurso);

        _mockAgendaRepo.Setup(r => r.ObterPorIdAsync(agenda.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agenda);

        var service = CriarService();

        var resultado = await service.EnviarConvitePalestraAsync(agenda.Id);

        Assert.True(resultado.Sucesso);
        Assert.NotNull(agenda.DataConviteEnviado);
        _mockAgendaRepo.Verify(r => r.AtualizarAsync(agenda, It.IsAny<CancellationToken>()), Times.Once);
        _mockHistoricoRepo.Verify(r => r.AdicionarAsync(It.Is<MensagemHistorico>(m => m.Tipo == TipoMensagem.Convite), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnviarLembrete7Dias_QuandoNaoEnviado_DeveEnviarEMarcarData()
    {
        var cong = new Congregacao("Congregacao Central", "Campinas", "SP", "Sabado 19h", "19:00");
        var orador = new Orador(cong.Id, "Orador Confirmado", "11977776666");
        var discurso = new Discurso(30, "A esperança bíblica");
        var agenda = new Agenda(cong.Id, orador.Id, discurso.Id, DateTime.UtcNow.AddDays(7));
        agenda.Confirmar();

        typeof(Agenda).GetProperty("Congregacao")?.SetValue(agenda, cong);
        typeof(Agenda).GetProperty("Orador")?.SetValue(agenda, orador);
        typeof(Agenda).GetProperty("Discurso")?.SetValue(agenda, discurso);

        _mockAgendaRepo.Setup(r => r.ObterPorIdAsync(agenda.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agenda);

        var service = CriarService();

        var resultado = await service.EnviarLembrete7DiasAsync(agenda.Id);

        Assert.True(resultado.Sucesso);
        Assert.NotNull(agenda.DataLembreteEnviado);
        _mockAgendaRepo.Verify(r => r.AtualizarAsync(agenda, It.IsAny<CancellationToken>()), Times.Once);
        _mockEvolutionApi.Verify(e => e.EnviarTextoAsync(
            orador.TelefoneWhatsapp,
            It.Is<string>(msg => msg.Contains("cântico") && msg.Contains("mídias")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnviarLembrete7Dias_QuandoJaEnviado_DeveSerIdempotenteENaoReenviar()
    {
        var cong = new Congregacao("Congregacao Central", "Campinas", "SP", "Sabado 19h", "19:00");
        var orador = new Orador(cong.Id, "Orador Confirmado", "11977776666");
        var discurso = new Discurso(30, "A esperança bíblica");
        var agenda = new Agenda(cong.Id, orador.Id, discurso.Id, DateTime.UtcNow.AddDays(7));
        agenda.MarcarLembreteEnviado(DateTime.UtcNow.AddDays(-1));

        _mockAgendaRepo.Setup(r => r.ObterPorIdAsync(agenda.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agenda);

        var service = CriarService();

        var resultado = await service.EnviarLembrete7DiasAsync(agenda.Id);

        Assert.True(resultado.Sucesso);
        Assert.Equal("JA_ENVIADO", resultado.MessageId);
        _mockAgendaRepo.Verify(r => r.AtualizarAsync(agenda, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarRespostaWebhook_QuandoMessageIdDuplicado_DeveRetornarSemProcessar()
    {
        _mockHistoricoRepo.Setup(r => r.JaExisteIdentificadorProvedorAsync("msg_duplicada_123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = CriarService();

        var processado = await service.ProcessarRespostaWebhookAsync("11999998888", "1", "msg_duplicada_123");

        Assert.True(processado);
        // Não deve buscar oradores nem agendas
        _mockOradorRepo.Verify(r => r.ListarTodosAsync(It.IsAny<Guid?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarRespostaWebhook_Resposta1_DeveConfirmarPalestra()
    {
        var cong = new Congregacao("Congregacao A", "Cidade A", "SP", "Dom", "19h");
        var orador = new Orador(cong.Id, "Paulo Silva", "11999998888");
        var discurso = new Discurso(1, "Tema 1");
        var agenda = new Agenda(cong.Id, orador.Id, discurso.Id, DateTime.UtcNow.AddDays(5));
        agenda.MarcarConviteEnviado(DateTime.UtcNow);

        typeof(Agenda).GetProperty("Congregacao")?.SetValue(agenda, cong);
        typeof(Agenda).GetProperty("Orador")?.SetValue(agenda, orador);

        _mockHistoricoRepo.Setup(r => r.JaExisteIdentificadorProvedorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockOradorRepo.Setup(r => r.ListarTodosAsync(null, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Orador> { orador });

        _mockAgendaRepo.Setup(r => r.ObterPorPeriodoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Agenda> { agenda });

        var service = CriarService();

        var processado = await service.ProcessarRespostaWebhookAsync("11999998888", "1", "msg_nova_1");

        Assert.True(processado);
        Assert.Equal(StatusPalestra.Confirmado, agenda.Status);
        _mockAgendaRepo.Verify(r => r.AtualizarAsync(agenda, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessarRespostaWebhook_Resposta2_DeveRecusarPalestra()
    {
        var cong = new Congregacao("Congregacao B", "Cidade B", "SP", "Dom", "19h");
        var orador = new Orador(cong.Id, "Lucas Rocha", "11977778888");
        var discurso = new Discurso(2, "Tema 2");
        var agenda = new Agenda(cong.Id, orador.Id, discurso.Id, DateTime.UtcNow.AddDays(5));
        agenda.MarcarConviteEnviado(DateTime.UtcNow);

        typeof(Agenda).GetProperty("Congregacao")?.SetValue(agenda, cong);
        typeof(Agenda).GetProperty("Orador")?.SetValue(agenda, orador);

        _mockHistoricoRepo.Setup(r => r.JaExisteIdentificadorProvedorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockOradorRepo.Setup(r => r.ListarTodosAsync(null, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Orador> { orador });

        _mockAgendaRepo.Setup(r => r.ObterPorPeriodoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Agenda> { agenda });

        var service = CriarService();

        var processado = await service.ProcessarRespostaWebhookAsync("11977778888", "2", "msg_nova_2");

        Assert.True(processado);
        Assert.Equal(StatusPalestra.Recusado, agenda.Status);
        _mockAgendaRepo.Verify(r => r.AtualizarAsync(agenda, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessarRespostaWebhook_MensagemComumNaoConvite_DeveIgnorarSilenciosamente()
    {
        var cong = new Congregacao("Congregacao C", "Cidade C", "SP", "Dom", "19h");
        var orador = new Orador(cong.Id, "Marcos", "11966665555");
        var discurso = new Discurso(3, "Tema 3");
        var agenda = new Agenda(cong.Id, orador.Id, discurso.Id, DateTime.UtcNow.AddDays(5));
        agenda.MarcarConviteEnviado(DateTime.UtcNow);

        _mockHistoricoRepo.Setup(r => r.JaExisteIdentificadorProvedorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockOradorRepo.Setup(r => r.ListarTodosAsync(null, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Orador> { orador });

        _mockAgendaRepo.Setup(r => r.ObterPorPeriodoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Agenda> { agenda });

        var service = CriarService();

        // Orador manda conversa comum não relacionada a confirmação
        var processado = await service.ProcessarRespostaWebhookAsync("11966665555", "Bom dia irmão, tudo bem? Como vai a família?", "msg_comum_1");

        Assert.False(processado);
        Assert.Equal(StatusPalestra.Pendente, agenda.Status);
        _mockAgendaRepo.Verify(r => r.AtualizarAsync(agenda, It.IsAny<CancellationToken>()), Times.Never);
        _mockEvolutionApi.Verify(e => e.EnviarTextoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarRespostaWebhook_QuandoOradorNaoTemConvitePendente_DeveIgnorarSilenciosamente()
    {
        var cong = new Congregacao("Congregacao D", "Cidade D", "SP", "Dom", "19h");
        var orador = new Orador(cong.Id, "Joao", "11955554444");

        _mockHistoricoRepo.Setup(r => r.JaExisteIdentificadorProvedorAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockOradorRepo.Setup(r => r.ListarTodosAsync(null, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Orador> { orador });

        _mockAgendaRepo.Setup(r => r.ObterPorPeriodoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Agenda>());

        var service = CriarService();

        var processado = await service.ProcessarRespostaWebhookAsync("11955554444", "1", "msg_sem_convite");

        Assert.False(processado);
        _mockAgendaRepo.Verify(r => r.AtualizarAsync(It.IsAny<Agenda>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("553197024077", "(31) 99702-4077", true)]
    [InlineData("553197024077", "31997024077", true)]
    [InlineData("553197024077", "5531997024077", true)]
    [InlineData("3197024077", "31997024077", true)]
    [InlineData("553197024077", "553197024077", true)]
    [InlineData("551197024077", "5531997024077", false)] // DDDs diferentes
    [InlineData("553188884077", "5531997024077", false)] // Números diferentes
    public void TelefonesSaoCompativeis_DeveIdentificarNumerosComOuSemNonoDigito(string tel1, string tel2, bool esperado)
    {
        var resultado = WhatsAppNotificationService.TelefonesSaoCompativeis(tel1, tel2);
        Assert.Equal(esperado, resultado);
    }

    [Fact]
    public async Task ProcessarRespostaWebhook_CenarioHugoAlex_Com8DigitosNoWebhookE9DigitosNoCadastro_DeveRecusarEEnviarMensagemAmigavel()
    {
        var cong = new Congregacao("Congregação Central", "Belo Horizonte", "MG", "Domingo", "19:00");
        var orador = new Orador(cong.Id, "Hugo Alex", "(31) 99702-4077");
        var discurso = new Discurso(10, "Tema Especial");
        var agenda = new Agenda(cong.Id, orador.Id, discurso.Id, DateTime.UtcNow.AddDays(14));
        agenda.MarcarConviteEnviado(DateTime.UtcNow);

        typeof(Agenda).GetProperty("Congregacao")?.SetValue(agenda, cong);
        typeof(Agenda).GetProperty("Orador")?.SetValue(agenda, orador);
        typeof(Agenda).GetProperty("Discurso")?.SetValue(agenda, discurso);

        _mockHistoricoRepo.Setup(r => r.JaExisteIdentificadorProvedorAsync("AC8A0E4BDA2C948E2AAC3DCC6625180C", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockOradorRepo.Setup(r => r.ListarTodosAsync(null, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Orador> { orador });

        _mockAgendaRepo.Setup(r => r.ObterPorPeriodoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Agenda> { agenda });

        string? mensagemEnviada = null;
        string? telefoneEnviado = null;
        _mockEvolutionApi.Setup(e => e.EnviarTextoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((tel, msg, ct) =>
            {
                telefoneEnviado = tel;
                mensagemEnviada = msg;
            })
            .ReturnsAsync(new ResultadoEnvioMensagem(true, "evo_resp_123"));

        var service = CriarService();

        // Webhook chega com 553197024077 (8 dígitos após DDD) e texto "2"
        var processado = await service.ProcessarRespostaWebhookAsync("553197024077", "2", "AC8A0E4BDA2C948E2AAC3DCC6625180C");

        Assert.True(processado);
        Assert.Equal(StatusPalestra.Recusado, agenda.Status);
        Assert.Equal("553197024077", telefoneEnviado);
        Assert.NotNull(mensagemEnviada);
        Assert.Contains("Olá, irmão *Hugo Alex*! 🙏", mensagemEnviada);
        Assert.Contains("Agradecemos de coração", mensagemEnviada);
        _mockAgendaRepo.Verify(r => r.AtualizarAsync(agenda, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessarRespostaWebhook_QuandoEstavaRecusadoEOradorResponde1_DeveReverterParaConfirmadoELimparMotivo()
    {
        var cong = new Congregacao("Congregação Central", "Belo Horizonte", "MG", "Domingo", "19:00");
        var orador = new Orador(cong.Id, "Hugo Alex", "(31) 99702-4077");
        var discurso = new Discurso(10, "Tema Especial");
        var agenda = new Agenda(cong.Id, orador.Id, discurso.Id, DateTime.UtcNow.AddDays(14));
        agenda.MarcarConviteEnviado(DateTime.UtcNow);
        agenda.Recusar("Recusado pelo orador via WhatsApp");

        typeof(Agenda).GetProperty("Congregacao")?.SetValue(agenda, cong);
        typeof(Agenda).GetProperty("Orador")?.SetValue(agenda, orador);
        typeof(Agenda).GetProperty("Discurso")?.SetValue(agenda, discurso);

        _mockHistoricoRepo.Setup(r => r.JaExisteIdentificadorProvedorAsync("AC8A0E4BDA2C948E2AAC3DCC6625180D", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockOradorRepo.Setup(r => r.ListarTodosAsync(null, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Orador> { orador });

        _mockAgendaRepo.Setup(r => r.ObterPorPeriodoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Agenda> { agenda });

        var service = CriarService();

        // Orador agora manda "1" para confirmar
        var processado = await service.ProcessarRespostaWebhookAsync("553197024077", "1", "AC8A0E4BDA2C948E2AAC3DCC6625180D");

        Assert.True(processado);
        Assert.Equal(StatusPalestra.Confirmado, agenda.Status);
        Assert.Null(agenda.MotivoCancelamentoOuRecusa);
        _mockAgendaRepo.Verify(r => r.AtualizarAsync(agenda, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnviarConvitePalestra_ComEnderecoELinkMaps_DeveIncluirNaMensagem()
    {
        var cong = new Congregacao(
            "Congregação Central",
            "Belo Horizonte",
            "MG",
            "Domingo",
            "19:00",
            endereco: "Av. Principal, 123",
            linkGoogleMaps: "https://maps.google.com/?q=loc"
        );
        var orador = new Orador(cong.Id, "Hugo Alex", "31997024077");
        var discurso = new Discurso(10, "Tema Importante");
        var agenda = new Agenda(cong.Id, orador.Id, discurso.Id, DateTime.UtcNow.AddDays(7));

        typeof(Agenda).GetProperty("Congregacao")?.SetValue(agenda, cong);
        typeof(Agenda).GetProperty("Orador")?.SetValue(agenda, orador);
        typeof(Agenda).GetProperty("Discurso")?.SetValue(agenda, discurso);

        _mockAgendaRepo.Setup(r => r.ObterPorIdAsync(agenda.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agenda);

        string? mensagemEnviada = null;
        _mockEvolutionApi.Setup(e => e.EnviarTextoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, CancellationToken>((tel, msg, ct) => mensagemEnviada = msg)
            .ReturnsAsync(new ResultadoEnvioMensagem(true, "msg_maps_123"));

        var service = CriarService();

        var res = await service.EnviarConvitePalestraAsync(agenda.Id);

        Assert.True(res.Sucesso);
        Assert.NotNull(mensagemEnviada);
        Assert.Contains("Av. Principal, 123", mensagemEnviada);
        Assert.Contains("https://maps.google.com/?q=loc", mensagemEnviada);
    }
}

