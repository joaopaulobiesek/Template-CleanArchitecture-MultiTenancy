using Microsoft.AspNetCore.Mvc;
using Template.Application.Common.Persistence;
using Template.Infra.Persistence.Contexts;
using Template.Infra.Settings.Configurations;

namespace Template.Api.Middlewares;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly HashSet<Guid> ProcessedTenants = new();

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        var endpoint = context.GetEndpoint();

        var groupName = endpoint?.Metadata.GetMetadata<ApiExplorerSettingsAttribute>()?.GroupName;

        string connectionString = string.Empty;

        Guid tenantId = Guid.Empty;

        if (context.Request.Query.TryGetValue("state", out var state) && !string.IsNullOrEmpty(state) && Guid.TryParse(state, out tenantId))
        {
            context.Items["TenantId"] = tenantId;
            connectionString = GetTenantConnectionConfiguration.GetTenantConnectionString(tenantId);
        }
        else if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantIdValue) && Guid.TryParse(tenantIdValue, out tenantId))
        {
            context.Items["TenantId"] = tenantId;
            connectionString = GetTenantConnectionConfiguration.GetTenantConnectionString(tenantId);
        }

        if (string.IsNullOrEmpty(connectionString) || tenantId == Guid.Empty)
            if (!string.IsNullOrEmpty(groupName) && groupName.Contains("Core.Api."))
                throw new Exception("The 'X-Tenant-ID' header or 'state' query parameter is required for requests to Core.API. Please ensure the tenant information is provided.");

        if (tenantId != Guid.Empty)
            if (!string.IsNullOrEmpty(groupName) && groupName.Contains("Tenant.Api."))
                throw new Exception("The 'X-Tenant-ID' header or 'state' query parameter is not allowed for requests to Tenants.API. Please remove tenant information from the request.");

        if (!string.IsNullOrEmpty(connectionString))
        {
            var dbContext = serviceProvider.GetRequiredService<ICoreContext>();
            dbContext.SetConnectionString(connectionString);

            if (!ProcessedTenants.Contains(tenantId))
            {
                await dbContext.ApplyMigrations();
                var initializer = serviceProvider.GetRequiredService<DatabaseInitializer>();
                await initializer.SeedAsync();
                ProcessedTenants.Add(tenantId);
            }
        }

        await _next(context);
    }
}