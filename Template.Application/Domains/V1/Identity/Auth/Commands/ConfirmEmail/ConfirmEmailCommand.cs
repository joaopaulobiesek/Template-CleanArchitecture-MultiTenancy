using Template.Application.Common.Security;

namespace Template.Application.Domains.V1.Identity.Auth.Commands.ConfirmEmail;

/// <summary>
/// Comando de confirmação de e-mail - não requer autenticação.
/// </summary>
[Auditable("Usuário confirmou e-mail", AllowAnonymous = true)]
[BypassIpRestriction]
public class ConfirmEmailCommand
{
    /// <summary>
    /// ID do usuário.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Token de confirmação de e-mail.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
