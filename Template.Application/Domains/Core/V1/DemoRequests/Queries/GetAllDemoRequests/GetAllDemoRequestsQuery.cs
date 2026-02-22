using Template.Application.Common.Models;
using Template.Application.Common.Security;
using Template.Domain.Constants;
using static Template.Domain.Constants.Policies;

namespace Template.Application.Domains.Core.V1.DemoRequests.Queries.GetAllDemoRequests;

/// <summary>
/// Query para listar solicitações de demonstração.
/// Requer autenticação - apenas Admin do Core pode acessar.
/// </summary>
[Authorize(Roles = Roles.Admin)]
[Authorize(Policy = $"{CanList},{CanView}")]
public class GetAllDemoRequestsQuery : BasePaginatedQuery
{
}
