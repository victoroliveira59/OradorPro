using AgendaPalestras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaPalestras.Infrastructure.Persistence.Configurations;

public class CongregacaoConfiguration : IEntityTypeConfiguration<Congregacao>
{
    public void Configure(EntityTypeBuilder<Congregacao> builder)
    {
        builder.ToTable("Congregacoes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.UsuarioCriadorId)
            .IsRequired(false);

        builder.HasIndex(c => c.UsuarioCriadorId);

        builder.HasIndex(c => c.Nome);

        builder.Property(c => c.Cidade)
            .HasMaxLength(100);

        builder.Property(c => c.Circuito)
            .HasMaxLength(50);

        builder.Property(c => c.DiasReuniao)
            .HasMaxLength(100);

        builder.Property(c => c.HorarioReuniao)
            .HasMaxLength(50);

        builder.Property(c => c.Endereco)
            .HasMaxLength(250);

        builder.Property(c => c.LinkGoogleMaps)
            .HasMaxLength(500);

        builder.Property(c => c.Ativo)
            .IsRequired();

        builder.Property(c => c.CriadoEm)
            .IsRequired();

        builder.HasMany(c => c.Oradores)
            .WithOne(o => o.Congregacao)
            .HasForeignKey(o => o.CongregacaoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
