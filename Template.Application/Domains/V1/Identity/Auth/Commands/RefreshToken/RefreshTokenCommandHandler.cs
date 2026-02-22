using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Models;
using Template.Application.Domains.V1.ViewModels.Users;

namespace Template.Application.Domains.V1.Identity.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : HandlerBase<RefreshTokenCommand, TokenPairVm>
{
    private readonly IIdentityService _identityService;

    public RefreshTokenCommandHandler(HandlerDependencies<RefreshTokenCommand, TokenPairVm> dependencies)
        : base(dependencies)
    {
        _identityService = dependencies.IdentityService;
    }

    protected override async Task<ApiResponse<TokenPairVm>> RunCore(
        RefreshTokenCommand request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        // Obtém tokens dos cookies via ICurrentUser
        var accessToken = _user.AuthToken;
        var refreshToken = _user.RefreshToken;

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            return new ErrorResponse<TokenPairVm>("Refresh token or access token not found.", 401);

        // Extrai UserId do token (mesmo expirado)
        var userId = _identityService.ExtractUserIdFromToken(accessToken);

        if (string.IsNullOrEmpty(userId))
            return new ErrorResponse<TokenPairVm>("Invalid access token.", 401);

        // Tenta renovar os tokens
        var result = await _identityService.RefreshTokensAsync(userId, refreshToken, _user.X_Tenant_ID);

        if (result is null)
            return new ErrorResponse<TokenPairVm>("Invalid or expired refresh token.", 401);

        var tokenPairVm = new TokenPairVm(
            result.AccessToken,
            result.RefreshToken,
            result.AccessTokenExpires,
            result.RefreshTokenExpires
        );

        return new SuccessResponse<TokenPairVm>("Tokens refreshed successfully.", tokenPairVm);
    }
}
