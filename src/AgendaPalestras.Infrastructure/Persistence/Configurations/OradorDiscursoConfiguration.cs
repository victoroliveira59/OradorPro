using AgendaPalestras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaPalestras.Infrastructure.Persistence.Configurations;

public class OradorDiscursoConfiguration : IEntityTypeConfiguration<OradorDiscurso>
{
    public void Configure(EntityTypeBuilder<OradorDiscurso> builder)
    {
        builder.ToTable("OradoresDiscursos");

        builder.HasKey(od => od.Id);

        builder.HasIndex(od => new { od.OradorId, od.DiscursoId })
            .IsUnique();

        builder.HasOne(od => od.Orador)
            .WithMany(o => o.OradorDiscursos)
            .HasForeignKey(od => od.OradorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(od => od.Discurso)
            .WithMany(d => d.OradorDiscursos)
            .HasForeignKey(od => od.DiscursoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(od => od.CriadoEm)
            .IsRequired();
    }
}
