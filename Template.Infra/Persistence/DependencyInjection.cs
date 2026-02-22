using Microsoft.EntityFrameworkCore.Migrations;
using System.Runtime.InteropServices;
using Template.Infra.Persistence.Contexts.Core;
using Template.Infra.Persistence.Contexts.Tenant;

namespace Template.Infra.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddContext(this IServiceCollection services, IConfiguration config)
    {
        bool isMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        string connectionString_TempMigrations = isMac ? "TempMigrations_MAC" : "TempMigrations";
        string connectionString_CoreContext = isMac ? "CoreContext_MAC" : "CoreContext";

        var connectionString = config.GetConnectionString(connectionString_CoreContext);

        services.AddDbContext<CoreContext>(options
            => options.UseSqlServer(connectionString, sqlServerOptions =>
            {
                sqlServerOptions.MigrationsHistoryTable(
                    tableName: HistoryRepository.DefaultTableName
                );
                sqlServerOptions.CommandTimeout(360);
            })
        );

        services.AddScoped<ICoreContext>(sp => sp.GetRequiredService<CoreContext>());

        services.AddScoped<ICoreDapperConnection, CoreDapper>();

        if (Environment.GetCommandLineArgs().Any(arg => arg.Contains("ef", StringComparison.OrdinalIgnoreCase)))
            services.AddDbContext<TenantContext>(options => options.UseSqlServer(
                config.GetConnectionString(connectionString_TempMigrations),
                sqlServerOptions => sqlServerOptions.CommandTimeout(360)
            ));

        services.AddScoped<ITenantContext>(provider =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<TenantContext>();
            var context = new TenantContext(optionsBuilder.Options);
            return context;
        });

        services.AddScoped<ITenantDapperConnection, TenantDapper>();

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
            .AddEntityFrameworkStores<CoreContext>();

        return services;
    }
}