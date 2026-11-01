using AgendaPalestras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaPalestras.Infrastructure.Persistence.Configurations;

public class OradorConfiguration : IEntityTypeConfiguration<Orador>
{
    public void Configure(EntityTypeBuilder<Orador> builder)
    {
        builder.ToTable("Oradores");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(o => o.TelefoneWhatsapp)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(o => o.Email)
            .HasMaxLength(150);

        builder.Property(o => o.Ativo)
            .IsRequired();

        builder.Property(o => o.CriadoEm)
            .IsRequired();

        builder.HasOne(o => o.Congregacao)
            .WithMany(c => c.Oradores)
            .HasForeignKey(o => o.CongregacaoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.OradorDiscursos)
            .WithOne(od => od.Orador)
            .HasForeignKey(od => od.OradorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
