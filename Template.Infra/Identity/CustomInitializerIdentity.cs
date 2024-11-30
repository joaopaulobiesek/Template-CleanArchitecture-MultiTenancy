using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Template.Infra.Persistence.Contexts;
using Template.Infra.Persistence.Contexts.Core;
using Template.Infra.Settings.Configurations;

namespace Template.Infra.Identity;

public class CustomInitializerIdentity
{
    private readonly IServiceProvider _serviceProvider;

    public CustomInitializerIdentity(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public UserManager<ContextUser> GetUserManagerForTenant(Guid tenantId)
    {
        var connectionString = GetTenantConnectionConfiguration.GetTenantConnectionString(tenantId);

        var userStore = new UserStore<ContextUser, ContextRole, BaseContext, string, ContextUserClaim, ContextUserRole, ContextUserLogin, ContextUserToken,ContextRoleClaim>(CreateCoreContext(connectionString));

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
        var connectionString = GetTenantConnectionConfiguration.GetTenantConnectionString(tenantId);

        var roleStore = new RoleStore<ContextRole, CoreContext, string, ContextUserRole, ContextRoleClaim>(CreateCoreContext(connectionString));

        return new RoleManager<ContextRole>(
            roleStore,
            _serviceProvider.GetServices<IRoleValidator<ContextRole>>(),
            _serviceProvider.GetRequiredService<ILookupNormalizer>(),
            _serviceProvider.GetRequiredService<IdentityErrorDescriber>(),
            _serviceProvider.GetRequiredService<ILogger<RoleManager<ContextRole>>>()
        );
    }

    private CoreContext CreateCoreContext(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CoreContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new CoreContext(optionsBuilder.Options);
    }
}