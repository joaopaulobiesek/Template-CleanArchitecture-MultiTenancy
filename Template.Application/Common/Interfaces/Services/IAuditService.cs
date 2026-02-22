namespace Template.Application.Common.Interfaces.Services;

/// <summary>
/// DTO para enfileirar auditoria no Hangfire.
/// Contém todos os dados necessários para salvar o log de auditoria,
/// incluindo a ConnectionString do tenant para uso no job em background.
/// </summary>
public class AuditLogEntry
{
    public string UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public Guid TenantId { get; set; }
    public string Action { get; set; }
    public string Category { get; set; }
    public string? HttpMethod { get; set; }
    public string? Endpoint { get; set; }
    public DateTime ExecutedAt { get; set; }
    public long DurationMs { get; set; }
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? RequestBodyEncrypted { get; set; }
    public string? EncryptionKeyId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    /// <summary>
    /// Connection string do tenant para uso no job Hangfire.
    /// Necessário pois o job roda em background sem HttpContext.
    /// </summary>
    public string? ConnectionString { get; set; }

    public AuditLogEntry(
        string userId,
        string? userName,
        string? userEmail,
        Guid tenantId,
        string action,
        string category,
        string? httpMethod,
        string? endpoint,
        DateTime executedAt,
        long durationMs,
        bool success,
        int statusCode,
        string? errorMessage,
        string? requestBodyEncrypted,
        string? encryptionKeyId,
        string? ipAddress,
        string? userAgent,
        string? connectionString)
    {
        UserId = userId;
        UserName = userName;
        UserEmail = userEmail;
        TenantId = tenantId;
        Action = action;
        Category = category;
        HttpMethod = httpMethod;
        Endpoint = endpoint;
        ExecutedAt = executedAt;
        DurationMs = durationMs;
        Success = success;
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
        RequestBodyEncrypted = requestBodyEncrypted;
        EncryptionKeyId = encryptionKeyId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        ConnectionString = connectionString;
    }
}

/// <summary>
/// Serviço de auditoria - enfileira logs no Hangfire para processamento assíncrono
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Enfileira um registro de auditoria para ser salvo em background
    /// </summary>
    void EnqueueAuditLog(AuditLogEntry entry);
}
