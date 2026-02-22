using Template.Application.Common.Security;

namespace Template.Application.Domains.V1.Identity.Auth.Commands.LoginUser;

/// <summary>
/// Comando de login - auditado mesmo sem token (AllowAnonymous)
/// Não salva o body pois contém senha (SaveRequestBody = false)
/// </summary>
[Auditable("Usuário logou", SaveRequestBody = false, AllowAnonymous = true)]
[BypassIpRestriction]
public class LoginUserCommand
{
    public string Email { get; set; }
    public string Password { get; set; }
}