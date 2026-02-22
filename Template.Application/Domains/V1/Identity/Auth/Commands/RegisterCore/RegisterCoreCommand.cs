using Template.Domain.Interfaces.Core;

namespace Template.Application.Domains.V1.Identity.Auth.Commands.RegisterCore;

public class RegisterCoreCommand : IRegisterClient
{
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
}
