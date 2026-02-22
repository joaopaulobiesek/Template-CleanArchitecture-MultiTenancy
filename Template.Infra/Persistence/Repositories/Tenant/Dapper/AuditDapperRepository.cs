using Template.Application.Common.Interfaces.IRepositories.Tenant.Implementations;

namespace Template.Infra.Persistence.Repositories.Dapper;

/// <summary>
/// Repositório Dapper para consultas de auditoria otimizadas.
/// Usa SQL direto para evitar overhead do Entity Framework em consultas pesadas.
/// </summary>
public class AuditDapperRepository : IAuditDapperRepository
{
    private readonly ITenantDapperConnection _dapper;

    public AuditDapperRepository(ITenantDapperConnection dapper)
    {
        _dapper = dapper;
    }

    public async Task<AuditDashboardDto> GetDashboardDataAsync(
        DateTime startDate,
        DateTime endDate,
        int topActionsCount,
        int topUsersCount,
        int topCategoriesCount,
        CancellationToken cancellationToken = default)
    {
        var result = new AuditDashboardDto();

        // Query 1: KPIs (todos em uma única query)
        var kpisSql = @"
            SELECT
                COUNT(DISTINCT CASE WHEN UserId <> 'anonymous' AND UserId IS NOT NULL AND UserId <> '' THEN UserId END) AS UniqueUsers,
                COUNT(*) AS TotalActions,
                ISNULL(AVG(CAST(DurationMs AS FLOAT)), 0) AS AverageDurationMs,
                CASE WHEN COUNT(*) > 0
                    THEN CAST(SUM(CASE WHEN Success = 0 THEN 1 ELSE 0 END) AS FLOAT) * 100.0 / COUNT(*)
                    ELSE 0
                END AS ErrorRate
            FROM AuditLog
            WHERE Active = 1 AND ExecutedAt >= @StartDate AND ExecutedAt < @EndDate";

        var kpis = await _dapper.QueryFirstOrDefaultAsync<KpisDto>(
            kpisSql,
            new { StartDate = startDate, EndDate = endDate },
            cancellationToken: cancellationToken);

        if (kpis != null)
        {
            result.UniqueUsers = kpis.UniqueUsers;
            result.TotalActions = kpis.TotalActions;
            result.AverageDurationMs = kpis.AverageDurationMs;
            result.ErrorRate = kpis.ErrorRate;
        }

        // Query 2: Top Actions
        var topActionsSql = @"
            SELECT TOP (@TopCount) Action, COUNT(*) AS Count
            FROM AuditLog
            WHERE Active = 1 AND ExecutedAt >= @StartDate AND ExecutedAt < @EndDate
            GROUP BY Action
            ORDER BY Count DESC";

        var topActions = await _dapper.QueryAsync<TopActionDto>(
            topActionsSql,
            new { StartDate = startDate, EndDate = endDate, TopCount = topActionsCount },
            cancellationToken: cancellationToken);

        result.TopActions = topActions.ToList();

        // Query 3: Top Users
        var topUsersSql = @"
            SELECT TOP (@TopCount)
                UserId,
                MAX(UserName) AS UserName,
                COUNT(*) AS ActionCount,
                AVG(CAST(DurationMs AS FLOAT)) AS AvgDurationMs
            FROM AuditLog
            WHERE Active = 1 AND ExecutedAt >= @StartDate AND ExecutedAt < @EndDate
            GROUP BY UserId
            ORDER BY ActionCount DESC";

        var topUsers = await _dapper.QueryAsync<TopUserDto>(
            topUsersSql,
            new { StartDate = startDate, EndDate = endDate, TopCount = topUsersCount },
            cancellationToken: cancellationToken);

        result.TopUsers = topUsers.ToList();

        // Query 4: Actions by Hour
        var actionsByHourSql = @"
            SELECT DATEPART(HOUR, ExecutedAt) AS Hour, COUNT(*) AS Count
            FROM AuditLog
            WHERE Active = 1 AND ExecutedAt >= @StartDate AND ExecutedAt < @EndDate
            GROUP BY DATEPART(HOUR, ExecutedAt)
            ORDER BY Hour";

        var actionsByHour = await _dapper.QueryAsync<ActionsByHourDto>(
            actionsByHourSql,
            new { StartDate = startDate, EndDate = endDate },
            cancellationToken: cancellationToken);

        result.ActionsByHour = actionsByHour.ToList();

        // Query 5: Top Categories
        var topCategoriesSql = @"
            SELECT TOP (@TopCount) Category, COUNT(*) AS Count
            FROM AuditLog
            WHERE Active = 1 AND ExecutedAt >= @StartDate AND ExecutedAt < @EndDate
            GROUP BY Category
            ORDER BY Count DESC";

        var topCategories = await _dapper.QueryAsync<TopCategoryDto>(
            topCategoriesSql,
            new { StartDate = startDate, EndDate = endDate, TopCount = topCategoriesCount },
            cancellationToken: cancellationToken);

        result.TopCategories = topCategories.ToList();

        return result;
    }

    public async Task<AuditFiltersDto> GetFiltersAsync(CancellationToken cancellationToken = default)
    {
        var result = new AuditFiltersDto();

        // Query 1: Distinct Categories
        var categoriesSql = @"
            SELECT DISTINCT Category
            FROM AuditLog
            WHERE Active = 1
            ORDER BY Category";

        var categories = await _dapper.QueryAsync<string>(
            categoriesSql,
            cancellationToken: cancellationToken);

        result.Categories = categories.ToList();

        // Query 2: Distinct Users
        var usersSql = @"
            SELECT DISTINCT UserId, MAX(UserName) AS UserName
            FROM AuditLog
            WHERE Active = 1
            GROUP BY UserId
            ORDER BY MAX(UserName)";

        var users = await _dapper.QueryAsync<AuditUserDto>(
            usersSql,
            cancellationToken: cancellationToken);

        result.Users = users.ToList();

        return result;
    }

    public async Task<AuditLogListResultDto> GetPaginatedAsync(
        string? search,
        Dictionary<string, string>? customFilter,
        string? sortColumn,
        bool ascending,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var result = new AuditLogListResultDto();

        // Monta WHERE clause
        var whereConditions = new List<string> { "Active = 1" };
        var parameters = new Dictionary<string, object>();

        // Busca textual
        if (!string.IsNullOrWhiteSpace(search))
        {
            whereConditions.Add(@"(
                UserName LIKE @Search OR
                UserEmail LIKE @Search OR
                Action LIKE @Search OR
                Category LIKE @Search OR
                Endpoint LIKE @Search
            )");
            parameters["Search"] = $"%{search}%";
        }

        // Filtros customizados
        if (customFilter != null)
        {
            if (customFilter.TryGetValue("StartDate", out var startDateStr) &&
                DateTime.TryParse(startDateStr, out var startDate))
            {
                whereConditions.Add("ExecutedAt >= @StartDate");
                parameters["StartDate"] = startDate;
            }

            if (customFilter.TryGetValue("EndDate", out var endDateStr) &&
                DateTime.TryParse(endDateStr, out var endDate))
            {
                whereConditions.Add("ExecutedAt < @EndDate");
                parameters["EndDate"] = endDate.AddDays(1);
            }

            if (customFilter.TryGetValue("Success", out var successStr) &&
                bool.TryParse(successStr, out var success))
            {
                whereConditions.Add("Success = @Success");
                parameters["Success"] = success;
            }

            if (customFilter.TryGetValue("Category", out var category) &&
                !string.IsNullOrWhiteSpace(category))
            {
                whereConditions.Add("Category = @Category");
                parameters["Category"] = category;
            }

            if (customFilter.TryGetValue("UserId", out var userId) &&
                !string.IsNullOrWhiteSpace(userId))
            {
                whereConditions.Add("UserId = @UserId");
                parameters["UserId"] = userId;
            }

            if (customFilter.TryGetValue("HttpMethod", out var httpMethod) &&
                !string.IsNullOrWhiteSpace(httpMethod))
            {
                whereConditions.Add("HttpMethod = @HttpMethod");
                parameters["HttpMethod"] = httpMethod;
            }
        }

        var whereClause = string.Join(" AND ", whereConditions);

        // Monta ORDER BY
        var orderColumn = sortColumn?.ToLower() switch
        {
            "username" => "UserName",
            "useremail" => "UserEmail",
            "action" => "Action",
            "category" => "Category",
            "httpmethod" => "HttpMethod",
            "endpoint" => "Endpoint",
            "durationms" => "DurationMs",
            "success" => "Success",
            "statuscode" => "StatusCode",
            _ => "ExecutedAt"
        };
        var orderDirection = ascending ? "ASC" : "DESC";

        // Query COUNT (sem os campos pesados)
        var countSql = $"SELECT COUNT(*) FROM AuditLog WHERE {whereClause}";
        result.TotalCount = await _dapper.QueryFirstOrDefaultAsync<int>(countSql, parameters, cancellationToken: cancellationToken);

        // Query paginada - EXCLUI RequestBodyEncrypted e EncryptionKeyId (campos pesados!)
        var offset = (pageNumber - 1) * pageSize;
        parameters["Offset"] = offset;
        parameters["PageSize"] = pageSize;

        var dataSql = $@"
            SELECT
                Id, UserId, UserName, UserEmail, Action, Category,
                HttpMethod, Endpoint, ExecutedAt, DurationMs,
                Success, StatusCode, ErrorMessage, IpAddress, UserAgent
            FROM AuditLog
            WHERE {whereClause}
            ORDER BY {orderColumn} {orderDirection}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = await _dapper.QueryAsync<AuditLogListItemDto>(dataSql, parameters, cancellationToken: cancellationToken);
        result.Items = items.ToList();

        return result;
    }

    #region Private DTOs

    private class KpisDto
    {
        public int UniqueUsers { get; set; }
        public int TotalActions { get; set; }
        public double AverageDurationMs { get; set; }
        public double ErrorRate { get; set; }
    }

    #endregion
}
