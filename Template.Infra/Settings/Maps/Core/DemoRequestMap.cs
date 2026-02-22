using Template.Domain.Entity.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Template.Infra.Settings.Maps.Core;

public class DemoRequestMap : IEntityTypeConfiguration<DemoRequest>
{
    public void Configure(EntityTypeBuilder<DemoRequest> builder)
    {
        builder.ToTable("DemoRequests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.CompanyName)
            .HasMaxLength(200);

        builder.Property(x => x.EventType)
            .HasMaxLength(100);

        builder.Property(x => x.EstimatedAudience)
            .HasMaxLength(100);

        builder.Property(x => x.Message)
            .HasMaxLength(2000);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue(DemoRequestStatus.Pending);

        builder.Property(x => x.AdminNotes)
            .HasMaxLength(2000);

        builder.Property(x => x.ContactedAt);

        // Índices
        builder.HasIndex(x => x.Email)
            .HasDatabaseName("IX_DemoRequests_Email");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_DemoRequests_Status");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("IX_DemoRequests_CreatedAt");

        builder.HasIndex(x => x.Active)
            .HasDatabaseName("IX_DemoRequests_Active");
    }
}
