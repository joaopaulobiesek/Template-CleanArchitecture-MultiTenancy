using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Tenant.Implementations;
using Template.Application.Common.Models;
using Template.Application.Common.Security;
using Template.Application.Domains.Tenant.V1.ViewModels.Audit;
using Template.Domain.Constants;

namespace Template.Application.Domains.Tenant.V1.Audit.Queries.GetAuditFilters;

/// <summary>
/// Query para obter opções de filtros disponíveis para a listagem de audit logs
/// Retorna categorias e usuários distintos para popular selects no frontend
/// Apenas Admin pode acessar
/// </summary>
[Authorize(Roles = $"{Roles.Admin},{Roles.TI}")]
public class GetAuditFiltersQuery
{
}

public class GetAuditFiltersQueryHandler : HandlerBase<GetAuditFiltersQuery, AuditFiltersVM>
{
    private readonly IAuditDapperRepository _dapperRepository;

    public GetAuditFiltersQueryHandler(
        HandlerDependencies<GetAuditFiltersQuery, AuditFiltersVM> dependencies,
        IAuditDapperRepository dapperRepository) : base(dependencies)
    {
        _dapperRepository = dapperRepository;
    }

    protected override async Task<ApiResponse<AuditFiltersVM>> RunCore(
        GetAuditFiltersQuery request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        // Usa Dapper para queries otimizadas (SQL direto sem overhead do EF Core)
        var data = await _dapperRepository.GetFiltersAsync(cancellationToken);

        var filters = new AuditFiltersVM
        {
            Categories = data.Categories,
            Users = data.Users
                .Select(x => new AuditUserFilterVM(x.UserId, x.UserName))
                .ToList()
        };

        return new SuccessResponse<AuditFiltersVM>("Filtros carregados.", filters);
    }
}
