using Template.Application.Common.Security;
using Template.Domain.Constants;

namespace Template.Application.Domains.Core.V1.LgpdTerms.Commands.Update;

[Authorize(Roles = $"{Roles.Admin},{Roles.TI}")]
[Authorize(Policy = Policies.CanEdit)]
public class UpdateLgpdTermCommand
{
    public Guid Id { get; set; }

    /// <summary>
    /// Novo conteúdo do Termo de Uso. Se null, não atualiza.
    /// </summary>
    public string? TermsOfUseContent { get; set; }

    /// <summary>
    /// Novo conteúdo da Política de Privacidade. Se null, não atualiza.
    /// </summary>
    public string? PrivacyPolicyContent { get; set; }
}
