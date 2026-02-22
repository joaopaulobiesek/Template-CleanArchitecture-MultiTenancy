using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Tenant.Implementations;
using Template.Application.Common.Models;
using Template.Application.Common.Security;
using Template.Application.Domains.Tenant.V1.ViewModels.Audit;
using Template.Domain.Constants;

namespace Template.Application.Domains.Tenant.V1.Audit.Queries.GetAllAuditLogs;

/// <summary>
/// Query para listar logs de auditoria com paginação e filtros
/// Apenas Admin pode acessar
/// </summary>
[Authorize(Roles = $"{Roles.Admin},{Roles.TI}")]
public class GetAllAuditLogsQuery : BasePaginatedQuery
{
    // Herda de BasePaginatedQuery:
    // - PageNumber
    // - PageSize
    // - Src (busca textual)
    // - ColumnName (ordenação)
    // - AscDesc (1 = ASC, -1 = DESC)
    // - CustomFilter (filtros JSON)
    //   Filtros suportados:
    //   - StartDate: Data de início (yyyy-MM-dd)
    //   - EndDate: Data de fim (yyyy-MM-dd)
    //   - Success: true/false
    //   - Category: Nome da categoria
    //   - UserId: ID do usuário
    //   - HttpMethod: GET, POST, PUT, DELETE
}

/// <summary>
/// Handler otimizado usando Dapper para evitar carregar RequestBodyEncrypted (campo pesado).
/// Melhora performance de 40-80s para menos de 1s em 3300 registros.
/// </summary>
public class GetAllAuditLogsQueryHandler : HandlerBase<GetAllAuditLogsQuery, IEnumerable<AuditLogVM>>
{
    private readonly IAuditDapperRepository _dapperRepository;

    public GetAllAuditLogsQueryHandler(
        HandlerDependencies<GetAllAuditLogsQuery, IEnumerable<AuditLogVM>> dependencies,
        IAuditDapperRepository dapperRepository) : base(dependencies)
    {
        _dapperRepository = dapperRepository;
    }

    protected override async Task<ApiResponse<IEnumerable<AuditLogVM>>> RunCore(
        GetAllAuditLogsQuery request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        // Usa Dapper para query otimizada (exclui RequestBodyEncrypted)
        var result = await _dapperRepository.GetPaginatedAsync(
            request.Src,
            request.GetCustomFilterDictionary(),
            request.ColumnName,
            request.AscDesc != 0, // 0 = DESC, != 0 = ASC
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        // Mapeia DTOs para ViewModels
        var viewModels = result.Items.Select(AuditLogVM.FromDto).ToList();

        // Retorna PaginatedList
        return new PaginatedList<AuditLogVM>(
            viewModels,
            result.TotalCount,
            request.PageNumber,
            request.PageSize
        );
    }
}
