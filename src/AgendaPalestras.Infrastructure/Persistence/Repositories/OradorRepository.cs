using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgendaPalestras.Infrastructure.Persistence.Repositories;

public class OradorRepository : IOradorRepository
{
    private readonly ApplicationDbContext _context;

    public OradorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Orador?> ObterPorIdAsync(Guid id, bool incluirRepertorio = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Oradores
            .Include(o => o.Congregacao)
            .AsQueryable();

        if (incluirRepertorio)
        {
            query = query
                .Include(o => o.OradorDiscursos)
                .ThenInclude(od => od.Discurso);
        }

        return await query.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<List<Orador>> ListarTodosAsync(Guid? congregacaoId = null, bool apenasAtivos = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Oradores
            .Include(o => o.Congregacao)
            .Include(o => o.OradorDiscursos)
            .ThenInclude(od => od.Discurso)
            .AsNoTracking();

        if (congregacaoId.HasValue)
            query = query.Where(o => o.CongregacaoId == congregacaoId.Value);

        if (apenasAtivos)
            query = query.Where(o => o.Ativo);

        return await query.OrderBy(o => o.Nome).ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Orador orador, CancellationToken cancellationToken = default)
    {
        await _context.Oradores.AddAsync(orador, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Orador orador, CancellationToken cancellationToken = default)
    {
        var oradorEntry = _context.Entry(orador);

        // Apenas sincroniza repertório se a coleção foi explicitamente carregada ou possui itens
        if (oradorEntry.Collection(o => o.OradorDiscursos).IsLoaded || orador.OradorDiscursos.Count > 0)
        {
            var dbDiscursos = await _context.OradoresDiscursos
                .Where(od => od.OradorId == orador.Id)
                .ToListAsync(cancellationToken);

            var currentDiscursoIds = orador.OradorDiscursos.Select(od => od.DiscursoId).ToHashSet();

            // 1. Remove do banco os discursos desselecionados
            var paraRemover = dbDiscursos.Where(d => !currentDiscursoIds.Contains(d.DiscursoId)).ToList();
            if (paraRemover.Count > 0)
            {
                _context.OradoresDiscursos.RemoveRange(paraRemover);
            }

            // 2. Adiciona os novos discursos garantindo estado Added
            var dbDiscursoIds = dbDiscursos.Select(d => d.DiscursoId).ToHashSet();
            foreach (var od in orador.OradorDiscursos)
            {
                if (!dbDiscursoIds.Contains(od.DiscursoId))
                {
                    var odEntry = _context.Entry(od);
                    if (odEntry.State == EntityState.Detached)
                    {
                        await _context.OradoresDiscursos.AddAsync(od, cancellationToken);
                    }
                    else
                    {
                        odEntry.State = EntityState.Added;
                    }
                }
            }
        }

        if (oradorEntry.State == EntityState.Detached)
        {
            _context.Oradores.Attach(orador);
            oradorEntry.State = EntityState.Modified;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Oradores.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _context.Oradores.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExisteComTelefoneAsync(string telefone, Guid? ignorarId = null, CancellationToken cancellationToken = default)
    {
        var digitos = new string(telefone.Where(char.IsDigit).ToArray());
        var query = _context.Oradores.Where(o => o.TelefoneWhatsapp == digitos);
        if (ignorarId.HasValue)
            query = query.Where(o => o.Id != ignorarId.Value);

        return await query.AnyAsync(cancellationToken);
    }
}
