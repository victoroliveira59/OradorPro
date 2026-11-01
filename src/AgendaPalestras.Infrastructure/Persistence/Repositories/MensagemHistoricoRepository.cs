using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgendaPalestras.Infrastructure.Persistence.Repositories;

public class MensagemHistoricoRepository : IMensagemHistoricoRepository
{
    private readonly ApplicationDbContext _context;

    public MensagemHistoricoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MensagemHistorico?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MensagensHistorico
            .Include(m => m.Agenda)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<MensagemHistorico>> ObterPorAgendaIdAsync(Guid agendaId, CancellationToken cancellationToken = default)
    {
        return await _context.MensagensHistorico
            .Where(m => m.AgendaId == agendaId)
            .OrderByDescending(m => m.DataRegistro)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> JaExisteIdentificadorProvedorAsync(string identificadorProvedor, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identificadorProvedor))
            return false;

        return await _context.MensagensHistorico
            .AnyAsync(m => m.IdentificadorProvedor == identificadorProvedor, cancellationToken);
    }

    public async Task<MensagemHistorico?> ObterPorIdentificadorProvedorAsync(string identificadorProvedor, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identificadorProvedor))
            return null;

        return await _context.MensagensHistorico
            .Include(m => m.Agenda)
            .FirstOrDefaultAsync(m => m.IdentificadorProvedor == identificadorProvedor, cancellationToken);
    }

    public async Task AdicionarAsync(MensagemHistorico mensagem, CancellationToken cancellationToken = default)
    {
        await _context.MensagensHistorico.AddAsync(mensagem, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(MensagemHistorico mensagem, CancellationToken cancellationToken = default)
    {
        _context.MensagensHistorico.Update(mensagem);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
