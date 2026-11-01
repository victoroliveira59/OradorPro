using AgendaPalestras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaPalestras.Infrastructure.Persistence.Configurations;

public class DiscursoConfiguration : IEntityTypeConfiguration<Discurso>
{
    public void Configure(EntityTypeBuilder<Discurso> builder)
    {
        builder.ToTable("Discursos");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Numero)
            .IsRequired();

        builder.HasIndex(d => d.Numero)
            .IsUnique();

        builder.Property(d => d.Tema)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Ativo)
            .IsRequired();

        builder.Property(d => d.CriadoEm)
            .IsRequired();
    }
}
