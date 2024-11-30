namespace Template.Infra.Settings.Configurations;

public static class GetTenantConnectionConfiguration
{
    public static string GetTenantConnectionString(Guid tenantId)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .Build();

        if (tenantId == Guid.Empty)
            return string.Empty;

        var connectionString = configuration.GetConnectionString(tenantId.ToString());

        if (!string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }

        throw new Exception($"Tenant {tenantId} not found.");
    }
}