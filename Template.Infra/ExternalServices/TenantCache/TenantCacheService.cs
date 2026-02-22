using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Interfaces.Services;
using Template.Application.Common.Models;

namespace Template.Infra.ExternalServices.TenantCache;

/// <summary>
/// Serviço de cache para resolução de tenant usando MemoryCache
/// </summary>
public class TenantCacheService : ITenantCacheService
{
    private readonly IMemoryCache _cache;
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<TenantCacheService> _logger;

    private const string ConnectionStringCachePrefix = "tenant-conn-";
    private const string StorageCachePrefix = "tenant-storage-";
    private const string SendGridCachePrefix = "tenant-sendgrid-";
    private const string TimeZoneCachePrefix = "tenant-timezone-";
    private const string UrlTenantCachePrefix = "url-tenant-";
    private const string TenantUrlCachePrefix = "tenant-url-";
    private const string AllowedIpsCachePrefix = "tenant-allowedips-";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public TenantCacheService(
        IMemoryCache cache,
        IClientRepository clientRepository,
        ILogger<TenantCacheService> logger)
    {
        _cache = cache;
        _clientRepository = clientRepository;
        _logger = logger;
    }

    public async Task<string?> GetConnectionStringAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            return null;

        var cacheKey = $"{ConnectionStringCachePrefix}{tenantId}";

        // Tenta obter do cache
        if (_cache.TryGetValue(cacheKey, out string? cachedConnectionString))
        {
            _logger.LogDebug("ConnectionString para tenant {TenantId} obtida do cache", tenantId);
            return cachedConnectionString;
        }

        // Busca no banco
        var connectionStringBase64 = await _clientRepository.GetConnectionStringByIdAsync(tenantId, cancellationToken);

        if (string.IsNullOrWhiteSpace(connectionStringBase64))
        {
            _logger.LogWarning("ConnectionString não encontrada para tenant {TenantId}", tenantId);
            return null;
        }

        // Decodifica Base64
        var connectionString = DecodeBase64(connectionStringBase64);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogError("Falha ao decodificar ConnectionString para tenant {TenantId}", tenantId);
            return null;
        }

        // Armazena no cache
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheDuration)
            .SetPriority(CacheItemPriority.High);

        _cache.Set(cacheKey, connectionString, cacheOptions);

        _logger.LogInformation("ConnectionString para tenant {TenantId} carregada e cacheada", tenantId);

        return connectionString;
    }

    public async Task<StorageConfiguration?> GetStorageConfigurationAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            return null;

        var cacheKey = $"{StorageCachePrefix}{tenantId}";

        // Tenta obter do cache
        if (_cache.TryGetValue(cacheKey, out StorageConfiguration? cachedConfig))
        {
            _logger.LogDebug("StorageConfiguration para tenant {TenantId} obtida do cache", tenantId);
            return cachedConfig;
        }

        // Busca no banco (Base64 de JSON)
        var storageConfigBase64 = await _clientRepository.GetStorageConfigurationByIdAsync(tenantId, cancellationToken);

        if (string.IsNullOrWhiteSpace(storageConfigBase64))
        {
            _logger.LogDebug("StorageConfiguration não encontrada no banco para tenant {TenantId}", tenantId);
            return null;
        }

        // Decodifica Base64 -> JSON
        var json = DecodeBase64(storageConfigBase64);

        if (string.IsNullOrWhiteSpace(json))
        {
            _logger.LogError("Falha ao decodificar StorageConfiguration para tenant {TenantId}", tenantId);
            return null;
        }

        // Deserializa JSON -> StorageConfiguration
        StorageConfiguration? storageConfig;
        try
        {
            storageConfig = JsonSerializer.Deserialize<StorageConfiguration>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Falha ao deserializar StorageConfiguration para tenant {TenantId}", tenantId);
            return null;
        }

        if (storageConfig == null)
        {
            _logger.LogError("StorageConfiguration deserializada é nula para tenant {TenantId}", tenantId);
            return null;
        }

        // Armazena no cache
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheDuration)
            .SetPriority(CacheItemPriority.High);

        _cache.Set(cacheKey, storageConfig, cacheOptions);

        _logger.LogInformation("StorageConfiguration para tenant {TenantId} carregada e cacheada", tenantId);

        return storageConfig;
    }

    public async Task<SendGridConfiguration?> GetSendGridConfigurationAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            return null;

        var cacheKey = $"{SendGridCachePrefix}{tenantId}";

        // Tenta obter do cache
        if (_cache.TryGetValue(cacheKey, out SendGridConfiguration? cachedConfig))
        {
            _logger.LogDebug("SendGridConfiguration para tenant {TenantId} obtida do cache", tenantId);
            return cachedConfig;
        }

        // Busca no banco (Base64 de JSON)
        var sendGridConfigBase64 = await _clientRepository.GetSendGridConfigurationByIdAsync(tenantId, cancellationToken);

        if (string.IsNullOrWhiteSpace(sendGridConfigBase64))
        {
            _logger.LogDebug("SendGridConfiguration não encontrada no banco para tenant {TenantId}", tenantId);
            return null;
        }

        // Decodifica Base64 -> JSON
        var json = DecodeBase64(sendGridConfigBase64);

        if (string.IsNullOrWhiteSpace(json))
        {
            _logger.LogError("Falha ao decodificar SendGridConfiguration para tenant {TenantId}", tenantId);
            return null;
        }

        // Deserializa JSON -> SendGridConfiguration
        SendGridConfiguration? sendGridConfig;
        try
        {
            sendGridConfig = JsonSerializer.Deserialize<SendGridConfiguration>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Falha ao deserializar SendGridConfiguration para tenant {TenantId}", tenantId);
            return null;
        }

        if (sendGridConfig == null)
        {
            _logger.LogError("SendGridConfiguration deserializada é nula para tenant {TenantId}", tenantId);
            return null;
        }

        // Armazena no cache
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheDuration)
            .SetPriority(CacheItemPriority.High);

        _cache.Set(cacheKey, sendGridConfig, cacheOptions);

        _logger.LogInformation("SendGridConfiguration para tenant {TenantId} carregada e cacheada", tenantId);

        return sendGridConfig;
    }

    public async Task<string?> GetTimeZoneIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            return null;

        var cacheKey = $"{TimeZoneCachePrefix}{tenantId}";

        // Tenta obter do cache
        if (_cache.TryGetValue(cacheKey, out string? cachedTimeZoneId))
        {
            _logger.LogDebug("TimeZoneId para tenant {TenantId} obtido do cache", tenantId);
            return cachedTimeZoneId;
        }

        // Busca no banco
        var timeZoneId = await _clientRepository.GetTimeZoneIdByIdAsync(tenantId, cancellationToken);

        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            _logger.LogDebug("TimeZoneId não encontrado para tenant {TenantId}, usando UTC como padrão", tenantId);
            return "UTC"; // Fallback para UTC se não tiver configurado
        }

        // Armazena no cache
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheDuration)
            .SetPriority(CacheItemPriority.High);

        _cache.Set(cacheKey, timeZoneId, cacheOptions);

        _logger.LogInformation("TimeZoneId {TimeZoneId} para tenant {TenantId} carregado e cacheado", timeZoneId, tenantId);

        return timeZoneId;
    }

    public async Task<Guid?> GetTenantIdByUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        var host = NormalizeUrl(url);

        if (string.IsNullOrWhiteSpace(host))
            return null;

        var cacheKey = $"{UrlTenantCachePrefix}{host}";

        // Tenta obter do cache
        if (_cache.TryGetValue(cacheKey, out Guid cachedTenantId))
        {
            _logger.LogDebug("TenantId para URL {Url} obtido do cache: {TenantId}", url, cachedTenantId);
            return cachedTenantId;
        }

        // Busca no banco (passa o host já normalizado)
        var client = await _clientRepository.GetByUrlAsync(host, cancellationToken);

        if (client == null)
        {
            _logger.LogWarning("Tenant não encontrado para URL {Url}", url);
            return null;
        }

        // Armazena no cache
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheDuration)
            .SetPriority(CacheItemPriority.High);

        _cache.Set(cacheKey, client.Id, cacheOptions);

        _logger.LogInformation("TenantId {TenantId} para URL {Url} carregado e cacheado", client.Id, url);

        return client.Id;
    }

    public async Task<string?> GetTenantUrlByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            return null;

        var cacheKey = $"{TenantUrlCachePrefix}{tenantId}";

        // Tenta obter do cache
        if (_cache.TryGetValue(cacheKey, out string? cachedUrl))
        {
            _logger.LogDebug("URL para tenant {TenantId} obtida do cache: {Url}", tenantId, cachedUrl);
            return cachedUrl;
        }

        // Busca no banco
        var url = await _clientRepository.GetUrlByIdAsync(tenantId, cancellationToken);

        if (string.IsNullOrWhiteSpace(url))
        {
            _logger.LogWarning("URL não encontrada para tenant {TenantId}", tenantId);
            return null;
        }

        // Armazena no cache
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheDuration)
            .SetPriority(CacheItemPriority.High);

        _cache.Set(cacheKey, url, cacheOptions);

        _logger.LogInformation("URL {Url} para tenant {TenantId} carregada e cacheada", url, tenantId);

        return url;
    }

    public void InvalidateTenantCache(Guid tenantId)
    {
        _cache.Remove($"{ConnectionStringCachePrefix}{tenantId}");
        _cache.Remove($"{StorageCachePrefix}{tenantId}");
        _cache.Remove($"{SendGridCachePrefix}{tenantId}");
        _cache.Remove($"{TimeZoneCachePrefix}{tenantId}");
        _cache.Remove($"{TenantUrlCachePrefix}{tenantId}");
        _cache.Remove($"{AllowedIpsCachePrefix}{tenantId}");
        _logger.LogInformation("Cache completo invalidado para tenant {TenantId}", tenantId);
    }

    public void InvalidateStorageCache(Guid tenantId)
    {
        var cacheKey = $"{StorageCachePrefix}{tenantId}";
        _cache.Remove(cacheKey);
        _logger.LogInformation("Cache de StorageConfiguration invalidado para tenant {TenantId}", tenantId);
    }

    public void InvalidateSendGridCache(Guid tenantId)
    {
        var cacheKey = $"{SendGridCachePrefix}{tenantId}";
        _cache.Remove(cacheKey);
        _logger.LogInformation("Cache de SendGridConfiguration invalidado para tenant {TenantId}", tenantId);
    }

    public void InvalidateUrlCache(string url)
    {
        var normalizedUrl = NormalizeUrl(url);
        var cacheKey = $"{UrlTenantCachePrefix}{normalizedUrl}";
        _cache.Remove(cacheKey);
        _logger.LogInformation("Cache de URL invalidado para {Url}", url);
    }

    public async Task<List<string>?> GetAllowedIpsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
            return null;

        var cacheKey = $"{AllowedIpsCachePrefix}{tenantId}";

        if (_cache.TryGetValue(cacheKey, out List<string>? cachedIps))
        {
            _logger.LogDebug("AllowedIps para tenant {TenantId} obtida do cache", tenantId);
            return cachedIps;
        }

        var allowedIpsBase64 = await _clientRepository.GetAllowedIpsByIdAsync(tenantId, cancellationToken);

        if (string.IsNullOrWhiteSpace(allowedIpsBase64))
        {
            _cache.Set(cacheKey, (List<string>?)null, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration,
                Priority = CacheItemPriority.High
            });
            return null;
        }

        var json = DecodeBase64(allowedIpsBase64);
        var ips = JsonSerializer.Deserialize<List<string>>(json);

        _cache.Set(cacheKey, ips, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration,
            Priority = CacheItemPriority.High
        });

        _logger.LogDebug("AllowedIps para tenant {TenantId} carregada do banco e cacheada ({Count} IPs)", tenantId, ips?.Count ?? 0);
        return ips;
    }

    public void InvalidateAllowedIpsCache(Guid tenantId)
    {
        var cacheKey = $"{AllowedIpsCachePrefix}{tenantId}";
        _cache.Remove(cacheKey);
        _logger.LogInformation("Cache de AllowedIps invalidado para tenant {TenantId}", tenantId);
    }

    /// <summary>
    /// Decodifica uma string Base64 para string normal
    /// </summary>
    private static string? DecodeBase64(string base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String))
            return null;

        try
        {
            var bytes = Convert.FromBase64String(base64String);
            return Encoding.UTF8.GetString(bytes);
        }
        catch (FormatException)
        {
            // Se não for Base64 válido, retorna a string original (pode já estar decodificada)
            return base64String;
        }
    }

    /// <summary>
    /// Extrai apenas o host (domínio) de uma URL completa para uso como chave de cache.
    /// Exemplos:
    /// - "https://bfs.Template.cc/pt-br/auth/login" -> "bfs.Template.cc"
    /// - "http://localhost:4200/pt-br/auth/login" -> "localhost"
    /// - "https://www.feira.com.br/" -> "feira.com.br"
    /// </summary>
    private static string NormalizeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        try
        {
            // Tenta parsear como URI
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                var host = uri.Host.ToLowerInvariant();

                // Remove www. se presente
                if (host.StartsWith("www."))
                    host = host[4..];

                return host;
            }

            // Fallback: trata como string se não for URI válida
            var result = url.ToLowerInvariant();

            // Remove protocolo
            result = result.Replace("https://", "").Replace("http://", "");

            // Remove www.
            if (result.StartsWith("www."))
                result = result[4..];

            // Remove path (tudo após a primeira /)
            var slashIndex = result.IndexOf('/');
            if (slashIndex > 0)
                result = result[..slashIndex];

            // Remove porta
            var colonIndex = result.IndexOf(':');
            if (colonIndex > 0)
                result = result[..colonIndex];

            return result.TrimEnd('/');
        }
        catch
        {
            return string.Empty;
        }
    }
}
