using Microsoft.AspNetCore.Mvc;
using Template.Api.Controllers.System;
using Template.Application.Common.Behaviours;
using Template.Application.Common.Models;
using Template.Application.Domains.Core.V1.Clients.Commands.CreateClient;
using Template.Application.Domains.Core.V1.Clients.Commands.DeactivateClient;
using Template.Application.Domains.Core.V1.Clients.Commands.ReactivateClient;
using Template.Application.Domains.Core.V1.Clients.Commands.UpdateClient;
using Template.Application.Domains.Core.V1.Clients.Queries.GetAll;
using Template.Application.Domains.Core.V1.Clients.Queries.GetById;
using Template.Application.Domains.Core.V1.Clients.Queries.GetSimple;
using Template.Application.Domains.Core.V1.ViewModels;

namespace Template.Api.Controllers.Core.V1.Clients;

[ApiController]
[Route("core/api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "Core.Api.v1")]
public class ClientController : BaseController
{
    /// <summary>
    /// Responsável por Registrar um novo cliente.
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<ClientVM>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse<ClientVM>))]
    public async Task<IActionResult> CreateClientAsync(
        [FromServices] IHandlerBase<CreateClientCommand, ClientVM> handler,
        [FromBody] CreateClientCommand command,
        CancellationToken cancellationToken)
        => HandleResponse(await handler.Execute(command, cancellationToken));

    /// <summary>
    /// Responsável por Editar um cliente.
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<ClientVM>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse<ClientVM>))]
    public async Task<IActionResult> UpdateClientAsync(
        [FromServices] IHandlerBase<UpdateClientCommand, ClientVM> handler,
        [FromBody] UpdateClientCommand command,
        CancellationToken cancellationToken)
        => HandleResponse(await handler.Execute(command, cancellationToken));

    /// <summary>
    /// Responsável por listar todos os clientes.
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedList<ClientVM>))]
    public async Task<IActionResult> GetAllAsync(
        [FromServices] IHandlerBase<GetAllQuery, IEnumerable<ClientVM>> handler,
        [FromQuery] GetAllQuery query,
        CancellationToken cancellationToken)
        => HandleResponse(await handler.Execute(query, cancellationToken));

    /// <summary>
    /// Retorna todos os dados de um cliente por ID, incluindo configurações.
    /// </summary>
    [HttpGet("GetById")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<ClientDetailVM>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse<ClientDetailVM>))]
    public async Task<IActionResult> GetByIdAsync(
        [FromServices] IHandlerBase<GetByIdQuery, ClientDetailVM> handler,
        [FromQuery] GetByIdQuery query,
        CancellationToken cancellationToken)
        => HandleResponse(await handler.Execute(query, cancellationToken));

    /// <summary>
    /// Lista clientes de forma simplificada para selects/dropdowns.
    /// Retorna Id e DisplayName (FullName - DocumentNumber).
    /// Se src for null, retorna todos. Busca por FullName, DocumentNumber e Email.
    /// </summary>
    [HttpGet("Simple")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<IEnumerable<ClientSimpleVM>>))]
    public async Task<IActionResult> GetSimpleAsync(
        [FromServices] IHandlerBase<GetClientsSimpleQuery, IEnumerable<ClientSimpleVM>> handler,
        [FromQuery] GetClientsSimpleQuery query,
        CancellationToken cancellationToken)
        => HandleResponse(await handler.Execute(query, cancellationToken));

    /// <summary>
    /// Responsável por desativar um cliente.
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPatch("Deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<string>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse<string>))]
    public async Task<IActionResult> DeactivateClientAsync(
        [FromServices] IHandlerBase<DeactivateClientCommand, string> handler,
        [FromBody] DeactivateClientCommand command, CancellationToken cancellationToken)
        => HandleResponse(await handler.Execute(command, cancellationToken));

    /// <summary>
    /// Responsável por reativar um cliente.
    /// </summary>
    [HttpPatch("Reactivate")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<string>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse<string>))]
    public async Task<IActionResult> ReactivateClientAsync(
        [FromServices] IHandlerBase<ReactivateClientCommand, string> handler,
        [FromBody] ReactivateClientCommand command, CancellationToken cancellationToken)
        => HandleResponse(await handler.Execute(command, cancellationToken));

    /// <summary>
    /// Responsável por excluir um cliente fisicamente.
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="clientId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("Delete/{clientId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<string>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse<string>))]
    public async Task<IActionResult> DeleteClientAsync(
        [FromServices] IHandlerBase<Guid, string> handler,
        Guid clientId, CancellationToken cancellationToken)
        => HandleResponse(await handler.Execute(clientId, cancellationToken));
}