using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Exceptions;
using AgendaPalestras.Domain.Interfaces;
using FluentValidation;

namespace AgendaPalestras.Application.Services;

public class AgendaService : IAgendaService
{
    private readonly IAgendaRepository _agendaRepository;
    private readonly ICongregacaoRepository _congregacaoRepository;
    private readonly IOradorRepository _oradorRepository;
    private readonly IDiscursoRepository _discursoRepository;
    private readonly IValidator<CriarAgendaDto> _criarValidator;

    public AgendaService(
        IAgendaRepository agendaRepository,
        ICongregacaoRepository congregacaoRepository,
        IOradorRepository oradorRepository,
        IDiscursoRepository discursoRepository,
        IValidator<CriarAgendaDto> criarValidator)
    {
        _agendaRepository = agendaRepository;
        _congregacaoRepository = congregacaoRepository;
        _oradorRepository = oradorRepository;
        _discursoRepository = discursoRepository;
        _criarValidator = criarValidator;
    }

    public async Task<AgendaDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var agenda = await _agendaRepository.ObterPorIdAsync(id, cancellationToken);
        return agenda == null ? null : AgendaDto.FromDomain(agenda);
    }

    public async Task<IReadOnlyList<AgendaDto>> ObterPorPeriodoAsync(
        DateTime inicio,
        DateTime fim,
        Guid? congregacaoId = null,
        CancellationToken cancellationToken = default)
    {
        var agendas = await _agendaRepository.ObterPorPeriodoAsync(inicio, fim, congregacaoId, cancellationToken);
        return agendas.Select(AgendaDto.FromDomain).ToList();
    }

    public async Task<AgendaDto> CriarAsync(CriarAgendaDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _criarValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var congregacao = await _congregacaoRepository.ObterPorIdAsync(dto.CongregacaoId, cancellationToken)
            ?? throw new DomainException("Congregação não encontrada.");

        var orador = await _oradorRepository.ObterPorIdAsync(dto.OradorId, incluirRepertorio: true, cancellationToken)
            ?? throw new DomainException("Orador não encontrado.");

        if (!orador.Ativo)
            throw new DomainException("O orador selecionado está inativo.");

        var discurso = await _discursoRepository.ObterPorIdAsync(dto.DiscursoId, cancellationToken)
            ?? throw new DomainException("Tema de discurso não encontrado.");

        if (!discurso.Ativo)
            throw new DomainException("O tema de discurso selecionado está inativo.");

        // Validar repertório do orador (N:N)
        var temDiscursoNoRepertorio = orador.OradorDiscursos.Any(od => od.DiscursoId == dto.DiscursoId);
        if (!temDiscursoNoRepertorio)
            throw new DomainException($"O discurso '{discurso.Tema}' não faz parte do repertório cadastrado para o orador '{orador.Nome}'.");

        // Validar conflitos de unicidade por dia (ADR-0006 e ADR-0010)
        var conflitoOrador = await _agendaRepository.ExisteConflitoOradorAsync(dto.OradorId, dto.DataHora, null, cancellationToken);
        if (conflitoOrador)
            throw new DomainException($"O orador '{orador.Nome}' já possui um agendamento marcado para este dia.");

        var conflitoCongregacao = await _agendaRepository.ExisteConflitoCongregacaoAsync(dto.CongregacaoId, dto.DataHora, null, cancellationToken);
        if (conflitoCongregacao)
            throw new DomainException($"A congregação '{congregacao.Nome}' já possui uma palestra/orador marcado para este dia.");

        var agenda = new Agenda(
            dto.CongregacaoId,
            dto.OradorId,
            dto.DiscursoId,
            dto.DataHora,
            dto.Observacoes ?? string.Empty
        );

        await _agendaRepository.AdicionarAsync(agenda, cancellationToken);

        var criado = await _agendaRepository.ObterPorIdAsync(agenda.Id, cancellationToken);
        return AgendaDto.FromDomain(criado ?? agenda);
    }

    public async Task<AgendaDto> AtualizarAsync(AtualizarAgendaDto dto, CancellationToken cancellationToken = default)
    {
        var agenda = await _agendaRepository.ObterPorIdAsync(dto.Id, cancellationToken)
            ?? throw new DomainException("Agendamento não encontrado.");

        var orador = await _oradorRepository.ObterPorIdAsync(agenda.OradorId, incluirRepertorio: true, cancellationToken)
            ?? throw new DomainException("Orador não encontrado.");

        var discurso = await _discursoRepository.ObterPorIdAsync(dto.DiscursoId, cancellationToken)
            ?? throw new DomainException("Tema de discurso não encontrado.");

        if (!discurso.Ativo)
            throw new DomainException("O tema de discurso selecionado está inativo.");

        var temDiscursoNoRepertorio = orador.OradorDiscursos.Any(od => od.DiscursoId == dto.DiscursoId);
        if (!temDiscursoNoRepertorio)
            throw new DomainException($"O discurso '{discurso.Tema}' não faz parte do repertório cadastrado para o orador '{orador.Nome}'.");

        // Validar conflito de orador no mesmo dia
        var conflitoOrador = await _agendaRepository.ExisteConflitoOradorAsync(agenda.OradorId, dto.DataHora, agenda.Id, cancellationToken);
        if (conflitoOrador)
            throw new DomainException($"O orador '{orador.Nome}' já possui outro agendamento marcado para este dia.");

        // Validar conflito de congregação no mesmo dia
        var conflitoCongregacao = await _agendaRepository.ExisteConflitoCongregacaoAsync(agenda.CongregacaoId, dto.DataHora, agenda.Id, cancellationToken);
        if (conflitoCongregacao)
            throw new DomainException("A congregação já possui outro orador/discurso agendado para este dia.");

        agenda.Reagendar(dto.DataHora, dto.DiscursoId, dto.Observacoes);
        await _agendaRepository.AtualizarAsync(agenda, cancellationToken);

        var atualizado = await _agendaRepository.ObterPorIdAsync(agenda.Id, cancellationToken);
        return AgendaDto.FromDomain(atualizado ?? agenda);
    }

    public async Task ConfirmarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var agenda = await _agendaRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new DomainException("Agendamento não encontrado.");

        agenda.Confirmar();
        await _agendaRepository.AtualizarAsync(agenda, cancellationToken);
    }

    public async Task RecusarAsync(Guid id, string? motivo = null, CancellationToken cancellationToken = default)
    {
        var agenda = await _agendaRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new DomainException("Agendamento não encontrado.");

        agenda.Recusar(motivo);
        await _agendaRepository.AtualizarAsync(agenda, cancellationToken);
    }

    public async Task CancelarAsync(CancelarAgendaDto dto, CancellationToken cancellationToken = default)
    {
        var agenda = await _agendaRepository.ObterPorIdAsync(dto.Id, cancellationToken)
            ?? throw new DomainException("Agendamento não encontrado.");

        agenda.Cancelar(dto.Motivo);
        await _agendaRepository.AtualizarAsync(agenda, cancellationToken);
    }

    public async Task<bool> ExisteConflitoCongregacaoAsync(
        Guid congregacaoId,
        DateTime dataHora,
        Guid? ignorarAgendaId = null,
        CancellationToken cancellationToken = default)
    {
        return await _agendaRepository.ExisteConflitoCongregacaoAsync(congregacaoId, dataHora, ignorarAgendaId, cancellationToken);
    }

    public async Task AtualizarDesignacoesReuniaoAsync(AtualizarDesignacoesReuniaoDto dto, CancellationToken cancellationToken = default)
    {
        var agenda = await _agendaRepository.ObterPorIdAsync(dto.AgendaId, cancellationToken)
            ?? throw new DomainException("Agendamento não encontrado.");

        agenda.AtualizarDesignacoesReuniao(dto.PresidenteReuniao, dto.LeitorSentinela, dto.OracaoFinal);
        await _agendaRepository.AtualizarAsync(agenda, cancellationToken);
    }

    public async Task SalvarDesignacoesEmLoteAsync(IEnumerable<AtualizarDesignacoesReuniaoDto> lista, CancellationToken cancellationToken = default)
    {
        foreach (var item in lista)
        {
            var agenda = await _agendaRepository.ObterPorIdAsync(item.AgendaId, cancellationToken);
            if (agenda != null)
            {
                agenda.AtualizarDesignacoesReuniao(item.PresidenteReuniao, item.LeitorSentinela, item.OracaoFinal);
                await _agendaRepository.AtualizarAsync(agenda, cancellationToken);
            }
        }
    }
}
