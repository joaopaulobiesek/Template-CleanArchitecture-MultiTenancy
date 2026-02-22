using Template.Api.Controllers.System;
using Template.Application.Common.Behaviours;
using Template.Application.Common.Models;
using Template.Application.Domains.Tenant.V1.Audit.Queries.DecryptAuditLog;
using Template.Application.Domains.Tenant.V1.Audit.Queries.GetAllAuditLogs;
using Template.Application.Domains.Tenant.V1.Audit.Queries.GetAuditDashboard;
using Template.Application.Domains.Tenant.V1.Audit.Queries.GetAuditFilters;
using Template.Application.Domains.Tenant.V1.ViewModels.Audit;
using Microsoft.AspNetCore.Mvc;

namespace Template.Api.Controllers.Tenant.V1.Audit;

/// <summary>
/// Controller para auditoria do sistema
/// Apenas Admin pode acessar
/// </summary>
[ApiController]
[Route("tenant/api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "Tenant.Api.v1")]
public class AuditLogsController : BaseController
{
    /// <summary>
    /// Lista todos os logs de auditoria (paginado)
    /// </summary>
    /// <remarks>
    /// Filtros customizados disponíveis via CustomFilter (JSON):
    /// - StartDate: Data de início (yyyy-MM-dd)
    /// - EndDate: Data de fim (yyyy-MM-dd)
    /// - Success: true/false
    /// - Category: Nome da categoria
    /// - UserId: ID do usuário
    /// - HttpMethod: GET, POST, PUT, DELETE
    ///
    /// Exemplo: CustomFilter={"StartDate":"2025-01-01","EndDate":"2025-01-31","Success":"true"}
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedList<AuditLogVM>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllAsync(
        [FromServices] IHandlerBase<GetAllAuditLogsQuery, IEnumerable<AuditLogVM>> handler,
        [FromQuery] GetAllAuditLogsQuery query,
        CancellationToken cancellationToken)
        => HandleResponse(await handler.Execute(query, cancellationToken));

    /// <summary>
    /// Obtém dados do dashboard de auditoria (KPIs e gráficos)
    /// </summary>
    /// <remarks>
    /// Retorna:
    /// - KPIs: Usuários únicos, total de ações, duração média, taxa de erro
    /// - Top ações mais executadas
    /// - Top usuários mais ativos
    /// - Distribuição de ações por hora
    /// - Top categorias
    /// </remarks>
    [HttpGet("dashboard")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<AuditDashboardVM>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDashboardAsync(
        [FromServices] IHandlerBase<GetAuditDashboardQuery, AuditDashboardVM> handler,
        [FromQuery] GetAuditDashboardQuery query,
        CancellationToken cancellationToken)
        => HandleResponse(await handler.Execute(query, cancellationToken));

    /// <summary>
    /// Obtém opções de filtros disponíveis (categorias e usuários)
    /// </summary>
    /// <remarks>
    /// Retorna listas distintas de:
    /// - Categorias existentes nos logs
    /// - Usuários que possuem logs
    ///
    /// Usar para popular selects/dropdowns no frontend
    /// </remarks>
    [HttpGet("filters")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<AuditFiltersVM>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFiltersAsync(
        [FromServices] IHandlerBase<GetAuditFiltersQuery, AuditFiltersVM> handler,
        CancellationToken cancellationToken)
        => HandleResponse(await handler.Execute(new GetAuditFiltersQuery(), cancellationToken));

    /// <summary>
    /// Busca log de auditoria por ID com RequestBody descriptografado
    /// </summary>
    /// <param name="id">ID do log de auditoria</param>
    [HttpGet("{id:guid}/decrypt")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<AuditLogVM>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DecryptAsync(
        [FromServices] IHandlerBase<DecryptAuditLogQuery, AuditLogVM> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
        => HandleResponse(await handler.Execute(new DecryptAuditLogQuery { Id = id }, cancellationToken));
}
