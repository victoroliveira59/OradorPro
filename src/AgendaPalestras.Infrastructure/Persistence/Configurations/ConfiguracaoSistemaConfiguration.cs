using AgendaPalestras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaPalestras.Infrastructure.Persistence.Configurations;

public class ConfiguracaoSistemaConfiguration : IEntityTypeConfiguration<ConfiguracaoSistema>
{
    public void Configure(EntityTypeBuilder<ConfiguracaoSistema> builder)
    {
        builder.ToTable("ConfiguracoesSistema");

        builder.HasKey(c => c.Chave);

        builder.Property(c => c.Chave)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Valor)
            .IsRequired();

        builder.Property(c => c.AtualizadoEm)
            .IsRequired();
    }
}
