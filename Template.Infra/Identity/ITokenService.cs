namespace Template.Infra.Identity;

public interface ITokenService
{
    string GenerateJwt(ContextUser user, List<string> roles, Guid tenantId);
    string ValidateTokenGetUserId(string token, Guid tenantId);
}
