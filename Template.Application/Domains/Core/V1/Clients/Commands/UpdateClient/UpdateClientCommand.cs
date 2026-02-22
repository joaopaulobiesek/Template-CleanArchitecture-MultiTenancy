using Template.Application.Common.Security;
using Template.Domain.Constants;
using Template.Domain.Interfaces.Core;

namespace Template.Application.Domains.Core.V1.Clients.Commands.UpdateClient;

[Authorize(Roles = $"{Roles.Admin},{Roles.TI},{Roles.User}")]
[Authorize(Policy = Policies.CanEdit)]
public class UpdateClientCommand : IUpdateClient
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string? Url { get; set; }
    public string? ConnectionString { get; set; }
    public string? StorageConfiguration { get; set; }
    public string? SendGridConfiguration { get; set; }
    public string? TimeZoneId { get; set; }
    public bool Paid { get; set; }
    public string? UserId { get; set; }
    public string? AllowedIpsJson { get; set; }
}