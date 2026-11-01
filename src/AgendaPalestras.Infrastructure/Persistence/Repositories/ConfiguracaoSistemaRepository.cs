using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgendaPalestras.Infrastructure.Persistence.Repositories;

public class ConfiguracaoSistemaRepository : IConfiguracaoSistemaRepository
{
    private readonly ApplicationDbContext _context;

    public ConfiguracaoSistemaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string?> ObterValorAsync(string chave, CancellationToken cancellationToken = default)
    {
        var chaveLower = chave.Trim().ToLowerInvariant();
        var item = await _context.ConfiguracoesSistema
            .FirstOrDefaultAsync(c => c.Chave.ToLower() == chaveLower, cancellationToken);

        return item?.Valor;
    }

    public async Task SalvarValorAsync(string chave, string valor, CancellationToken cancellationToken = default)
    {
        var chaveNormalizada = chave.Trim();
        var chaveLower = chaveNormalizada.ToLowerInvariant();
        var item = await _context.ConfiguracoesSistema
            .FirstOrDefaultAsync(c => c.Chave.ToLower() == chaveLower, cancellationToken);

        if (item == null)
        {
            var novo = new ConfiguracaoSistema(chaveNormalizada, valor);
            await _context.ConfiguracoesSistema.AddAsync(novo, cancellationToken);
        }
        else
        {
            item.Atualizar(valor);
            _context.ConfiguracoesSistema.Update(item);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Dictionary<string, string>> ObterTodasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ConfiguracoesSistema
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Chave, c => c.Valor, cancellationToken);
    }
}
