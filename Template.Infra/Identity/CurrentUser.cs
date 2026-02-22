using Microsoft.AspNetCore.Http;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Security;

namespace Template.Infra.Identity;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Id
    {
        get
        {
            var internalUserId = InternalAuthContext.GetUserId();
            if (!string.IsNullOrEmpty(internalUserId))
                return internalUserId;

            // Se não, busca do HttpContext (requisições HTTP normais)
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }

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

    public string? GroupName
    {
        get
        {
            return _httpContextAccessor.HttpContext?.Items["GroupName"]?.ToString();
        }
    }

    public string? AuthToken
    {
        get
        {
            return _httpContextAccessor.HttpContext?.Request.Cookies["auth_token"];
        }
    }

    public string? RefreshToken
    {
        get
        {
            return _httpContextAccessor.HttpContext?.Request.Cookies["refresh_token"];
        }
    }

    public string? IpAddress
    {
        get
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Em produção (Azure, proxy reverso), o IP real vem no header X-Forwarded-For
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // X-Forwarded-For pode conter múltiplos IPs: "clientIp, proxy1, proxy2"
                // O primeiro é o IP real do cliente
                var clientIp = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                return clientIp;
            }

            return context.Connection.RemoteIpAddress?.ToString();
        }
    }

    public string? UserAgent
    {
        get
        {
            return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
        }
    }

    public string? HttpMethod
    {
        get
        {
            return _httpContextAccessor.HttpContext?.Request.Method;
        }
    }

    public string? Endpoint
    {
        get
        {
            return _httpContextAccessor.HttpContext?.Request.Path.ToString();
        }
    }

    public string? Email
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
        }
    }

    public string? Name
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
        }
    }

    public string? Jti
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Jti);
        }
    }

    public string? Scp
    {
        get
        {
            // ASP.NET Core remapeia "scp" para URI completa via DefaultInboundClaimTypeMap
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue("scp")
                ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("http://schemas.microsoft.com/identity/claims/scope");
        }
    }

    public IReadOnlyList<string> Roles
    {
        get
        {
            var claims = _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role);
            if (claims == null || !claims.Any())
                return Array.Empty<string>();

            // Normaliza para Title Case (ex: "ADMIN" -> "Admin", "user" -> "User")
            return claims
                .Select(c => c.Value)
                .Where(r => !string.IsNullOrEmpty(r))
                .Select(r => char.ToUpper(r[0]) + r[1..].ToLower())
                .ToList()
                .AsReadOnly();
        }
    }

    public bool HasRole(string role)
    {
        return Roles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
    }
}