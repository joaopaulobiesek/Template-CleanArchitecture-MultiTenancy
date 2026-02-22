using Template.Application.Common.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Template.Infra.ExternalServices.TenantCache;

public static class DependencyInjection
{
    public static IServiceCollection AddTenantCacheService(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<ITenantCacheService, TenantCacheService>();

        return services;
    }
}
