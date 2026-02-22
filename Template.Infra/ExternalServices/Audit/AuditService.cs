using Hangfire;
using Microsoft.Extensions.Logging;
using Template.Application.Common.Interfaces.Services;
using Template.Infra.BackgroundJobs;

namespace Template.Infra.ExternalServices.Audit;

/// <summary>
/// Serviço de auditoria que enfileira logs no Hangfire.
/// Não bloqueia a request - processamento assíncrono.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IBackgroundJobClient backgroundJobClient,
        ILogger<AuditService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public void EnqueueAuditLog(AuditLogEntry entry)
    {
        try
        {
            // Enfileira no Hangfire para processamento em background
            _backgroundJobClient.Enqueue<AuditLogJob>(job => job.SaveAuditLogAsync(entry));

            _logger.LogDebug("[AuditService] Audit log enfileirado: {Action} por {UserId}",
                entry.Action, entry.UserId);
        }
        catch (Exception ex)
        {
            // Log do erro mas NÃO propaga - auditoria não deve quebrar a request
            _logger.LogError(ex, "[AuditService] Erro ao enfileirar audit log: {Action} por {UserId}",
                entry.Action, entry.UserId);
        }
    }
}
