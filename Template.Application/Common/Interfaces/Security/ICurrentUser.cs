namespace Template.Application.Common.Interfaces.Security;

public interface ICurrentUser
{
    string? Id { get; }
    Guid X_Tenant_ID { get; }
    string? GroupName { get; }

    /// <summary>
    /// Access Token do cookie HTTP-Only (auth_token)
    /// </summary>
    string? AuthToken { get; }

    /// <summary>
    /// Refresh Token do cookie HTTP-Only (refresh_token)
    /// </summary>
    string? RefreshToken { get; }

    /// <summary>
    /// Endereco IP do cliente
    /// </summary>
    string? IpAddress { get; }

    /// <summary>
    /// User-Agent do cliente (browser/app)
    /// </summary>
    string? UserAgent { get; }

    /// <summary>
    /// Metodo HTTP da requisicao (GET, POST, PUT, DELETE)
    /// </summary>
    string? HttpMethod { get; }

    /// <summary>
    /// Endpoint/rota da requisicao
    /// </summary>
    string? Endpoint { get; }

    /// <summary>
    /// Email do usuario (extraido do token)
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Nome do usuario (extraido do token)
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// JTI (JWT ID) - identificador unico do token (extraido do token)
    /// Usado como chave de criptografia para auditoria
    /// </summary>
    string? Jti { get; }

    /// <summary>
    /// Scope do token (extraido da claim "scp")
    /// Quando "full", indica bypass de verificacao de IP
    /// </summary>
    string? Scp { get; }

    /// <summary>
    /// Roles do usuario (Admin, User, TI, etc)
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// Verifica se o usuario possui uma role especifica (case-insensitive)
    /// </summary>
    bool HasRole(string role);
}