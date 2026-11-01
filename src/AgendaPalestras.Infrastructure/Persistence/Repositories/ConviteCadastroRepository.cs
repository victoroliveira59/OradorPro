using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgendaPalestras.Infrastructure.Persistence.Repositories;

public class ConviteCadastroRepository : IConviteCadastroRepository
{
    private readonly ApplicationDbContext _context;

    public ConviteCadastroRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ConviteCadastro?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ConvitesCadastro
            .Include(c => c.CriadoPorUsuario)
            .Include(c => c.UsuarioCriado)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<ConviteCadastro?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return await _context.ConvitesCadastro
            .Include(c => c.CriadoPorUsuario)
            .Include(c => c.UsuarioCriado)
            .FirstOrDefaultAsync(c => c.Codigo == codigo, cancellationToken);
    }

    public async Task<List<ConviteCadastro>> ListarTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ConvitesCadastro
            .Include(c => c.CriadoPorUsuario)
            .Include(c => c.UsuarioCriado)
            .OrderByDescending(c => c.CriadoEm)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(ConviteCadastro convite, CancellationToken cancellationToken = default)
    {
        await _context.ConvitesCadastro.AddAsync(convite, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(ConviteCadastro convite, CancellationToken cancellationToken = default)
    {
        _context.ConvitesCadastro.Update(convite);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
