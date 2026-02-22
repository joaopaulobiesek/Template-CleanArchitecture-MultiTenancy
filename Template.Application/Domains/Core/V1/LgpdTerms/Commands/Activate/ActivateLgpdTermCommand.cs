using Template.Application.Common.Security;
using Template.Domain.Constants;

namespace Template.Application.Domains.Core.V1.LgpdTerms.Commands.Activate;

[Authorize(Roles = $"{Roles.Admin},{Roles.TI}")]
[Authorize(Policy = Policies.CanEdit)]
public class ActivateLgpdTermCommand
{
    public Guid Id { get; set; }
}
