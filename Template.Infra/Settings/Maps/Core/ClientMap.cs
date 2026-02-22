using Template.Domain.Entity.Core;
using Template.Infra.Persistence.Contexts;

namespace Template.Infra.Settings.Maps.Core;

internal class ClientMap : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable(nameof(Client), Schema.Default);

        builder.HasKey(u => u.Id);
        builder.Property(u => u.FullName).IsRequired(true).HasMaxLength(100);
        builder.Property(u => u.DocumentNumber).IsRequired(true).HasMaxLength(20);
        builder.Property(u => u.Phone).IsRequired(false).HasMaxLength(20);
        builder.Property(u => u.Email).IsRequired(false).HasMaxLength(254);
        builder.Property(u => u.ConnectionString).IsRequired(false).HasMaxLength(5000);
        builder.Property(u => u.StorageConfiguration).IsRequired(false).HasMaxLength(5000);
        builder.Property(u => u.Url).IsRequired(false).HasMaxLength(1024);
        builder.Property(u => u.TimeZoneId).IsRequired(false).HasMaxLength(100);
        builder.Property(u => u.Paid).IsRequired(true);
        builder.Property(u => u.UserId).IsRequired(false).HasMaxLength(450);
        builder.Property(u => u.AllowedIpsJson).IsRequired(false).HasMaxLength(5000);

        // Index para busca por UserId
        builder.HasIndex(u => u.UserId).HasDatabaseName("IX_Client_UserId");
    }
}