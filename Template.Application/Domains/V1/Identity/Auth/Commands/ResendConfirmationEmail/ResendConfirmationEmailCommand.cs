using Template.Application.Common.Security;

namespace Template.Application.Domains.V1.Identity.Auth.Commands.ResendConfirmationEmail;

/// <summary>
/// Comando para reenvio de e-mail de confirmação - não requer autenticação.
/// </summary>
[Auditable("Usuário solicitou reenvio de e-mail de confirmação", AllowAnonymous = true)]
[BypassIpRestriction]
public class ResendConfirmationEmailCommand
{
    /// <summary>
    /// E-mail do usuário.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
