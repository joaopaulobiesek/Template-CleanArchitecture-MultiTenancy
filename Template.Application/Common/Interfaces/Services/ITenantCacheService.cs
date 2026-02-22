using Template.Application.Common.Models;

namespace Template.Application.Common.Interfaces.Services;

/// <summary>
/// Serviço de cache para resolução de tenant
/// </summary>
public interface ITenantCacheService
{
    /// <summary>
    /// Obtém a connection string do cache pelo TenantId.
    /// Se não existir no cache, busca no banco e armazena.
    /// </summary>
    Task<string?> GetConnectionStringAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém a configuração de storage do cache pelo TenantId.
    /// Se não existir no cache, busca no banco e armazena.
    /// </summary>
    Task<StorageConfiguration?> GetStorageConfigurationAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém a configuração do SendGrid do cache pelo TenantId.
    /// Se não existir no cache, busca no banco e armazena.
    /// </summary>
    Task<SendGridConfiguration?> GetSendGridConfigurationAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o TimeZoneId do cache pelo TenantId.
    /// Se não existir no cache, busca no banco e armazena.
    /// </summary>
    Task<string?> GetTimeZoneIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o TenantId do cache pela URL.
    /// Se não existir no cache, busca no banco e armazena.
    /// </summary>
    Task<Guid?> GetTenantIdByUrlAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém a URL do tenant pelo TenantId.
    /// Se não existir no cache, busca no banco e armazena.
    /// </summary>
    Task<string?> GetTenantUrlByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalida o cache para um tenant específico (connection string, storage)
    /// </summary>
    void InvalidateTenantCache(Guid tenantId);

    /// <summary>
    /// Invalida o cache de storage para um tenant específico
    /// </summary>
    void InvalidateStorageCache(Guid tenantId);

    /// <summary>
    /// Invalida o cache do SendGrid para um tenant específico
    /// </summary>
    void InvalidateSendGridCache(Guid tenantId);

    /// <summary>
    /// Invalida o cache para uma URL específica
    /// </summary>
    void InvalidateUrlCache(string url);

    /// <summary>
    /// Obtém a lista de IPs permitidos do cache pelo TenantId.
    /// Se não existir no cache, busca no banco e armazena.
    /// Retorna null se não houver restrição de IP configurada.
    /// </summary>
    Task<List<string>?> GetAllowedIpsAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalida o cache de IPs permitidos para um tenant específico
    /// </summary>
    void InvalidateAllowedIpsCache(Guid tenantId);
}
