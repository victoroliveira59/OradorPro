using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgendaPalestras.Infrastructure.Persistence.Repositories;

public class DiscursoRepository : IDiscursoRepository
{
    private readonly ApplicationDbContext _context;

    public DiscursoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Discurso?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Discursos.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Discurso?> ObterPorNumeroAsync(int numero, CancellationToken cancellationToken = default)
    {
        return await _context.Discursos.FirstOrDefaultAsync(d => d.Numero == numero, cancellationToken);
    }

    public async Task<List<Discurso>> ListarTodosAsync(bool apenasAtivos = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Discursos.AsNoTracking();
        if (apenasAtivos)
            query = query.Where(d => d.Ativo);

        return await query.OrderBy(d => d.Numero).ToListAsync(cancellationToken);
    }

    public async Task<List<Discurso>> ListarPorIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idsSet = ids.ToHashSet();
        return await _context.Discursos
            .Where(d => idsSet.Contains(d.Id))
            .OrderBy(d => d.Numero)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Discurso discurso, CancellationToken cancellationToken = default)
    {
        await _context.Discursos.AddAsync(discurso, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Discurso discurso, CancellationToken cancellationToken = default)
    {
        _context.Discursos.Update(discurso);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Discursos.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _context.Discursos.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
