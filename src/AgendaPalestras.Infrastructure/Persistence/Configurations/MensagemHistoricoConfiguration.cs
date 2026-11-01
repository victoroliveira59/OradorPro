using AgendaPalestras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaPalestras.Infrastructure.Persistence.Configurations;

public class MensagemHistoricoConfiguration : IEntityTypeConfiguration<MensagemHistorico>
{
    public void Configure(EntityTypeBuilder<MensagemHistorico> builder)
    {
        builder.ToTable("MensagensHistorico");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Telefone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(m => m.Conteudo)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(m => m.IdentificadorProvedor)
            .HasMaxLength(150);

        builder.Property(m => m.ErroDetalhe)
            .HasMaxLength(1000);

        builder.HasOne(m => m.Agenda)
            .WithMany()
            .HasForeignKey(m => m.AgendaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(m => m.IdentificadorProvedor)
            .HasDatabaseName("IX_MensagensHistorico_IdentificadorProvedor");

        builder.HasIndex(m => m.AgendaId)
            .HasDatabaseName("IX_MensagensHistorico_AgendaId");
    }
}
