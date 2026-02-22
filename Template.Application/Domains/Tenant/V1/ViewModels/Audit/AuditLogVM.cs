using Template.Domain.Entity.Tenant.Audit;

namespace Template.Application.Domains.Tenant.V1.ViewModels.Audit;

/// <summary>
/// ViewModel para exibição de um registro de auditoria
/// </summary>
public class AuditLogVM
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? HttpMethod { get; set; }
    public string? Endpoint { get; set; }
    public DateTime ExecutedAt { get; set; }
    public long DurationMs { get; set; }
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    /// <summary>
    /// RequestBody descriptografado (preenchido apenas na rota de decrypt)
    /// </summary>
    public string? RequestBodyDecrypted { get; set; }

    public AuditLogVM() { }

    public AuditLogVM(
        Guid id,
        string userId,
        string? userName,
        string? userEmail,
        string action,
        string category,
        string? httpMethod,
        string? endpoint,
        DateTime executedAt,
        long durationMs,
        bool success,
        int statusCode,
        string? errorMessage,
        string? ipAddress,
        string? userAgent)
    {
        Id = id;
        UserId = userId;
        UserName = userName;
        UserEmail = userEmail;
        Action = action;
        Category = category;
        HttpMethod = httpMethod;
        Endpoint = endpoint;
        ExecutedAt = executedAt;
        DurationMs = durationMs;
        Success = success;
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public static AuditLogVM FromDomain(AuditLog? entity)
    {
        if (entity == null) return new AuditLogVM();

        return new AuditLogVM(
            entity.Id,
            entity.UserId,
            entity.UserName,
            entity.UserEmail,
            entity.Action,
            entity.Category,
            entity.HttpMethod,
            entity.Endpoint,
            entity.ExecutedAt,
            entity.DurationMs,
            entity.Success,
            entity.StatusCode,
            entity.ErrorMessage,
            entity.IpAddress,
            entity.UserAgent
        );
    }

    public static AuditLogVM FromDto(Common.Interfaces.IRepositories.Tenant.Implementations.AuditLogListItemDto dto)
    {
        return new AuditLogVM(
            dto.Id,
            dto.UserId,
            dto.UserName,
            dto.UserEmail,
            dto.Action,
            dto.Category,
            dto.HttpMethod,
            dto.Endpoint,
            dto.ExecutedAt,
            dto.DurationMs,
            dto.Success,
            dto.StatusCode,
            dto.ErrorMessage,
            dto.IpAddress,
            dto.UserAgent
        );
    }
}
