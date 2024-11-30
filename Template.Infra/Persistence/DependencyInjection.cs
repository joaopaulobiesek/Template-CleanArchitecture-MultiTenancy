using Microsoft.EntityFrameworkCore.Migrations;
using Template.Infra.Persistence.Contexts.Core;
using Template.Infra.Persistence.Contexts.Tenant;

namespace Template.Infra.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddContext(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString(nameof(TenantContext));

        services.AddDbContext<TenantContext>(options
            => options.UseSqlServer(connectionString, sqlServerOptions =>
            {
                sqlServerOptions.MigrationsHistoryTable(
                    tableName: HistoryRepository.DefaultTableName
                );
                sqlServerOptions.CommandTimeout(360);
            })
        );

        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

        services.AddScoped<ITenantDapperConnection, TenantDapper>();

        if (Environment.GetCommandLineArgs().Any(arg => arg.Contains("ef", StringComparison.OrdinalIgnoreCase)))
            services.AddDbContext<CoreContext>(options => options.UseSqlServer(config.GetConnectionString("TempMigrations")));

        services.AddScoped<ICoreContext>(provider =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<CoreContext>();
            var context = new CoreContext(optionsBuilder.Options);
            return context;
        });

        services.AddScoped<ICoreDapperConnection, CoreDapper>();

        services
            .AddDefaultIdentity<ContextUser>(o =>
            {
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequiredLength = 3;
                o.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<ContextRole>()
            .AddErrorDescriber<IdentityPortugueseMessages>()
            .AddEntityFrameworkStores<TenantContext>();

        return services;
    }
}