using Template.Application.Common.Interfaces.Security;
using Microsoft.AspNetCore.Http;

namespace Template.Infra.Identity;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Id => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public Guid X_Tenant_ID
    {
        get
        {
            var tenantId = _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-ID"].ToString();

            if (Guid.TryParse(tenantId, out var parsedTenantId))
            {
                return parsedTenantId;
            }

            return Guid.Empty;
        }
    }
}