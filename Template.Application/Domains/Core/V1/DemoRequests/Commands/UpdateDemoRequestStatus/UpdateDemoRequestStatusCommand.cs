using Template.Application.Common.Security;
using Template.Domain.Constants;
using static Template.Domain.Constants.Policies;

namespace Template.Application.Domains.Core.V1.DemoRequests.Commands.UpdateDemoRequestStatus;

/// <summary>
/// Command para atualizar status de uma solicitação de demonstração.
/// Apenas Admin do Core pode acessar.
/// </summary>
[Authorize(Roles = Roles.Admin)]
[Authorize(Policy = $"{CanEdit}")]
public class UpdateDemoRequestStatusCommand
{
    public Guid Id { get; set; }

    /// <summary>
    /// Novo status: Pending, Contacted, Converted, Rejected
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Observações do Admin
    /// </summary>
    public string? AdminNotes { get; set; }
}
