using Template.Application.Common.Models;
using Template.Application.Domains.V1.ViewModels.Users;

namespace Template.Application.Common.Interfaces.Security;

public interface IIdentityService
{
    /// <summary>
    /// Login com retorno de par de tokens (Access + Refresh)
    /// </summary>
    Task<LoginTokenResult?> LoginAsync(string emailUserName, string password, Guid xTenantID);

    /// <summary>
    /// Renova os tokens usando o Refresh Token
    /// </summary>
    Task<LoginTokenResult?> RefreshTokensAsync(string userId, string refreshToken, Guid xTenantID);

    /// <summary>
    /// Revoga o Refresh Token do usuario (logout)
    /// </summary>
    Task RevokeRefreshTokenAsync(string userId);

    /// <summary>
    /// Extrai o UserId de um token JWT (mesmo expirado)
    /// </summary>
    string? ExtractUserIdFromToken(string token);

    Task<string?> GetUserNameAsync(string userId);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<bool> AuthorizeAsync(string userId, string policyName);
    Task<UserVm> CreateUserAsync(IUser user, string? password);
    Task<UserVm> EditUserAsync(IUser user, string? password);
    Task<ApiResponse<string>> DeleteUserAsync(string userId);
    IQueryable<UserVm>? ListUsersAsync(int order, string param, string? searchText = null, Dictionary<string, string>? customFilter = null);
    Task<UserVm?> GetUserByIdAsync(string userId);
    Task<List<string>> GetUserRole(string userId);
    Task<List<string>> GetUserPolicies(string userId);
    Task<ApiResponse<LoginGoogleResponse>> HandleExternalLoginAsync(string provider, string providerKey, string email, string name, string? phoneNumber, string? picture, string? state);
    Task<ApiResponse<string>> AddLoginProviderTokenAsync(string providerKey, string loginProvider, string tokenName, string tokenValue);
    Task<ApiResponse<string>> RemoveLoginProviderTokenAsync(string providerKey, string loginProvider, string tokenName);
    Task<ApiResponse<string>> GetLoginProviderTokenAsync(string userId, string loginProvider, string tokenName);
    Task<UserVm> SearchUserByEmailOrUserNameAsync(string emailUserName);
    Task<bool> AddUserToRoleAsync(string userId, string role);
    Task<bool> RemoveUserFromRoleAsync(string userId, string role);

    // Email Confirmation
    /// <summary>
    /// Verifica se o email do usuario foi confirmado.
    /// </summary>
    Task<bool> IsEmailConfirmedAsync(string userId);

    /// <summary>
    /// Gera um token de confirmacao de email.
    /// </summary>
    Task<string?> GenerateEmailConfirmationTokenAsync(string userId);

    /// <summary>
    /// Confirma o email do usuario usando o token.
    /// </summary>
    Task<ApiResponse<string>> ConfirmEmailAsync(string userId, string token);

    /// <summary>
    /// Busca usuario por email.
    /// </summary>
    Task<UserVm?> GetUserByEmailAsync(string email);

    // Password Reset
    /// <summary>
    /// Gera um token de reset de senha.
    /// </summary>
    Task<string?> GeneratePasswordResetTokenAsync(string userId);

    /// <summary>
    /// Reseta a senha do usuario usando o token.
    /// </summary>
    Task<ApiResponse<string>> ResetPasswordAsync(string userId, string token, string newPassword);

    /// <summary>
    /// Busca todos os usuarios que possuem uma determinada role.
    /// </summary>
    Task<List<UserVm>> GetUsersInRoleAsync(string roleName);

    /// <summary>
    /// Lista todos os usuarios de forma simplificada (Id, FullName, Email) ordenados por nome.
    /// </summary>
    Task<List<UserSimpleVM>> ListUsersSimpleAsync();
}