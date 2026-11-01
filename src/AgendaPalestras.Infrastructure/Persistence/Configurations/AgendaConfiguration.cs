using AgendaPalestras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaPalestras.Infrastructure.Persistence.Configurations;

public class AgendaConfiguration : IEntityTypeConfiguration<Agenda>
{
    public void Configure(EntityTypeBuilder<Agenda> builder)
    {
        builder.ToTable("Agendas");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Observacoes)
            .HasMaxLength(1000);

        builder.Property(a => a.MotivoCancelamentoOuRecusa)
            .HasMaxLength(500);

        builder.Property(a => a.Status)
            .IsRequired();

        builder.HasOne(a => a.Congregacao)
            .WithMany()
            .HasForeignKey(a => a.CongregacaoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Orador)
            .WithMany()
            .HasForeignKey(a => a.OradorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Discurso)
            .WithMany()
            .HasForeignKey(a => a.DiscursoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.OradorId, a.DataHora })
            .HasDatabaseName("IX_Agendas_Orador_DataHora");

        builder.HasIndex(a => new { a.CongregacaoId, a.DataHora })
            .HasDatabaseName("IX_Agendas_Congregacao_DataHora");
    }
}
