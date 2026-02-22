using System.Linq.Expressions;
using Template.Application.Common.Extensions;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Models;
using Template.Application.Domains.V1.ViewModels.Users;
using Template.Domain.Constants;

namespace Template.Infra.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ContextUser> _userManager;
    private readonly ITokenService _tokenService;

    public IdentityService(
        UserManager<ContextUser> userManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<LoginTokenResult?> LoginAsync(string emailUserName, string password, Guid xTenantID)
    {
        var user = await SearchUserAsync(emailUserName);

        if (user is null)
            return null;

        var checkPassword = await _userManager.CheckPasswordAsync(user, password);

        if (!checkPassword)
            return null;

        // Verifica se o email foi confirmado - Retorna resultado especial para o handler diferenciar
        if (!user.EmailConfirmed)
        {
            return new LoginTokenResult
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                AccessToken = "EMAIL_NOT_CONFIRMED" // Marcador especial
            };
        }

        // Passa apenas o userId para o TokenService buscar o user com seu proprio DbContext
        var tokenPair = await _tokenService.GenerateTokenPairAsync(user.Id, xTenantID);

        return new LoginTokenResult(
            user.Id,
            user.FullName ?? string.Empty,
            user.Email ?? string.Empty,
            tokenPair.AccessToken,
            tokenPair.RefreshToken,
            tokenPair.AccessTokenExpires,
            tokenPair.RefreshTokenExpires
        );
    }

    public async Task<LoginTokenResult?> RefreshTokensAsync(string userId, string refreshToken, Guid xTenantID)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return null;

        var tokenPair = await _tokenService.RefreshTokensAsync(userId, refreshToken, xTenantID);

        if (tokenPair is null)
            return null;

        return new LoginTokenResult(
            user.Id,
            user.FullName ?? string.Empty,
            user.Email ?? string.Empty,
            tokenPair.AccessToken,
            tokenPair.RefreshToken,
            tokenPair.AccessTokenExpires,
            tokenPair.RefreshTokenExpires
        );
    }

    public async Task RevokeRefreshTokenAsync(string userId)
    {
        await _tokenService.RevokeRefreshTokenAsync(userId);
    }

    public string? ExtractUserIdFromToken(string token)
    {
        return _tokenService.ExtractUserIdFromToken(token);
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<UserVm> CreateUserAsync(IUser user, string? password)
    {
        var newUser = new ContextUser
        {
            FullName = user.FullName,
            UserName = user.Email,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ProfileImageUrl = user.ProfileImageUrl,
            BypassIp = user.BypassIp,
        };

        var result = new IdentityResult();

        if (string.IsNullOrEmpty(password))
            result = await _userManager.CreateAsync(newUser);
        else
            result = await _userManager.CreateAsync(newUser, password);

        if (!result.Succeeded)
            return new UserVm();

        newUser = await SearchUserAsync(user.Email!);

        if (newUser == null)
            return new UserVm();

        var createRoles = await _userManager.AddToRolesAsync(newUser, user.Roles!);

        if (!createRoles.Succeeded)
            return new UserVm();

        var claims = user.Policies?.Select(policy => new Claim("Permission", policy)).ToList();
        if (claims != null)
        {
            var addClaimsResult = await _userManager.AddClaimsAsync(newUser, claims);
            if (!addClaimsResult.Succeeded)
                return new UserVm();
        }

        return new UserVm(newUser.Id, newUser.Email);
    }

    public async Task<UserVm> EditUserAsync(IUser user, string? password)
    {
        var editUser = await _userManager.FindByIdAsync(user.Id!);

        if (editUser is null)
            return new UserVm();

        editUser.Update(user);

        var result = await _userManager.UpdateAsync(editUser);
        if (!result.Succeeded)
            return new UserVm();

        if (!string.IsNullOrWhiteSpace(password))
        {
            var removePasswordResult = await _userManager.RemovePasswordAsync(editUser);
            if (!removePasswordResult.Succeeded)
                return new UserVm();

            var addPasswordResult = await _userManager.AddPasswordAsync(editUser, password);
            if (!addPasswordResult.Succeeded)
                return new UserVm();
        }

        var currentRoles = await _userManager.GetRolesAsync(editUser);
        var rolesToAdd = user.Roles.Except(currentRoles).ToList();
        var rolesToRemove = currentRoles.Except(user.Roles).ToList();

        if (rolesToAdd.Any())
        {
            var addRolesResult = await _userManager.AddToRolesAsync(editUser, rolesToAdd);
            if (!addRolesResult.Succeeded)
                return new UserVm();
        }

        if (rolesToRemove.Any())
        {
            var removeRolesResult = await _userManager.RemoveFromRolesAsync(editUser, rolesToRemove);
            if (!removeRolesResult.Succeeded)
                return new UserVm();
        }

        var currentClaims = await _userManager.GetClaimsAsync(editUser);
        var currentPolicies = currentClaims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToList();

        var policiesToAdd = user.Policies.Except(currentPolicies)
            .Select(policy => new Claim("Permission", policy))
            .ToList();

        var policiesToRemove = currentClaims
            .Where(c => c.Type == "Permission" && !user.Policies.Contains(c.Value))
            .ToList();

        if (policiesToAdd.Any())
        {
            var addClaimsResult = await _userManager.AddClaimsAsync(editUser, policiesToAdd);
            if (!addClaimsResult.Succeeded)
                return new UserVm();
        }

        if (policiesToRemove.Any())
        {
            var removeClaimsResult = await _userManager.RemoveClaimsAsync(editUser, policiesToRemove);
            if (!removeClaimsResult.Succeeded)
                return new UserVm();
        }

        return new UserVm(editUser.Id, editUser.Email);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var userClaims = await _userManager.GetClaimsAsync(user);

        var hasPolicy = userClaims.Any(claim => claim.Type == "Permission" && claim.Value == policyName);

        return hasPolicy;
    }

    public async Task<ApiResponse<string>> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user != null)
        {
            var result = await DeleteUserAsync(user);

            if (result.Success)
                return new SuccessResponse<string>("User deleted successfully");
            else
                return new ErrorResponse<string>("Failed to delete user", 400, null, (result as ErrorResponse<IdentityResult>)?.Errors);
        }
        else
        {
            return new ErrorResponse<string>("User not found", 404);
        }
    }

    public async Task<ApiResponse<IdentityResult>> DeleteUserAsync(ContextUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }

    public IQueryable<UserVm>? ListUsersAsync(int order, string param, string? searchText = null, Dictionary<string, string>? customFilter = null)
    {
        IQueryable<ContextUser> userQuery = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            userQuery = userQuery.Where(x =>
            (x.FullName != null && x.FullName.ToUpper().Contains(searchText.ToUpper())) ||
            (x.Email != null && x.Email.ToUpper().Contains(searchText.ToUpper()))
            );
        }

        // Aplica custom filters COM WHITELIST na entidade ContextUser (ANTES do Select!)
        if (customFilter != null && customFilter.Any())
        {
            var filteredQuery = userQuery.ApplyCustomFiltersWithWhitelist(
                customFilter,
                "Email",
                "PhoneNumber"
            );

            if (filteredQuery != null)
                userQuery = filteredQuery;
        }

        if (order == -1)
            userQuery = userQuery.OrderByDescending(SearchOrderProperty(param!));
        else
            userQuery = userQuery.OrderBy(SearchOrderProperty(param!));

        var vmQuery = userQuery.Select(u => new UserVm(u.Id, u.Email, u.FullName, u.ProfileImageUrl, null, null, u.BypassIp));

        return vmQuery;
    }

    public async Task<UserVm?> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null) return null;

        return new UserVm
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ProfileImageUrl = user.ProfileImageUrl,
            BypassIp = user.BypassIp
        };
    }

    public async Task<List<string>> GetUserRole(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null) return new List<string>();

        var roles = await _userManager.GetRolesAsync(user);

        if (roles is null) return new List<string>();

        return roles.ToList();
    }

    public async Task<List<string>> GetUserPolicies(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null) return new List<string>();

        var claims = await _userManager.GetClaimsAsync(user);

        // Filtra apenas as claims de permission (policies) e retorna como uma lista de strings
        return claims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToList();
    }

    public async Task<ApiResponse<LoginGoogleResponse>> HandleExternalLoginAsync(string provider, string providerKey, string email, string name, string? phoneNumber, string? picture, string? state)
    {
        Guid xTenantID = Guid.Empty;
        if (!string.IsNullOrEmpty(state) && Guid.TryParse(state, out var parsedGuid))
        {
            xTenantID = parsedGuid;
        }

        var user = await _userManager.FindByLoginAsync(provider, providerKey);
        if (user != null)
        {
            var responseResult = new LoginGoogleResponse
            {
                UserId = user.Id,
                Token = await GenerateTokensAndUpdateUserAsync(user, xTenantID)
            };
            return new SuccessResponse<LoginGoogleResponse>("Login with external provider succeeded.", responseResult);
        }

        user = await SearchUserAsync(email);

        if (user == null)
        {
            var roles = new List<string> { Roles.User };
            var policies = new List<string> { Policies.CanList, Policies.CanView, Policies.CanCreate, Policies.CanEdit, Policies.CanDelete, Policies.CanViewReports };

            user = new ContextUser
            {
                UserName = email,
                Email = email,
                FullName = name,
                PhoneNumber = phoneNumber,
                ProfileImageUrl = picture,
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                return new ErrorResponse<LoginGoogleResponse>(
                    "Failed to create user.",
                    400,
                    null,
                    createResult.Errors.Select(e => new NotificationError("UserCreation", e.Description)).ToList()
                );

            user = await SearchUserAsync(email);

            var createRolesResult = await _userManager.AddToRolesAsync(user!, roles);
            if (!createRolesResult.Succeeded)
                return new ErrorResponse<LoginGoogleResponse>(
                    "Failed to assign roles to user.",
                    400,
                    null,
                    createRolesResult.Errors.Select(e => new NotificationError("RoleAssignment", e.Description)).ToList()
                );

            var claims = policies.Select(policy => new Claim("Permission", policy)).ToList();
            if (claims.Any())
            {
                var addClaimsResult = await _userManager.AddClaimsAsync(user!, claims);
                if (!addClaimsResult.Succeeded)
                    return new ErrorResponse<LoginGoogleResponse>(
                        "Failed to add claims to user.",
                        400,
                        null,
                        addClaimsResult.Errors.Select(e => new NotificationError("ClaimsAssignment", e.Description)).ToList()
                    );
            }
        }

        var addLoginResult = await _userManager.AddLoginAsync(user!, new UserLoginInfo(provider, providerKey, provider));
        if (!addLoginResult.Succeeded)
        {
            return new ErrorResponse<LoginGoogleResponse>(
                "Failed to associate external login with user.",
                400,
                null,
                addLoginResult.Errors.Select(e => new NotificationError("ExternalLogin", e.Description)).ToList()
            );
        }

        var response = new LoginGoogleResponse
        {
            UserId = user!.Id,
            Token = await GenerateTokensAndUpdateUserAsync(user, xTenantID)
        };

        return new SuccessResponse<LoginGoogleResponse>("External login successfully associated with user.", response);
    }

    public async Task<ApiResponse<string>> AddLoginProviderTokenAsync(string providerKey, string loginProvider, string tokenName, string tokenValue)
    {
        var user = await _userManager.FindByLoginAsync(loginProvider, providerKey);
        if (user == null)
            return new ErrorResponse<string>($"User with ID '{providerKey}' not found.");

        var result = await _userManager.SetAuthenticationTokenAsync(user, loginProvider, tokenName, tokenValue);
        if (!result.Succeeded)
            return new ErrorResponse<string>(
                "Failed to save authentication token.",
                400,
                null,
                result.Errors.Select(e => new NotificationError("ExternalLogin", e.Description)).ToList()
            );

        return new SuccessResponse<string>("External Login Ok!");
    }

    public async Task<ApiResponse<string>> RemoveLoginProviderTokenAsync(string providerKey, string loginProvider, string tokenName)
    {
        var user = await _userManager.FindByLoginAsync(loginProvider, providerKey);
        if (user == null)
            return new ErrorResponse<string>($"User with ID '{providerKey}' not found.");

        var result = await _userManager.RemoveAuthenticationTokenAsync(user, loginProvider, tokenName);
        if (!result.Succeeded)
            return new ErrorResponse<string>("Failed to remove authentication token.");

        return new SuccessResponse<string>("Success to remove authentication token.");
    }

    public async Task<ApiResponse<string>> GetLoginProviderTokenAsync(string userId, string loginProvider, string tokenName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new ErrorResponse<string>($"User with ID '{userId}' not found.");

        return new SuccessResponse<string>(
            "Success to get authentication token.",
            await _userManager.GetAuthenticationTokenAsync(user, loginProvider, tokenName)
        );
    }

    public async Task<UserVm> SearchUserByEmailOrUserNameAsync(string emailUserName)
    {
        var user = await SearchUserAsync(emailUserName);

        if (user == null)
            return new UserVm();

        return new UserVm(user.Id, user.Email);
    }

    private async Task<ContextUser?> SearchUserAsync(string emailUserName) =>
        emailUserName.Contains('@')
            ? await _userManager.FindByEmailAsync(emailUserName)
            : await _userManager.FindByNameAsync(emailUserName);

    private async Task<string> GenerateTokensAndUpdateUserAsync(ContextUser user, Guid xTenantID)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateJwt(user, roles.Select(r => r.ToUpper()).ToList(), xTenantID);
        await _userManager.UpdateAsync(user);
        return token;
    }

    private static Expression<Func<ContextUser, object>> SearchOrderProperty(string param)
        => param?.ToLower() switch
        {
            "email" => user => user.Email!,
            _ => user => user.FullName
        };

    public async Task<bool> AddUserToRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        var result = await _userManager.AddToRoleAsync(user, role);
        return result.Succeeded;
    }

    public async Task<bool> RemoveUserFromRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        var result = await _userManager.RemoveFromRoleAsync(user, role);
        return result.Succeeded;
    }

    // ============================================
    // EMAIL CONFIRMATION METHODS
    // ============================================

    public async Task<bool> IsEmailConfirmedAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        return user.EmailConfirmed;
    }

    public async Task<string?> GenerateEmailConfirmationTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return null;

        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<ApiResponse<string>> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new ErrorResponse<string>("Usuário não encontrado.", 404);

        if (user.EmailConfirmed)
            return new SuccessResponse<string>("E-mail já foi confirmado anteriormente.");

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new NotificationError("EmailConfirmation", e.Description)).ToList();
            return new ErrorResponse<string>("Token de confirmação inválido ou expirado.", 400, null, errors);
        }

        return new SuccessResponse<string>("Email confirmado com sucesso!");
    }

    public async Task<UserVm?> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return null;

        return new UserVm
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ProfileImageUrl = user.ProfileImageUrl
        };
    }

    // ============================================
    // PASSWORD RESET METHODS
    // ============================================

    public async Task<string?> GeneratePasswordResetTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return null;

        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<ApiResponse<string>> ResetPasswordAsync(string userId, string token, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new ErrorResponse<string>("Usuário não encontrado.", 404);

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new NotificationError("PasswordReset", e.Description)).ToList();
            return new ErrorResponse<string>("Token de redefinição inválido ou expirado.", 400, null, errors);
        }

        return new SuccessResponse<string>("Senha redefinida com sucesso!");
    }

    // ============================================
    // GET USERS BY ROLE
    // ============================================

    public async Task<List<UserVm>> GetUsersInRoleAsync(string roleName)
    {
        var users = await _userManager.GetUsersInRoleAsync(roleName);
        return users.Select(u => new UserVm
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            ProfileImageUrl = u.ProfileImageUrl
        }).ToList();
    }

    public Task<List<UserSimpleVM>> ListUsersSimpleAsync()
    {
        var users = _userManager.Users
            .OrderBy(u => u.FullName)
            .Select(u => new UserSimpleVM(u.Id, u.FullName, u.Email))
            .ToList();

        return Task.FromResult(users);
    }
}