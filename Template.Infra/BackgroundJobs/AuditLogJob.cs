using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Template.Application.Common.Interfaces.Services;
using Template.Application.Common.Persistence;
using Template.Domain.Entity.Tenant.Audit;

namespace Template.Infra.BackgroundJobs;

/// <summary>
/// Job Hangfire para salvar logs de auditoria no banco de dados.
/// Executado em background para nao impactar performance das requests.
/// </summary>
public class AuditLogJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditLogJob> _logger;

    public AuditLogJob(
        IServiceScopeFactory scopeFactory,
        ILogger<AuditLogJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Salva um registro de auditoria no banco do tenant.
    /// A connection string vem no DTO pois o job roda em background sem HttpContext.
    /// </summary>
    public async Task SaveAuditLogAsync(AuditLogEntry entry)
    {
        try
        {
            // Valida se temos connection string
            if (string.IsNullOrEmpty(entry.ConnectionString))
            {
                _logger.LogWarning("[AuditLogJob] Connection string nao informada para tenant {TenantId}. Audit log descartado.", entry.TenantId);
                return;
            }

            using var scope = _scopeFactory.CreateScope();

            var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();

            // Configura a connection string do tenant (job roda sem HttpContext)
            tenantContext.SetConnectionString(entry.ConnectionString);

            // Cria a entidade
            var auditLog = new AuditLog(
                userId: entry.UserId,
                userName: entry.UserName,
                userEmail: entry.UserEmail,
                tenantId: entry.TenantId,
                action: entry.Action,
                category: entry.Category,
                httpMethod: entry.HttpMethod,
                endpoint: entry.Endpoint,
                executedAt: entry.ExecutedAt,
                durationMs: entry.DurationMs,
                success: entry.Success,
                statusCode: entry.StatusCode,
                errorMessage: entry.ErrorMessage,
                requestBodyEncrypted: entry.RequestBodyEncrypted,
                encryptionKeyId: entry.EncryptionKeyId,
                ipAddress: entry.IpAddress,
                userAgent: entry.UserAgent
            );

            // Salva no banco
            await tenantContext.Set<AuditLog>().AddAsync(auditLog);
            await tenantContext.SaveChangesAsync(CancellationToken.None);

            _logger.LogDebug("[AuditLogJob] Audit log salvo: {Action} por {UserId} no tenant {TenantId}",
                entry.Action, entry.UserId, entry.TenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AuditLogJob] Erro ao salvar audit log: {Action} por {UserId} no tenant {TenantId}",
                entry.Action, entry.UserId, entry.TenantId);
            throw; // Hangfire vai registrar a falha e pode retentar
        }
    }
}
