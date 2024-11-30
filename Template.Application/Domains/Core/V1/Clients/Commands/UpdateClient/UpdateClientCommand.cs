using Template.Application.Common.Security;
using Template.Domain.Constants;
using Template.Domain.Interfaces.Core;

namespace Template.Application.Domains.Core.V1.Clients.Commands.UpdateClient;

[Authorize(Roles = Roles.Admin)]
[Authorize(Policy = Policies.CanEdit)]
public class UpdateClientCommand : IUpdateClient
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string? ZipCode { get; set; }
}