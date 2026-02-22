using Template.Domain.Entity.Tenant.Audit;
using Template.Infra.Persistence.Contexts;

namespace Template.Infra.Settings.Maps.Tenant.Audit;

internal class AuditLogMap : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable(nameof(AuditLog), Schema.Default);

        builder.HasKey(x => x.Id);

        // Usuario
        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.UserName)
            .HasMaxLength(256);

        builder.Property(x => x.UserEmail)
            .HasMaxLength(256);

        builder.Property(x => x.TenantId)
            .IsRequired();

        // Acao
        builder.Property(x => x.Action)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.HttpMethod)
            .HasMaxLength(10);

        builder.Property(x => x.Endpoint)
            .HasMaxLength(500);

        // Execucao
        builder.Property(x => x.ExecutedAt)
            .IsRequired();

        builder.Property(x => x.DurationMs)
            .IsRequired();

        builder.Property(x => x.Success)
            .IsRequired();

        builder.Property(x => x.StatusCode)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        // Request criptografado
        builder.Property(x => x.RequestBodyEncrypted)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.EncryptionKeyId)
            .HasMaxLength(100);

        // Cliente
        builder.Property(x => x.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(x => x.UserAgent)
            .HasMaxLength(512);

        // Indices para consultas frequentes
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_AuditLog_UserId");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("IX_AuditLog_TenantId");

        builder.HasIndex(x => x.ExecutedAt)
            .HasDatabaseName("IX_AuditLog_ExecutedAt");

        builder.HasIndex(x => new { x.UserId, x.ExecutedAt })
            .HasDatabaseName("IX_AuditLog_UserId_ExecutedAt");

        builder.HasIndex(x => new { x.TenantId, x.ExecutedAt })
            .HasDatabaseName("IX_AuditLog_TenantId_ExecutedAt");

        builder.HasIndex(x => x.Action)
            .HasDatabaseName("IX_AuditLog_Action");

        builder.HasIndex(x => x.Category)
            .HasDatabaseName("IX_AuditLog_Category");

        builder.HasIndex(x => x.Success)
            .HasDatabaseName("IX_AuditLog_Success");

        // Performance indexes - Missing Indexes Analysis (user_seeks based)
        // IX_AuditLog_Active - 87 seeks
        builder.HasIndex(x => x.Active)
            .HasDatabaseName("IX_AuditLog_Active");

        // IX_AuditLog_Active_ExecutedAt - 275 seeks (somando todas as variações)
        // INCLUDE: colunas frequentemente retornadas no SELECT para evitar Key Lookup
        builder.HasIndex(x => new { x.Active, x.ExecutedAt })
            .IncludeProperties(x => new { x.UserId, x.UserName, x.DurationMs, x.Success, x.Action, x.Category })
            .HasDatabaseName("IX_AuditLog_Active_ExecutedAt");
    }
}
