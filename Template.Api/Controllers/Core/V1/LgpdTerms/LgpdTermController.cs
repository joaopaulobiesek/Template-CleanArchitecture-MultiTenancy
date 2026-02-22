using Microsoft.AspNetCore.Mvc;
using Template.Api.Controllers.System;
using Template.Application.Common.Behaviours;
using Template.Application.Common.Models;
using Template.Application.Domains.Core.V1.LgpdTerms.Commands.Create;
using Template.Application.Domains.Core.V1.LgpdTerms.Commands.Update;
using Template.Application.Domains.Core.V1.LgpdTerms.Commands.Activate;
using Template.Application.Domains.Core.V1.LgpdTerms.Queries.GetAll;
using Template.Application.Domains.Core.V1.LgpdTerms.Queries.GetById;
using Template.Application.Domains.Core.V1.LgpdTerms.Queries.GetActiveTerm;
using Template.Application.Domains.Core.V1.ViewModels;

namespace Template.Api.Controllers.Core.V1.LgpdTerms;

[ApiController]
[Route("core/api/v1/[controller]")]
[ApiExplorerSettings(GroupName = "Core.Api.v1")]
public class LgpdTermController : BaseController
{
    /// <summary>
    /// Responsável por criar um novo termo LGPD.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<LgpdTermVM>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse<LgpdTermVM>))]
    public async Task<IActionResult> CreateAsync(
        [FromServices] IHandlerBase<CreateLgpdTermCommand, LgpdTermVM> handler,
        [FromBody] CreateLgpdTermCommand command,
        CancellationToken cancellationToken)
        => HandleResponse(await handler.Execute(command, cancellationToken));

    /// <summary>
    /// Responsável por atualizar o conteúdo de um termo LGPD (apenas termos não ativos).
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<LgpdTermVM>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse<LgpdTermVM>))]
    public async Task<IActionResult> UpdateAsync(
        [FromServices] IHandlerBase<UpdateLgpdTermCommand, LgpdTermVM> handler,
        [FromBody] UpdateLgpdTermCommand command,
        CancellationToken cancellationToken)
        => HandleResponse(await handler.Execute(command, cancellationToken));

    /// <summary>
    /// Responsável por ativar um termo LGPD (desativa todos os outros automaticamente).
    /// </summary>
    [HttpPatch("Activate")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<LgpdTermVM>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse<LgpdTermVM>))]
    public async Task<IActionResult> ActivateAsync(
        [FromServices] IHandlerBase<ActivateLgpdTermCommand, LgpdTermVM> handler,
        [FromBody] ActivateLgpdTermCommand command,
        CancellationToken cancellationToken)
        => HandleResponse(await handler.Execute(command, cancellationToken));

    /// <summary>
    /// Responsável por listar todos os termos LGPD com paginação.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedList<LgpdTermVM>))]
    public async Task<IActionResult> GetAllAsync(
        [FromServices] IHandlerBase<GetAllLgpdTermQuery, IEnumerable<LgpdTermVM>> handler,
        [FromQuery] GetAllLgpdTermQuery query,
        CancellationToken cancellationToken)
        => HandleResponse(await handler.Execute(query, cancellationToken));

    /// <summary>
    /// Responsável por buscar um termo LGPD pelo ID.
    /// </summary>
    [HttpGet("GetById")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<LgpdTermVM>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse<LgpdTermVM>))]
    public async Task<IActionResult> GetByIdAsync(
        [FromServices] IHandlerBase<GetByIdLgpdTermQuery, LgpdTermVM> handler,
        [FromQuery] GetByIdLgpdTermQuery query,
        CancellationToken cancellationToken)
        => HandleResponse(await handler.Execute(query, cancellationToken));

    /// <summary>
    /// Responsável por buscar o termo LGPD ativo atual.
    /// Endpoint público - não requer autenticação.
    /// </summary>
    [HttpGet("Active")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<LgpdTermVM>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse<LgpdTermVM>))]
    public async Task<IActionResult> GetActiveAsync(
        [FromServices] IHandlerBase<GetActiveLgpdTermQuery, LgpdTermVM> handler,
        CancellationToken cancellationToken)
        => HandleResponse(await handler.Execute(new GetActiveLgpdTermQuery(), cancellationToken));
}
