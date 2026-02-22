using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Tenant.Implementations;
using Template.Application.Common.Models;
using Template.Application.Common.Security;
using Template.Application.Domains.Tenant.V1.ViewModels.Audit;
using Template.Domain.Constants;

namespace Template.Application.Domains.Tenant.V1.Audit.Queries.GetAuditDashboard;

/// <summary>
/// Query para obter dados do dashboard de auditoria
/// Retorna KPIs e gráficos para um período configurável
/// Apenas Admin pode acessar
/// </summary>
[Authorize(Roles = $"{Roles.Admin},{Roles.TI}")]
public class GetAuditDashboardQuery
{
    /// <summary>
    /// Data de início do período (obrigatório)
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Data de fim do período (obrigatório)
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Quantidade de itens no top de ações (default: 5)
    /// </summary>
    public int TopActionsCount { get; set; } = 5;

    /// <summary>
    /// Quantidade de itens no top de usuários (default: 10)
    /// </summary>
    public int TopUsersCount { get; set; } = 10;

    /// <summary>
    /// Quantidade de itens no top de categorias (default: 5)
    /// </summary>
    public int TopCategoriesCount { get; set; } = 5;
}

public class GetAuditDashboardQueryHandler : HandlerBase<GetAuditDashboardQuery, AuditDashboardVM>
{
    private readonly IAuditDapperRepository _dapperRepository;

    public GetAuditDashboardQueryHandler(
        HandlerDependencies<GetAuditDashboardQuery, AuditDashboardVM> dependencies,
        IAuditDapperRepository dapperRepository) : base(dependencies)
    {
        _dapperRepository = dapperRepository;
    }

    protected override async Task<ApiResponse<AuditDashboardVM>> RunCore(
        GetAuditDashboardQuery request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        var startDate = request.StartDate.Date;
        var endDate = request.EndDate.Date.AddDays(1); // Inclui o dia inteiro

        // Usa Dapper para queries otimizadas (5 queries SQL diretas vs 8 do EF Core)
        var data = await _dapperRepository.GetDashboardDataAsync(
            startDate,
            endDate,
            request.TopActionsCount,
            request.TopUsersCount,
            request.TopCategoriesCount,
            cancellationToken);

        var dashboard = new AuditDashboardVM
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Kpis = new AuditKpiVM
            {
                UniqueUsers = data.UniqueUsers,
                TotalActions = data.TotalActions,
                AverageDurationMs = Math.Round(data.AverageDurationMs, 2),
                ErrorRate = Math.Round(data.ErrorRate, 2)
            },
            TopActions = data.TopActions
                .Select(x => new TopItemVM(x.Action, x.Count))
                .ToList(),
            TopUsers = data.TopUsers
                .Select(x => new TopUserVM(x.UserId, x.UserName, x.ActionCount, Math.Round(x.AvgDurationMs, 2)))
                .ToList(),
            ActionsByHour = data.ActionsByHour
                .Select(x => new ActionsByHourVM(x.Hour, x.Count))
                .ToList(),
            TopCategories = data.TopCategories
                .Select(x => new TopItemVM(x.Category, x.Count))
                .ToList()
        };

        return new SuccessResponse<AuditDashboardVM>("Dashboard de auditoria carregado.", dashboard);
    }
}
