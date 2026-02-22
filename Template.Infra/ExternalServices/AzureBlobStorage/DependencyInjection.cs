using Azure.Storage.Blobs;
using Template.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Template.Infra.ExternalServices.AzureBlobStorage;

internal static class DependencyInjection
{
    public static IServiceCollection AdicionarStorage(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<ITenantStorageResolver, TenantStorageResolver>();

        services.AddScoped<IAzureStorage>(provider =>
        {
            var tenantStorageResolver = provider.GetRequiredService<ITenantStorageResolver>();
            var storageConfig = tenantStorageResolver.GetCurrentTenantStorage();
            var logger = provider.GetRequiredService<ILogger<AzureStorage>>();

            var client = new BlobContainerClient(storageConfig.ConnectionString, storageConfig.ContainerName);
            var clientTemp = new BlobContainerClient(storageConfig.ConnectionString, storageConfig.TempContainerName);

            return new AzureStorage(client, clientTemp, logger);
        });

        return services;
    }
}