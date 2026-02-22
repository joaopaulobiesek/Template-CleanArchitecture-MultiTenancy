namespace Template.Infra.Settings.Configurations;

public static class GetTenantConnectionConfiguration
{
    public static string GetTenantConnectionString(Guid tenantId, IConfiguration config)
    {
        if (tenantId == Guid.Empty)
            return string.Empty;

        var connectionString = config.GetConnectionString(tenantId.ToString());

        if (!string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }

        throw new Exception($"Tenant {tenantId} not found.");
    }
}