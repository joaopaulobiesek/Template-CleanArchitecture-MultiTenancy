using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Template.Application.Common.Interfaces.Services;
using Template.Infra.Persistence.Contexts;
using Template.Infra.Persistence.Contexts.Tenant;
using Template.Infra.Settings.Configurations;

namespace Template.Infra.Identity;

public class CustomInitializerIdentity
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;
    private readonly ITenantCacheService _tenantCacheService;

    public CustomInitializerIdentity(IServiceProvider serviceProvider, IConfiguration config, ITenantCacheService tenantCacheService)
    {
        _serviceProvider = serviceProvider;
        _config = config;
        _tenantCacheService = tenantCacheService;
    }

    public UserManager<ContextUser> GetUserManagerForTenant(Guid tenantId)
    {
        var connectionString = _tenantCacheService.GetConnectionStringAsync(tenantId).GetAwaiter().GetResult()
            ?? GetTenantConnectionConfiguration.GetTenantConnectionString(tenantId, _config);

        var userStore = new UserStore<ContextUser, ContextRole, BaseContext, string, ContextUserClaim, ContextUserRole, ContextUserLogin, ContextUserToken, ContextRoleClaim>(CreateTenantContext(connectionString));

        return new UserManager<ContextUser>(
            userStore,
            _serviceProvider.GetRequiredService<IOptions<IdentityOptions>>(),
            _serviceProvider.GetRequiredService<IPasswordHasher<ContextUser>>(),
            _serviceProvider.GetServices<IUserValidator<ContextUser>>(),
            _serviceProvider.GetServices<IPasswordValidator<ContextUser>>(),
            _serviceProvider.GetRequiredService<ILookupNormalizer>(),
            _serviceProvider.GetRequiredService<IdentityErrorDescriber>(),
            _serviceProvider,
            _serviceProvider.GetRequiredService<ILogger<UserManager<ContextUser>>>()
        );
    }

    public RoleManager<ContextRole> GetRoleManagerForTenant(Guid tenantId)
    {
        var connectionString = _tenantCacheService.GetConnectionStringAsync(tenantId).GetAwaiter().GetResult()
            ?? GetTenantConnectionConfiguration.GetTenantConnectionString(tenantId, _config);

        var roleStore = new RoleStore<ContextRole, TenantContext, string, ContextUserRole, ContextRoleClaim>(CreateTenantContext(connectionString));

        return new RoleManager<ContextRole>(
            roleStore,
            _serviceProvider.GetServices<IRoleValidator<ContextRole>>(),
            _serviceProvider.GetRequiredService<ILookupNormalizer>(),
            _serviceProvider.GetRequiredService<IdentityErrorDescriber>(),
            _serviceProvider.GetRequiredService<ILogger<RoleManager<ContextRole>>>()
        );
    }

    private TenantContext CreateTenantContext(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TenantContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new TenantContext(optionsBuilder.Options);
    }
}