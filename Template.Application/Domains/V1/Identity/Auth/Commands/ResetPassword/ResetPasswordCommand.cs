using Template.Application.Common.Security;

namespace Template.Application.Domains.V1.Identity.Auth.Commands.ResetPassword;

/// <summary>
/// Comando para redefinir senha usando token - não requer autenticação.
/// </summary>
[Auditable("Usuário redefiniu senha via token", AllowAnonymous = true)]
[BypassIpRestriction]
public class ResetPasswordCommand
{
    /// <summary>
    /// ID do usuário.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Token de reset (Base64 URL-safe).
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Nova senha.
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Confirmação da nova senha.
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;
}
