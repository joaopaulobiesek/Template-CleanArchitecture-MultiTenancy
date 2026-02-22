using Template.Application.Common.Interfaces.Services;
using Template.Application.Common.Models;
using Template.Application.Common.Persistence;
using Template.Infra.ExternalServices.AzureBlobStorage;
using Template.Infra.Persistence.Contexts;
using Microsoft.AspNetCore.Mvc;

namespace Template.Api.Middlewares;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;
    private static readonly HashSet<Guid> ProcessedTenants = new();
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public TenantMiddleware(RequestDelegate next, IConfiguration config, IServiceScopeFactory serviceScopeFactory)
    {
        _next = next;
        _config = config;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
    {
        var endpoint = context.GetEndpoint();

        var groupName = endpoint?.Metadata.GetMetadata<ApiExplorerSettingsAttribute>()?.GroupName;
        context.Items["GroupName"] = groupName;

        string? connectionString = null;
        var storageConfig = _config.GetSection($"Storage:Tenants:CoreBlob").Get<StorageConfiguration>();

        Guid tenantId = Guid.Empty;

        // Tenta obter TenantId do query param "state" (usado em OAuth callbacks)
        if (context.Request.Query.TryGetValue("state", out var state) && !string.IsNullOrEmpty(state) && Guid.TryParse(state, out tenantId))
        {
            context.Items["TenantId"] = tenantId;
            connectionString = await GetConnectionStringFromCacheAsync(tenantId);
            storageConfig = await GetStorageConfigurationFromCacheAsync(tenantId);
        }
        // Tenta obter TenantId do header "X-Tenant-ID"
        else if (context.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantIdValue) && Guid.TryParse(tenantIdValue, out tenantId))
        {
            context.Items["TenantId"] = tenantId;
            connectionString = await GetConnectionStringFromCacheAsync(tenantId);
            storageConfig = await GetStorageConfigurationFromCacheAsync(tenantId);
        }

        // Valida se Tenant.Api tem o tenant informado
        if (string.IsNullOrEmpty(connectionString) || tenantId == Guid.Empty)
            if (!string.IsNullOrEmpty(groupName) && groupName.Contains("Tenant.Api."))
                throw new Exception("The 'X-Tenant-ID' header or 'state' query parameter is required for requests to Tenant.Api. Please ensure the tenant information is provided.");

        // Valida se Core.Api NÃO tem tenant informado
        if (tenantId != Guid.Empty)
            if (!string.IsNullOrEmpty(groupName) && groupName.Contains("Core.Api."))
                throw new Exception("The 'X-Tenant-ID' header or 'state' query parameter is not allowed for requests to Core.Api. Please remove tenant information from the request.");

        // Configura storage do tenant
        if (!string.IsNullOrEmpty(storageConfig?.ConnectionString))
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var storage = scope.ServiceProvider.GetRequiredService<ITenantStorageResolver>();
            storage.SetCurrentTenantStorage(storageConfig);
        }

        // Configura conexão do banco e executa migrations se necessário
        if (!string.IsNullOrEmpty(connectionString))
        {
            var dbContext = serviceProvider.GetRequiredService<ITenantContext>();
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

    /// <summary>
    /// Obtém a connection string do cache ou do banco de dados.
    /// A connection string está armazenada em Base64 no banco e é decodificada pelo TenantCacheService.
    /// </summary>
    private async Task<string?> GetConnectionStringFromCacheAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return null;

        using var scope = _serviceScopeFactory.CreateScope();
        var tenantCacheService = scope.ServiceProvider.GetRequiredService<ITenantCacheService>();

        var connectionString = await tenantCacheService.GetConnectionStringAsync(tenantId);

        if (!string.IsNullOrEmpty(connectionString))
            return connectionString;

        // Fallback para o método antigo (appsettings.json) se não encontrar no banco
        // Isso permite migração gradual
        try
        {
            return Infra.Settings.Configurations.GetTenantConnectionConfiguration.GetTenantConnectionString(tenantId, _config);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Obtém a configuração de storage do cache ou do banco de dados.
    /// A configuração está armazenada como JSON em Base64 no banco.
    /// </summary>
    private async Task<StorageConfiguration?> GetStorageConfigurationFromCacheAsync(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            return null;

        using var scope = _serviceScopeFactory.CreateScope();
        var tenantCacheService = scope.ServiceProvider.GetRequiredService<ITenantCacheService>();

        var storageConfig = await tenantCacheService.GetStorageConfigurationAsync(tenantId);

        if (storageConfig != null)
            return storageConfig;

        // Fallback para o método antigo (appsettings.json) se não encontrar no banco
        // Isso permite migração gradual
        try
        {
            return _config.GetSection($"Storage:Tenants:{tenantId}").Get<StorageConfiguration>();
        }
        catch
        {
            return null;
        }
    }
}
