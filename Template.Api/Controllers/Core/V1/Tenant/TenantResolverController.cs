using Template.Api.Controllers.System;
using Template.Application.Common.Behaviours;
using Template.Application.Common.Models;
using Template.Application.Domains.Core.V1.Tenant.Queries.ResolveTenant;
using Template.Application.Domains.Core.V1.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Template.Api.Controllers.Core.V1.Tenant;

/// <summary>
/// Controller público para resolução de Tenant baseado na URL de origem.
/// Este endpoint é 100% aberto (sem autenticação) para permitir que o frontend
/// descubra qual TenantId usar antes de qualquer requisição autenticada.
/// </summary>
[ApiController]
[Route("core/api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "Core.Api.v1")]
public class TenantResolverController : BaseController
{
    /// <summary>
    /// Resolve o TenantId baseado na URL de origem da requisição.
    /// </summary>
    /// <remarks>
    /// Este endpoint é **100% público** (sem autenticação) e deve ser chamado pelo frontend
    /// para descobrir qual TenantId usar nas requisições subsequentes.
    ///
    /// **SEGURANÇA:** O endpoint resolve o tenant APENAS através dos headers HTTP controlados pelo navegador:
    /// 1. Header `Origin` (preferencial - enviado automaticamente em requisições CORS)
    /// 2. Header `Referer` (fallback - enviado pelo navegador)
    ///
    /// **Por que não aceitar URL como parâmetro?**
    /// Aceitar URL como query parameter permitiria que qualquer pessoa enumere TenantIds
    /// de outros clientes, expondo informações sensíveis do sistema.
    ///
    /// **Exemplo de uso:**
    /// ```
    /// GET /core/api/v1/TenantResolver/Resolve
    /// Origin: https://feira.Template.com
    /// ```
    ///
    /// **Resposta:**
    /// ```json
    /// {
    ///   "success": true,
    ///   "message": "Tenant resolved successfully.",
    ///   "data": {
    ///     "tenantId": "00000000-0000-0000-0000-000000000000",
    ///     "resolvedFrom": "Origin"
    ///   }
    /// }
    /// ```
    /// </remarks>
    /// <param name="handler">Handler injetado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>TenantId correspondente à URL de origem</returns>
    /// <response code="200">Tenant resolvido com sucesso</response>
    /// <response code="400">Headers Origin/Referer não presentes na requisição</response>
    /// <response code="404">Tenant não encontrado para a URL de origem</response>
    [HttpGet("Resolve")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<TenantResolveResultVM>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse<TenantResolveResultVM>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse<TenantResolveResultVM>))]
    public async Task<IActionResult> ResolveAsync(
        [FromServices] IHandlerBase<ResolveTenantQuery, TenantResolveResultVM> handler,
        CancellationToken cancellationToken)
    {
        // Extrai headers e passa para o handler
        var query = new ResolveTenantQuery
        {
            Origin = Request.Headers["Origin"].ToString(),
            Referer = Request.Headers["Referer"].ToString()
        };

        return HandleResponse(await handler.Execute(query, cancellationToken));
    }
}
