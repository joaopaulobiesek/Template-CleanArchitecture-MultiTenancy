using Template.Application.Common.Models;

namespace Template.Infra.ExternalServices.AzureBlobStorage;

public interface ITenantStorageResolver
{
    void SetCurrentTenantStorage(StorageConfiguration config);
    StorageConfiguration GetCurrentTenantStorage();
}

public class TenantStorageResolver : ITenantStorageResolver
{
    private static readonly AsyncLocal<StorageConfiguration> _currentTenantStorage = new();

    public void SetCurrentTenantStorage(StorageConfiguration config)
    {
        _currentTenantStorage.Value = config;
    }

    public StorageConfiguration GetCurrentTenantStorage()
    {
        return _currentTenantStorage.Value ?? throw new Exception("Nenhuma configuração de Storage definida para o Tenant.");
    }
}