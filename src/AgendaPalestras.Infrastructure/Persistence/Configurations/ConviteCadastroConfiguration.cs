using AgendaPalestras.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaPalestras.Infrastructure.Persistence.Configurations;

public class ConviteCadastroConfiguration : IEntityTypeConfiguration<ConviteCadastro>
{
    public void Configure(EntityTypeBuilder<ConviteCadastro> builder)
    {
        builder.ToTable("ConvitesCadastro");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Codigo)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(c => c.Codigo)
            .IsUnique();

        builder.Property(c => c.EmailConvidado)
            .HasMaxLength(150);

        builder.Property(c => c.DataExpiracao)
            .IsRequired();

        builder.Property(c => c.Usado)
            .IsRequired();

        builder.Property(c => c.CriadoEm)
            .IsRequired();

        builder.HasOne(c => c.CriadoPorUsuario)
            .WithMany()
            .HasForeignKey(c => c.CriadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.UsuarioCriado)
            .WithMany()
            .HasForeignKey(c => c.UsuarioCriadoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
