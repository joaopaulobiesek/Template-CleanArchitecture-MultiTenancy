using Template.Application.Common.Security;

namespace Template.Application.Domains.V1.Identity.Auth.Commands.Register;

/// <summary>
/// Comando de registro - auditado mesmo sem token (AllowAnonymous)
/// Não salva o body pois contém senha (SaveRequestBody = false)
/// </summary>
[Auditable("Usuário registrou-se", SaveRequestBody = false, AllowAnonymous = true)]
[BypassIpRestriction]
public class RegisterCommand
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
