using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgendaPalestras.Infrastructure.Persistence.Repositories;

public class CongregacaoRepository : ICongregacaoRepository
{
    private readonly ApplicationDbContext _context;

    public CongregacaoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Congregacao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Congregacoes
            .Include(c => c.Oradores)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Congregacao>> ListarTodasAsync(bool apenasAtivas = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Congregacoes
            .Include(c => c.Oradores)
            .AsNoTracking();

        if (apenasAtivas)
            query = query.Where(c => c.Ativo);

        return await query.OrderBy(c => c.Nome).ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Congregacao congregacao, CancellationToken cancellationToken = default)
    {
        await _context.Congregacoes.AddAsync(congregacao, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Congregacao congregacao, CancellationToken cancellationToken = default)
    {
        _context.Congregacoes.Update(congregacao);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Congregacoes.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _context.Congregacoes.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExisteComNomeAsync(string nome, Guid? ignorarId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Congregacoes.Where(c => c.Nome.ToLower() == nome.Trim().ToLower());
        if (ignorarId.HasValue)
            query = query.Where(c => c.Id != ignorarId.Value);

        return await query.AnyAsync(cancellationToken);
    }
}
