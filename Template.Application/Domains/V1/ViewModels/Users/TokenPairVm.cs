namespace Template.Application.Domains.V1.ViewModels.Users;

/// <summary>
/// ViewModel com par de tokens (Access + Refresh)
/// </summary>
public class TokenPairVm
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpires { get; set; }
    public DateTime RefreshTokenExpires { get; set; }

    public TokenPairVm() { }

    public TokenPairVm(string accessToken, string refreshToken, DateTime accessTokenExpires, DateTime refreshTokenExpires)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        AccessTokenExpires = accessTokenExpires;
        RefreshTokenExpires = refreshTokenExpires;
    }
}
