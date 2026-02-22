using Template.Application.Common.Interfaces.IRepositories.Tenant.Base;
using Template.Domain.Entity.Tenant.Audit;

namespace Template.Application.Common.Interfaces.IRepositories.Tenant.Implementations;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    /// <summary>
    /// Busca log de auditoria por ID
    /// </summary>
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca paginada com filtros
    /// </summary>
    IQueryable<AuditLog> SearchIQueryable(string? src, Dictionary<string, string>? customFilter = null);

    /// <summary>
    /// Total de usuários únicos em um período
    /// </summary>
    Task<int> GetUniqueUsersCountAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);

    /// <summary>
    /// Total de ações em um período
    /// </summary>
    Task<int> GetTotalActionsCountAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);

    /// <summary>
    /// Média de duração em ms em um período
    /// </summary>
    Task<double> GetAverageDurationAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);

    /// <summary>
    /// Taxa de erro em um período (0-100%)
    /// </summary>
    Task<double> GetErrorRateAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);

    /// <summary>
    /// Top N ações mais executadas em um período
    /// </summary>
    Task<List<(string Action, int Count)>> GetTopActionsAsync(DateTime startDate, DateTime endDate, int top = 5, CancellationToken ct = default);

    /// <summary>
    /// Top N usuários mais ativos em um período
    /// </summary>
    Task<List<(string UserId, string? UserName, int ActionCount, double AvgDurationMs)>> GetTopUsersAsync(DateTime startDate, DateTime endDate, int top = 10, CancellationToken ct = default);

    /// <summary>
    /// Ações agrupadas por hora em um período
    /// </summary>
    Task<List<(int Hour, int Count)>> GetActionsByHourAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default);

    /// <summary>
    /// Top N categorias mais acessadas
    /// </summary>
    Task<List<(string Category, int Count)>> GetTopCategoriesAsync(DateTime startDate, DateTime endDate, int top = 5, CancellationToken ct = default);

    /// <summary>
    /// Lista de categorias disponíveis para filtro
    /// </summary>
    Task<List<string>> GetDistinctCategoriesAsync(CancellationToken ct = default);

    /// <summary>
    /// Lista de usuários disponíveis para filtro
    /// </summary>
    Task<List<(string UserId, string? UserName)>> GetDistinctUsersAsync(CancellationToken ct = default);
}
