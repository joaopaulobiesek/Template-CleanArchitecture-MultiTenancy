namespace Template.Application.Common.Interfaces.IRepositories.Tenant.Implementations;

/// <summary>
/// Repositorio Dapper para consultas de auditoria otimizadas.
/// Usa SQL direto para evitar overhead do Entity Framework em consultas pesadas.
/// </summary>
public interface IAuditDapperRepository
{
    /// <summary>
    /// Obtem todos os dados do dashboard em uma unica query otimizada
    /// </summary>
    Task<AuditDashboardDto> GetDashboardDataAsync(
        DateTime startDate,
        DateTime endDate,
        int topActionsCount,
        int topUsersCount,
        int topCategoriesCount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtem filtros disponiveis (categorias e usuarios distintos)
    /// </summary>
    Task<AuditFiltersDto> GetFiltersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Listagem paginada de audit logs (sem RequestBodyEncrypted para performance)
    /// </summary>
    Task<AuditLogListResultDto> GetPaginatedAsync(
        string? search,
        Dictionary<string, string>? customFilter,
        string? sortColumn,
        bool ascending,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}

#region DTOs

public class AuditLogListResultDto
{
    public List<AuditLogListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class AuditLogListItemDto
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
}

public class AuditDashboardDto
{
    public int UniqueUsers { get; set; }
    public int TotalActions { get; set; }
    public double AverageDurationMs { get; set; }
    public double ErrorRate { get; set; }
    public List<TopActionDto> TopActions { get; set; } = new();
    public List<TopUserDto> TopUsers { get; set; } = new();
    public List<ActionsByHourDto> ActionsByHour { get; set; } = new();
    public List<TopCategoryDto> TopCategories { get; set; } = new();
}

public class TopActionDto
{
    public string Action { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class TopUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public int ActionCount { get; set; }
    public double AvgDurationMs { get; set; }
}

public class ActionsByHourDto
{
    public int Hour { get; set; }
    public int Count { get; set; }
}

public class TopCategoryDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class AuditFiltersDto
{
    public List<string> Categories { get; set; } = new();
    public List<AuditUserDto> Users { get; set; } = new();
}

public class AuditUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
}

#endregion
