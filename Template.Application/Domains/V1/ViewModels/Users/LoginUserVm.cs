namespace Template.Application.Domains.V1.ViewModels.Users;

public class LoginUserVm
{
    public string Name { get; set; }
    public string Email { get; set; }
    public List<string> Modules { get; set; }
    public List<string> Roles { get; set; }
    public List<string> Policies { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public DateTime AccessTokenExpires { get; set; }
    public DateTime RefreshTokenExpires { get; set; }

    /// <summary>
    /// Indica se o e-mail do usuário ainda não foi confirmado.
    /// Usado para o frontend mostrar opção de reenvio de e-mail.
    /// </summary>
    public bool EmailNotConfirmed { get; set; }

    public LoginUserVm() { }

    public LoginUserVm(string name, string email, List<string> modules, List<string> roles, List<string> policies, string token, string refreshToken, DateTime accessTokenExpires, DateTime refreshTokenExpires)
    {
        Name = name;
        Email = email;
        Modules = modules;
        Roles = roles;
        Policies = policies;
        Token = token;
        RefreshToken = refreshToken;
        AccessTokenExpires = accessTokenExpires;
        RefreshTokenExpires = refreshTokenExpires;
    }
}
