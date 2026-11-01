using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Application.Services;
using AgendaPalestras.Application.Validators;
using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Enums;
using AgendaPalestras.Domain.Exceptions;
using AgendaPalestras.Domain.Interfaces;
using Moq;
using Xunit;

namespace AgendaPalestras.UnitTests;

public class AgendaTests
{
    [Fact]
    public void CriarAgenda_ComDadosValidos_DeveInicializarComStatusPendente()
    {
        var congId = Guid.NewGuid();
        var oradorId = Guid.NewGuid();
        var discursoId = Guid.NewGuid();
        var dataHora = DateTime.UtcNow.AddDays(10);

        var agenda = new Agenda(congId, oradorId, discursoId, dataHora, "Reunião de fim de semana");

        Assert.NotEqual(Guid.Empty, agenda.Id);
        Assert.Equal(congId, agenda.CongregacaoId);
        Assert.Equal(oradorId, agenda.OradorId);
        Assert.Equal(discursoId, agenda.DiscursoId);
        Assert.Equal(dataHora, agenda.DataHora);
        Assert.Equal(StatusPalestra.Pendente, agenda.Status);
        Assert.Null(agenda.DataConviteEnviado);
        Assert.Null(agenda.DataResposta);
        Assert.Null(agenda.DataLembreteEnviado);
    }

    [Fact]
    public void CriarAgenda_ComDataNoPassado_DeveLancarDomainException()
    {
        var congId = Guid.NewGuid();
        var oradorId = Guid.NewGuid();
        var discursoId = Guid.NewGuid();
        var dataPassada = DateTime.UtcNow.AddDays(-2);

        Assert.Throws<DomainException>(() => new Agenda(congId, oradorId, discursoId, dataPassada));
    }

    [Fact]
    public void CriarAgenda_SemIdsObrigatorios_DeveLancarDomainException()
    {
        var data = DateTime.UtcNow.AddDays(5);
        Assert.Throws<DomainException>(() => new Agenda(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), data));
        Assert.Throws<DomainException>(() => new Agenda(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), data));
        Assert.Throws<DomainException>(() => new Agenda(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, data));
    }

    [Fact]
    public void Confirmar_QuandoPendente_DeveAlterarStatusParaConfirmado()
    {
        var agenda = new Agenda(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(5));
        agenda.Confirmar();

        Assert.Equal(StatusPalestra.Confirmado, agenda.Status);
        Assert.NotNull(agenda.DataResposta);
    }

    [Fact]
    public void Confirmar_QuandoCancelado_DeveLancarDomainException()
    {
        var agenda = new Agenda(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(5));
        agenda.Cancelar("Problema de saúde");

        Assert.Throws<DomainException>(() => agenda.Confirmar());
    }

    [Fact]
    public void Recusar_ComMotivo_DeveAlterarStatusParaRecusadoEGravarMotivo()
    {
        var agenda = new Agenda(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(5));
        agenda.Recusar("Viagem a trabalho");

        Assert.Equal(StatusPalestra.Recusado, agenda.Status);
        Assert.NotNull(agenda.DataResposta);
        Assert.Equal("Viagem a trabalho", agenda.MotivoCancelamentoOuRecusa);
    }

    [Fact]
    public void Cancelar_DeveAlterarStatusParaCancelado()
    {
        var agenda = new Agenda(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(5));
        agenda.Cancelar("Cancelamento solicitado");

        Assert.Equal(StatusPalestra.Cancelado, agenda.Status);
        Assert.Equal("Cancelamento solicitado", agenda.MotivoCancelamentoOuRecusa);
    }

    [Fact]
    public void Reagendar_ComNovaData_DeveAlterarDataEResetarStatusParaPendente()
    {
        var discurso1 = Guid.NewGuid();
        var discurso2 = Guid.NewGuid();
        var agenda = new Agenda(Guid.NewGuid(), Guid.NewGuid(), discurso1, DateTime.UtcNow.AddDays(5));
        agenda.Confirmar();

        var novaData = DateTime.UtcNow.AddDays(15);
        agenda.Reagendar(novaData, discurso2, "Mudança de tema e data");

        Assert.Equal(novaData, agenda.DataHora);
        Assert.Equal(discurso2, agenda.DiscursoId);
        Assert.Equal(StatusPalestra.Pendente, agenda.Status);
        Assert.Null(agenda.DataResposta);
    }

    [Fact]
    public async Task ServicoAgenda_AoCriarComDiscursoForaDoRepertorio_DeveLancarDomainException()
    {
        var mockAgendaRepo = new Mock<IAgendaRepository>();
        var mockCongRepo = new Mock<ICongregacaoRepository>();
        var mockOradorRepo = new Mock<IOradorRepository>();
        var mockDiscursoRepo = new Mock<IDiscursoRepository>();

        var cong = new Congregacao("Central", "São Paulo", "SP", "Circuito SP-01", "Domingo 19:00");
        var orador = new Orador(cong.Id, "Orador Teste", "11988887777");
        var discursoCadastrado = new Discurso(10, "Tema A");
        var discursoForaDoRepertorio = new Discurso(20, "Tema B");

        // Orador só tem discursoCadastrado
        orador.AdicionarDiscurso(discursoCadastrado.Id);

        mockCongRepo.Setup(r => r.ObterPorIdAsync(cong.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cong);
        mockOradorRepo.Setup(r => r.ObterPorIdAsync(orador.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orador);
        mockDiscursoRepo.Setup(r => r.ObterPorIdAsync(discursoForaDoRepertorio.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(discursoForaDoRepertorio);

        var service = new AgendaService(
            mockAgendaRepo.Object,
            mockCongRepo.Object,
            mockOradorRepo.Object,
            mockDiscursoRepo.Object,
            new CriarAgendaValidator());

        var dto = new CriarAgendaDto(cong.Id, orador.Id, discursoForaDoRepertorio.Id, DateTime.UtcNow.AddDays(7));

        var ex = await Assert.ThrowsAsync<DomainException>(() => service.CriarAsync(dto));
        Assert.Contains("não faz parte do repertório", ex.Message);
    }

    [Fact]
    public async Task ServicoAgenda_AoCriarComConflitoDeOrador_DeveLancarDomainException()
    {
        var mockAgendaRepo = new Mock<IAgendaRepository>();
        var mockCongRepo = new Mock<ICongregacaoRepository>();
        var mockOradorRepo = new Mock<IOradorRepository>();
        var mockDiscursoRepo = new Mock<IDiscursoRepository>();

        var cong = new Congregacao("Central", "São Paulo", "SP", "Circuito SP-01", "Domingo 19:00");
        var orador = new Orador(cong.Id, "Orador Teste", "11988887777");
        var discurso = new Discurso(10, "Tema A");
        orador.AdicionarDiscurso(discurso.Id);

        var dataHora = DateTime.UtcNow.AddDays(7);

        mockCongRepo.Setup(r => r.ObterPorIdAsync(cong.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cong);
        mockOradorRepo.Setup(r => r.ObterPorIdAsync(orador.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orador);
        mockDiscursoRepo.Setup(r => r.ObterPorIdAsync(discurso.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(discurso);

        // Simula conflito de orador
        mockAgendaRepo.Setup(r => r.ExisteConflitoOradorAsync(orador.Id, dataHora, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new AgendaService(
            mockAgendaRepo.Object,
            mockCongRepo.Object,
            mockOradorRepo.Object,
            mockDiscursoRepo.Object,
            new CriarAgendaValidator());

        var dto = new CriarAgendaDto(cong.Id, orador.Id, discurso.Id, dataHora);

        var ex = await Assert.ThrowsAsync<DomainException>(() => service.CriarAsync(dto));
        Assert.Contains("já possui um agendamento marcado para este dia", ex.Message);
    }

    [Fact]
    public async Task ServicoAgenda_AoCriarComConflitoDeCongregacao_DeveLancarDomainException()
    {
        var mockAgendaRepo = new Mock<IAgendaRepository>();
        var mockCongRepo = new Mock<ICongregacaoRepository>();
        var mockOradorRepo = new Mock<IOradorRepository>();
        var mockDiscursoRepo = new Mock<IDiscursoRepository>();

        var cong = new Congregacao("Central", "São Paulo", "SP", "Circuito SP-01", "Domingo 19:00");
        var orador = new Orador(cong.Id, "Orador Teste", "11988887777");
        var discurso = new Discurso(10, "Tema A");
        orador.AdicionarDiscurso(discurso.Id);

        var dataHora = DateTime.UtcNow.AddDays(7);

        mockCongRepo.Setup(r => r.ObterPorIdAsync(cong.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cong);
        mockOradorRepo.Setup(r => r.ObterPorIdAsync(orador.Id, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orador);
        mockDiscursoRepo.Setup(r => r.ObterPorIdAsync(discurso.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(discurso);

        // Sem conflito de orador, mas COM conflito de congregação no mesmo dia
        mockAgendaRepo.Setup(r => r.ExisteConflitoOradorAsync(orador.Id, dataHora, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        mockAgendaRepo.Setup(r => r.ExisteConflitoCongregacaoAsync(cong.Id, dataHora, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new AgendaService(
            mockAgendaRepo.Object,
            mockCongRepo.Object,
            mockOradorRepo.Object,
            mockDiscursoRepo.Object,
            new CriarAgendaValidator());

        var dto = new CriarAgendaDto(cong.Id, orador.Id, discurso.Id, dataHora);

        var ex = await Assert.ThrowsAsync<DomainException>(() => service.CriarAsync(dto));
        Assert.Contains("já possui uma palestra/orador marcado para este dia", ex.Message);
    }

    [Fact]
    public async Task ExisteConflitoCongregacaoAsync_DeveRepassarParaRepositorio()
    {
        var mockAgendaRepo = new Mock<IAgendaRepository>();
        var congId = Guid.NewGuid();
        var data = DateTime.UtcNow.AddDays(5);

        mockAgendaRepo.Setup(r => r.ExisteConflitoCongregacaoAsync(congId, data, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new AgendaService(
            mockAgendaRepo.Object,
            new Mock<ICongregacaoRepository>().Object,
            new Mock<IOradorRepository>().Object,
            new Mock<IDiscursoRepository>().Object,
            new CriarAgendaValidator());

        var conflito = await service.ExisteConflitoCongregacaoAsync(congId, data);

        Assert.True(conflito);
        mockAgendaRepo.Verify(r => r.ExisteConflitoCongregacaoAsync(congId, data, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarDesignacoesReuniaoAsync_DeveAtualizarERepassarParaRepositorio()
    {
        var mockAgendaRepo = new Mock<IAgendaRepository>();
        var agenda = new Agenda(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(7));

        mockAgendaRepo.Setup(r => r.ObterPorIdAsync(agenda.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agenda);

        var service = new AgendaService(
            mockAgendaRepo.Object,
            new Mock<ICongregacaoRepository>().Object,
            new Mock<IOradorRepository>().Object,
            new Mock<IDiscursoRepository>().Object,
            new CriarAgendaValidator());

        var dto = new AtualizarDesignacoesReuniaoDto(agenda.Id, "Irmão Presidente", "Irmão Leitor", "Irmão Oração");
        await service.AtualizarDesignacoesReuniaoAsync(dto);

        Assert.Equal("Irmão Presidente", agenda.PresidenteReuniao);
        Assert.Equal("Irmão Leitor", agenda.LeitorSentinela);
        Assert.Equal("Irmão Oração", agenda.OracaoFinal);
        mockAgendaRepo.Verify(r => r.AtualizarAsync(agenda, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SalvarDesignacoesEmLoteAsync_DeveIterarEAtualizarCadaAgenda()
    {
        var mockAgendaRepo = new Mock<IAgendaRepository>();
        var agenda1 = new Agenda(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(7));
        var agenda2 = new Agenda(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(14));

        mockAgendaRepo.Setup(r => r.ObterPorIdAsync(agenda1.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agenda1);
        mockAgendaRepo.Setup(r => r.ObterPorIdAsync(agenda2.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agenda2);

        var service = new AgendaService(
            mockAgendaRepo.Object,
            new Mock<ICongregacaoRepository>().Object,
            new Mock<IOradorRepository>().Object,
            new Mock<IDiscursoRepository>().Object,
            new CriarAgendaValidator());

        var lista = new List<AtualizarDesignacoesReuniaoDto>
        {
            new(agenda1.Id, "Pres 1", "Leitor 1", "Oracao 1"),
            new(agenda2.Id, "Pres 2", "Leitor 2", "Oracao 2")
        };

        await service.SalvarDesignacoesEmLoteAsync(lista);

        Assert.Equal("Pres 1", agenda1.PresidenteReuniao);
        Assert.Equal("Pres 2", agenda2.PresidenteReuniao);
        mockAgendaRepo.Verify(r => r.AtualizarAsync(agenda1, It.IsAny<CancellationToken>()), Times.Once);
        mockAgendaRepo.Verify(r => r.AtualizarAsync(agenda2, It.IsAny<CancellationToken>()), Times.Once);
    }
}
