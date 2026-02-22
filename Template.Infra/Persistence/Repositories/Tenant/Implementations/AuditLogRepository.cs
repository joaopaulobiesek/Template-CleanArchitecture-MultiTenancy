using Template.Application.Common.Interfaces.IRepositories.Tenant.Implementations;
using Template.Domain.Entity.Tenant.Audit;
using Template.Infra.Persistence.Repositories.Tenant.Base;

namespace Template.Infra.Persistence.Repositories.Tenant.Implementations;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<AuditLog>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.Active, cancellationToken);
    }

    public IQueryable<AuditLog> SearchIQueryable(string? src, Dictionary<string, string>? customFilter = null)
    {
        IQueryable<AuditLog> query = _context.Set<AuditLog>()
            .AsNoTracking()
            .Where(x => x.Active);

        // Busca textual
        if (!string.IsNullOrWhiteSpace(src))
        {
            query = query.Where(x =>
                x.UserName != null && x.UserName.Contains(src) ||
                x.UserEmail != null && x.UserEmail.Contains(src) ||
                x.Action.Contains(src) ||
                x.Category.Contains(src) ||
                x.Endpoint != null && x.Endpoint.Contains(src)
            );
        }

        // Filtros customizados
        if (customFilter != null && customFilter.Any())
        {
            // Filtro por período
            if (customFilter.TryGetValue("StartDate", out var startDateStr) &&
                DateTime.TryParse(startDateStr, out var startDate))
            {
                query = query.Where(x => x.ExecutedAt >= startDate);
            }

            if (customFilter.TryGetValue("EndDate", out var endDateStr) &&
                DateTime.TryParse(endDateStr, out var endDate))
            {
                // Adiciona 1 dia para incluir o dia inteiro
                query = query.Where(x => x.ExecutedAt < endDate.AddDays(1));
            }

            // Filtro por sucesso/erro
            if (customFilter.TryGetValue("Success", out var successStr) &&
                bool.TryParse(successStr, out var success))
            {
                query = query.Where(x => x.Success == success);
            }

            // Filtro por categoria
            if (customFilter.TryGetValue("Category", out var category) &&
                !string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(x => x.Category == category);
            }

            // Filtro por usuário
            if (customFilter.TryGetValue("UserId", out var userId) &&
                !string.IsNullOrWhiteSpace(userId))
            {
                query = query.Where(x => x.UserId == userId);
            }

            // Filtro por método HTTP
            if (customFilter.TryGetValue("HttpMethod", out var httpMethod) &&
                !string.IsNullOrWhiteSpace(httpMethod))
            {
                query = query.Where(x => x.HttpMethod == httpMethod);
            }
        }

        return query;
    }

    public async Task<int> GetUniqueUsersCountAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.Set<AuditLog>()
            .Where(x => x.Active && x.ExecutedAt >= startDate && x.ExecutedAt < endDate)
            .Where(x => x.UserId != "anonymous" && x.UserId != null && x.UserId != "") // Exclui anonymous e null
            .Select(x => x.UserId)
            .Distinct()
            .CountAsync(ct);
    }

    public async Task<int> GetTotalActionsCountAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        return await _context.Set<AuditLog>()
            .Where(x => x.Active && x.ExecutedAt >= startDate && x.ExecutedAt < endDate)
            .CountAsync(ct);
    }

    public async Task<double> GetAverageDurationAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var query = _context.Set<AuditLog>()
            .Where(x => x.Active && x.ExecutedAt >= startDate && x.ExecutedAt < endDate);

        if (!await query.AnyAsync(ct))
            return 0;

        return await query.AverageAsync(x => x.DurationMs, ct);
    }

    public async Task<double> GetErrorRateAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var query = _context.Set<AuditLog>()
            .Where(x => x.Active && x.ExecutedAt >= startDate && x.ExecutedAt < endDate);

        var total = await query.CountAsync(ct);
        if (total == 0)
            return 0;

        var errors = await query.CountAsync(x => !x.Success, ct);
        return (double)errors / total * 100;
    }

    public async Task<List<(string Action, int Count)>> GetTopActionsAsync(DateTime startDate, DateTime endDate, int top = 5, CancellationToken ct = default)
    {
        var result = await _context.Set<AuditLog>()
            .Where(x => x.Active && x.ExecutedAt >= startDate && x.ExecutedAt < endDate)
            .GroupBy(x => x.Action)
            .Select(g => new { Action = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToListAsync(ct);

        return result.Select(x => (x.Action, x.Count)).ToList();
    }

    public async Task<List<(string UserId, string? UserName, int ActionCount, double AvgDurationMs)>> GetTopUsersAsync(DateTime startDate, DateTime endDate, int top = 10, CancellationToken ct = default)
    {
        var result = await _context.Set<AuditLog>()
            .Where(x => x.Active && x.ExecutedAt >= startDate && x.ExecutedAt < endDate)
            .GroupBy(x => new { x.UserId, x.UserName })
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.UserName,
                ActionCount = g.Count(),
                AvgDurationMs = g.Average(x => x.DurationMs)
            })
            .OrderByDescending(x => x.ActionCount)
            .Take(top)
            .ToListAsync(ct);

        return result.Select(x => (x.UserId, x.UserName, x.ActionCount, x.AvgDurationMs)).ToList();
    }

    public async Task<List<(int Hour, int Count)>> GetActionsByHourAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        var result = await _context.Set<AuditLog>()
            .Where(x => x.Active && x.ExecutedAt >= startDate && x.ExecutedAt < endDate)
            .GroupBy(x => x.ExecutedAt.Hour)
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .OrderBy(x => x.Hour)
            .ToListAsync(ct);

        return result.Select(x => (x.Hour, x.Count)).ToList();
    }

    public async Task<List<(string Category, int Count)>> GetTopCategoriesAsync(DateTime startDate, DateTime endDate, int top = 5, CancellationToken ct = default)
    {
        var result = await _context.Set<AuditLog>()
            .Where(x => x.Active && x.ExecutedAt >= startDate && x.ExecutedAt < endDate)
            .GroupBy(x => x.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToListAsync(ct);

        return result.Select(x => (x.Category, x.Count)).ToList();
    }

    public async Task<List<string>> GetDistinctCategoriesAsync(CancellationToken ct = default)
    {
        return await _context.Set<AuditLog>()
            .Where(x => x.Active)
            .Select(x => x.Category)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(ct);
    }

    public async Task<List<(string UserId, string? UserName)>> GetDistinctUsersAsync(CancellationToken ct = default)
    {
        var result = await _context.Set<AuditLog>()
            .Where(x => x.Active)
            .Select(x => new { x.UserId, x.UserName })
            .Distinct()
            .OrderBy(x => x.UserName)
            .ToListAsync(ct);

        return result.Select(x => (x.UserId, x.UserName)).ToList();
    }
}
