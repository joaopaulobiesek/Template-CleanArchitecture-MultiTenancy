using Template.Domain.Entity.Core;
using Template.Infra.Persistence.Contexts;

namespace Template.Infra.Settings.Maps.Core;

internal class LgpdTermMap : IEntityTypeConfiguration<LgpdTerm>
{
    public void Configure(EntityTypeBuilder<LgpdTerm> builder)
    {
        builder.ToTable(nameof(LgpdTerm), Schema.Default);

        builder.HasKey(t => t.Id);

        // Versionamento
        builder.Property(t => t.Version)
               .IsRequired(true)
               .HasMaxLength(20); // "1.0", "2.0.1", etc

        // Conteúdo - Termo de Uso
        builder.Property(t => t.TermsOfUseContent)
               .IsRequired(true)
               .HasMaxLength(50000); // Permite até 50k caracteres

        builder.Property(t => t.TermsOfUseHash)
               .IsRequired(true)
               .HasMaxLength(100); // SHA256 em Base64

        // Conteúdo - Política de Privacidade
        builder.Property(t => t.PrivacyPolicyContent)
               .IsRequired(true)
               .HasMaxLength(50000); // Permite até 50k caracteres

        builder.Property(t => t.PrivacyPolicyHash)
               .IsRequired(true)
               .HasMaxLength(100); // SHA256 em Base64

        // Metadata
        builder.Property(t => t.PublishedAt)
               .IsRequired(true);

        builder.Property(t => t.IsActive)
               .IsRequired(true)
               .HasDefaultValue(false);

        builder.Property(t => t.InactivatedAt)
               .IsRequired(false);

        builder.Property(t => t.PublishedByUserId)
               .IsRequired(false);

        // Índices
        builder.HasIndex(t => t.Version)
               .IsUnique();

        builder.HasIndex(t => t.IsActive);
        builder.HasIndex(t => t.PublishedAt);
        builder.HasIndex(t => t.TermsOfUseHash);
        builder.HasIndex(t => t.PrivacyPolicyHash);
    }
}
