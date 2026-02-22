using Template.Application.Common.Security;

namespace Template.Application.Domains.V1.Identity.Auth.Commands.ForgotPassword;

/// <summary>
/// Comando para solicitar reset de senha - não requer autenticação.
/// Envia e-mail com link para redefinir senha.
/// </summary>
[Auditable("Usuário solicitou recuperação de senha", AllowAnonymous = true)]
[BypassIpRestriction]
public class ForgotPasswordCommand
{
    /// <summary>
    /// E-mail do usuário.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
