using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.Services;
using Template.Application.Common.Models;
using Template.Application.Domains.Core.V1.ViewModels;
using Microsoft.Extensions.Logging;

namespace Template.Application.Domains.Core.V1.Tenant.Queries.ResolveTenant;

/// <summary>
/// Query pública para resolução de TenantId baseado na URL de origem.
/// Este endpoint NÃO requer autenticação (sem [Authorize]).
///
/// SEGURANÇA: O endpoint resolve o tenant APENAS através dos headers HTTP
/// controlados pelo navegador (Origin/Referer). Não aceita URL como parâmetro
/// para evitar enumeração de TenantIds por atacantes.
/// </summary>
public class ResolveTenantQuery
{
    /// <summary>
    /// Header Origin da requisição (controlado pelo navegador)
    /// </summary>
    public string? Origin { get; set; }

    /// <summary>
    /// Header Referer da requisição (controlado pelo navegador)
    /// </summary>
    public string? Referer { get; set; }
}

public class ResolveTenantQueryHandler : HandlerBase<ResolveTenantQuery, TenantResolveResultVM>
{
    private readonly ITenantCacheService _tenantCacheService;
    private readonly ILogger<ResolveTenantQueryHandler> _logger;

    public ResolveTenantQueryHandler(
        HandlerDependencies<ResolveTenantQuery, TenantResolveResultVM> dependencies,
        ITenantCacheService tenantCacheService,
        ILogger<ResolveTenantQueryHandler> logger) : base(dependencies)
    {
        _tenantCacheService = tenantCacheService;
        _logger = logger;
    }

    protected override async Task<ApiResponse<TenantResolveResultVM>> RunCore(
        ResolveTenantQuery request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        string? resolvedUrl = null;
        string resolvedFrom = "Unknown";

        // 1. Tenta obter do header Origin (preferencial - controlado pelo navegador)
        if (!string.IsNullOrEmpty(request.Origin))
        {
            resolvedUrl = request.Origin;
            resolvedFrom = "Origin";
        }
        // 2. Tenta obter do header Referer (fallback - também controlado pelo navegador)
        else if (!string.IsNullOrEmpty(request.Referer))
        {
            resolvedUrl = request.Referer;
            resolvedFrom = "Referer";
        }

        // SEGURANÇA: Não aceitamos URL como parâmetro para evitar enumeração de TenantIds
        if (string.IsNullOrEmpty(resolvedUrl))
        {

            return new ErrorResponse<TenantResolveResultVM>(
                "Unable to determine the origin URL. This endpoint only accepts requests with Origin or Referer headers (browser requests).",
                400);
        }

        var tenantId = await _tenantCacheService.GetTenantIdByUrlAsync(resolvedUrl, cancellationToken);

        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
        {
            return new ErrorResponse<TenantResolveResultVM>(
                $"No tenant found for the provided URL.",
                404);
        }

        // Busca o TimeZoneId do tenant
        var timeZoneId = await _tenantCacheService.GetTimeZoneIdAsync(tenantId.Value, cancellationToken);

        return new SuccessResponse<TenantResolveResultVM>(
            "Tenant resolved successfully.",
            new TenantResolveResultVM(
                tenantId.Value,
                resolvedFrom,
                resolvedUrl,
                timeZoneId
            )
        );
    }
}
