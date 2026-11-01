using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgendaPalestras.Infrastructure.Persistence.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ApplicationDbContext _context;

    public UsuarioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .Include(u => u.Congregacao)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailLower = email.Trim().ToLowerInvariant();
        return await _context.Usuarios
            .Include(u => u.Congregacao)
            .FirstOrDefaultAsync(u => u.Email == emailLower, cancellationToken);
    }

    public async Task<bool> ExisteAdminAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios.AnyAsync(u => (u.Papel == "Admin" || u.Papel == "SuperAdmin") && u.Ativo, cancellationToken);
    }

    public async Task<IReadOnlyList<Usuario>> ListarTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Usuarios
            .Include(u => u.Congregacao)
            .OrderBy(u => u.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        await _context.Usuarios.AddAsync(usuario, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
