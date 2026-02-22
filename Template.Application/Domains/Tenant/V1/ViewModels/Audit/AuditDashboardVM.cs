namespace Template.Application.Domains.Tenant.V1.ViewModels.Audit;

/// <summary>
/// ViewModel para o dashboard de auditoria com KPIs e gráficos
/// </summary>
public class AuditDashboardVM
{
    /// <summary>
    /// Data de início do período consultado
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Data de fim do período consultado
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// KPIs principais
    /// </summary>
    public AuditKpiVM Kpis { get; set; } = new();

    /// <summary>
    /// Top ações mais executadas
    /// </summary>
    public List<TopItemVM> TopActions { get; set; } = new();

    /// <summary>
    /// Top usuários mais ativos
    /// </summary>
    public List<TopUserVM> TopUsers { get; set; } = new();

    /// <summary>
    /// Distribuição de ações por hora do dia
    /// </summary>
    public List<ActionsByHourVM> ActionsByHour { get; set; } = new();

    /// <summary>
    /// Top categorias mais acessadas
    /// </summary>
    public List<TopItemVM> TopCategories { get; set; } = new();
}

/// <summary>
/// KPIs do dashboard de auditoria
/// </summary>
public class AuditKpiVM
{
    /// <summary>
    /// Total de usuários únicos no período
    /// </summary>
    public int UniqueUsers { get; set; }

    /// <summary>
    /// Total de ações executadas no período
    /// </summary>
    public int TotalActions { get; set; }

    /// <summary>
    /// Média de duração das requisições em ms
    /// </summary>
    public double AverageDurationMs { get; set; }

    /// <summary>
    /// Taxa de erro (0-100%)
    /// </summary>
    public double ErrorRate { get; set; }
}

/// <summary>
/// Item generico para top N (acoes, categorias)
/// </summary>
public class TopItemVM
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }

    public TopItemVM() { }

    public TopItemVM(string name, int count)
    {
        Name = name;
        Count = count;
    }
}

/// <summary>
/// Usuario no ranking de mais ativos
/// </summary>
public class TopUserVM
{
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public int ActionCount { get; set; }
    public double AvgDurationMs { get; set; }

    public TopUserVM() { }

    public TopUserVM(string userId, string? userName, int actionCount, double avgDurationMs)
    {
        UserId = userId;
        UserName = userName;
        ActionCount = actionCount;
        AvgDurationMs = avgDurationMs;
    }
}

/// <summary>
/// Distribuicao de acoes por hora
/// </summary>
public class ActionsByHourVM
{
    public int Hour { get; set; }
    public int Count { get; set; }

    public ActionsByHourVM() { }

    public ActionsByHourVM(int hour, int count)
    {
        Hour = hour;
        Count = count;
    }
}
