using AgendaPalestras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaPalestras.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.HashSenha)
            .IsRequired();

        builder.Property(u => u.Papel)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Ativo)
            .IsRequired();

        builder.Property(u => u.CriadoEm)
            .IsRequired();

        builder.Property(u => u.EvolutionInstanceName)
            .HasMaxLength(100);

        builder.Property(u => u.EvolutionApiUrl)
            .HasMaxLength(250);

        builder.Property(u => u.EvolutionApiKey)
            .HasMaxLength(250);

        builder.HasOne(u => u.Congregacao)
            .WithMany()
            .HasForeignKey(u => u.CongregacaoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
