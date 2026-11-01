using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Enums;
using AgendaPalestras.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgendaPalestras.Infrastructure.Persistence.Repositories;

public class AgendaRepository : IAgendaRepository
{
    private readonly ApplicationDbContext _context;

    public AgendaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Agenda?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Agendas
            .Include(a => a.Congregacao)
            .Include(a => a.Orador)
            .Include(a => a.Discurso)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    private static DateTime ToUtc(DateTime dt)
    {
        return dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
        };
    }

    public async Task<IReadOnlyList<Agenda>> ObterPorPeriodoAsync(
        DateTime inicio,
        DateTime fim,
        Guid? congregacaoId = null,
        CancellationToken cancellationToken = default)
    {
        var inicioUtc = ToUtc(inicio);
        var fimUtc = ToUtc(fim);

        var query = _context.Agendas
            .Include(a => a.Congregacao)
            .Include(a => a.Orador)
            .Include(a => a.Discurso)
            .Where(a => a.DataHora >= inicioUtc && a.DataHora <= fimUtc);

        if (congregacaoId.HasValue)
        {
            query = query.Where(a => a.CongregacaoId == congregacaoId.Value);
        }

        return await query
            .OrderBy(a => a.DataHora)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteConflitoOradorAsync(
        Guid oradorId,
        DateTime dataHora,
        Guid? ignorarAgendaId = null,
        CancellationToken cancellationToken = default)
    {
        var dataHoraUtc = ToUtc(dataHora);

        var query = _context.Agendas
            .Where(a => a.OradorId == oradorId &&
                        a.DataHora == dataHoraUtc &&
                        a.Status != StatusPalestra.Cancelado);

        if (ignorarAgendaId.HasValue)
        {
            query = query.Where(a => a.Id != ignorarAgendaId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExisteConflitoCongregacaoAsync(
        Guid congregacaoId,
        DateTime dataHora,
        Guid? ignorarAgendaId = null,
        CancellationToken cancellationToken = default)
    {
        var dataHoraUtc = ToUtc(dataHora);

        var query = _context.Agendas
            .Where(a => a.CongregacaoId == congregacaoId &&
                        a.DataHora == dataHoraUtc &&
                        a.Status != StatusPalestra.Cancelado);

        if (ignorarAgendaId.HasValue)
        {
            query = query.Where(a => a.Id != ignorarAgendaId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Agenda>> ObterPalestrasParaLembreteAsync(
        DateTime dataAlvoInicio,
        DateTime dataAlvoFim,
        CancellationToken cancellationToken = default)
    {
        var inicioUtc = ToUtc(dataAlvoInicio);
        var fimUtc = ToUtc(dataAlvoFim);

        return await _context.Agendas
            .Include(a => a.Congregacao)
            .Include(a => a.Orador)
            .Include(a => a.Discurso)
            .Where(a => a.Status == StatusPalestra.Confirmado &&
                        a.DataHora >= inicioUtc &&
                        a.DataHora <= fimUtc &&
                        a.DataLembreteEnviado == null)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Agenda agenda, CancellationToken cancellationToken = default)
    {
        await _context.Agendas.AddAsync(agenda, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Agenda agenda, CancellationToken cancellationToken = default)
    {
        _context.Agendas.Update(agenda);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
