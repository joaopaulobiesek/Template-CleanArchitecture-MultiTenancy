using Template.Application.Common.Security;
using Template.Domain.Constants;
using Template.Domain.Interfaces.Core.Terms;

namespace Template.Application.Domains.Core.V1.LgpdTerms.Commands.Create;

[Authorize(Roles = $"{Roles.Admin},{Roles.TI}")]
[Authorize(Policy = Policies.CanCreate)]
public class CreateLgpdTermCommand : ILgpdTerm
{
    public string Version { get; set; } = string.Empty;
    public string TermsOfUseContent { get; set; } = string.Empty;
    public string PrivacyPolicyContent { get; set; } = string.Empty;
}
