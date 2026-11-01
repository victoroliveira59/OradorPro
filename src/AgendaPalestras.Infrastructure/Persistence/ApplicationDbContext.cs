using AgendaPalestras.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgendaPalestras.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Congregacao> Congregacoes => Set<Congregacao>();
    public DbSet<Orador> Oradores => Set<Orador>();
    public DbSet<Discurso> Discursos => Set<Discurso>();
    public DbSet<OradorDiscurso> OradoresDiscursos => Set<OradorDiscurso>();
    public DbSet<Agenda> Agendas => Set<Agenda>();
    public DbSet<MensagemHistorico> MensagensHistorico => Set<MensagemHistorico>();
    public DbSet<ConfiguracaoSistema> ConfiguracoesSistema => Set<ConfiguracaoSistema>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
