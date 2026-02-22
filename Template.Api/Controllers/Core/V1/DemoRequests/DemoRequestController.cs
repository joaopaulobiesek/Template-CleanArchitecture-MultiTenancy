using Template.Api.Controllers.System;
using Template.Application.Common.Behaviours;
using Template.Application.Common.Models;
using Template.Application.Domains.Core.V1.DemoRequests.Commands.CreateDemoRequest;
using Template.Application.Domains.Core.V1.DemoRequests.Commands.UpdateDemoRequestStatus;
using Template.Application.Domains.Core.V1.DemoRequests.Queries.GetAllDemoRequests;
using Template.Application.Domains.Core.V1.ViewModels.DemoRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Template.Api.Controllers.Core.V1.DemoRequests;

/// <summary>
/// Controller para gerenciar solicitações de demonstração do site.
/// POST (criar) é público, GET/PUT requerem Admin.
/// </summary>
[ApiController]
[Route("core/api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "Core.Api.v1")]
public class DemoRequestController : BaseController
{
    /// <summary>
    /// Cria uma nova solicitação de demonstração.
    /// Endpoint PÚBLICO - não requer autenticação.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateAsync(
        [FromServices] IHandlerBase<CreateDemoRequestCommand, DemoRequestVM> handler,
        [FromBody] CreateDemoRequestCommand command,
        CancellationToken cancellationToken)
        => HandleResponse(await handler.Execute(command, cancellationToken));

    /// <summary>
    /// Lista todas as solicitações de demonstração.
    /// Requer autenticação - apenas Admin.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllAsync(
        [FromServices] IHandlerBase<GetAllDemoRequestsQuery, IEnumerable<DemoRequestVM>> handler,
        [FromQuery] GetAllDemoRequestsQuery query,
        CancellationToken cancellationToken)
        => HandleResponse(await handler.Execute(query, cancellationToken));

    /// <summary>
    /// Atualiza o status de uma solicitação de demonstração.
    /// Requer autenticação - apenas Admin.
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatusAsync(
        [FromServices] IHandlerBase<UpdateDemoRequestStatusCommand, DemoRequestVM> handler,
        [FromRoute] Guid id,
        [FromBody] UpdateDemoRequestStatusCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;
        return HandleResponse(await handler.Execute(command, cancellationToken));
    }
}
