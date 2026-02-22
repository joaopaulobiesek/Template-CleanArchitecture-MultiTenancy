using Template.Application.Common.Interfaces.Services;
using Template.Infra.Persistence.Contexts.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Template.Infra.ExternalServices.Cors;

/// <summary>
/// Serviço de CORS dinâmico que carrega origens do appsettings + banco de dados (Client.Url).
/// Usa MemoryCache para performance e invalida quando URLs são alteradas.
/// Registrado como Singleton, usa IServiceScopeFactory para acessar o CoreContext (Scoped).
/// </summary>
public class CorsOriginService : ICorsOriginService
{
    private readonly IMemoryCache _cache;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CorsOriginService> _logger;

    private const string CacheKey = "cors-allowed-origins";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    // Lock para evitar múltiplas cargas simultâneas
    private static readonly SemaphoreSlim _loadLock = new(1, 1);

    public CorsOriginService(
        IMemoryCache cache,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        ILogger<CorsOriginService> logger)
    {
        _cache = cache;
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IReadOnlyList<string>> GetAllowedOriginsAsync(CancellationToken cancellationToken = default)
    {
        // Tenta obter do cache
        if (_cache.TryGetValue(CacheKey, out HashSet<string>? cachedOrigins) && cachedOrigins != null)
        {
            _logger.LogDebug("CORS: {Count} origens obtidas do cache", cachedOrigins.Count);
            return cachedOrigins.ToList().AsReadOnly();
        }

        // Carrega do banco + appsettings
        await RefreshCacheAsync(cancellationToken);

        // Retorna do cache
        if (_cache.TryGetValue(CacheKey, out cachedOrigins) && cachedOrigins != null)
        {
            return cachedOrigins.ToList().AsReadOnly();
        }

        return Array.Empty<string>();
    }

    public async Task<bool> IsOriginAllowedAsync(string origin, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(origin))
            return false;

        var allowedOrigins = await GetAllowedOriginsAsync(cancellationToken);

        // Normaliza a origem para comparação
        var normalizedOrigin = NormalizeOrigin(origin);

        // Verifica correspondência exata ou por host
        foreach (var allowed in allowedOrigins)
        {
            var normalizedAllowed = NormalizeOrigin(allowed);

            // Correspondência exata
            if (string.Equals(normalizedOrigin, normalizedAllowed, StringComparison.OrdinalIgnoreCase))
                return true;

            // Verifica se o host corresponde (ignora porta em dev)
            if (HostsMatch(origin, allowed))
                return true;
        }

        _logger.LogWarning("CORS: Origem não permitida: {Origin}", origin);
        return false;
    }

    public void InvalidateCache()
    {
        _cache.Remove(CacheKey);
        _logger.LogInformation("CORS: Cache de origens invalidado");
    }

    public async Task RefreshCacheAsync(CancellationToken cancellationToken = default)
    {
        await _loadLock.WaitAsync(cancellationToken);
        try
        {
            // Verifica novamente se outro thread já carregou
            if (_cache.TryGetValue(CacheKey, out HashSet<string>? _))
            {
                _logger.LogDebug("CORS: Cache já foi carregado por outro thread");
                return;
            }

            var origins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. Carrega URLs fixas do appsettings
            LoadOriginsFromSettings(origins);

            // 2. Carrega URLs dos Clients do banco de dados
            await LoadOriginsFromDatabaseAsync(origins, cancellationToken);

            // 3. Adiciona variações comuns (http/https, com/sem www, com/sem porta)
            var expandedOrigins = ExpandOrigins(origins);

            // 4. Armazena no cache
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheDuration)
                .SetPriority(CacheItemPriority.High);

            _cache.Set(CacheKey, expandedOrigins, cacheOptions);

            _logger.LogInformation("CORS: {Count} origens carregadas e cacheadas", expandedOrigins.Count);

            foreach (var origin in expandedOrigins)
            {
                _logger.LogDebug("CORS: Origem permitida: {Origin}", origin);
            }
        }
        finally
        {
            _loadLock.Release();
        }
    }

    /// <summary>
    /// Carrega origens do appsettings.json (CorsSettings:AllowedOrigins)
    /// Formato: "admin.Template.cc,Template.cc" ou "admin.Template.cc;Template.cc"
    /// </summary>
    private void LoadOriginsFromSettings(HashSet<string> origins)
    {
        // Tenta carregar do CorsSettings:AllowedOrigins
        var allowedOriginsConfig = _configuration["CorsSettings:AllowedOrigins"];

        if (!string.IsNullOrWhiteSpace(allowedOriginsConfig))
        {
            var configOrigins = allowedOriginsConfig
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(o => o.Trim())
                .Where(o => !string.IsNullOrWhiteSpace(o));

            foreach (var origin in configOrigins)
            {
                origins.Add(origin);
                _logger.LogDebug("CORS: Origem do appsettings: {Origin}", origin);
            }
        }
    }

    /// <summary>
    /// Carrega todas as URLs não-nulas dos Clients ativos do banco de dados.
    /// Usa IServiceScopeFactory para acessar o CoreContext (Scoped) a partir do serviço Singleton.
    /// </summary>
    private async Task LoadOriginsFromDatabaseAsync(HashSet<string> origins, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var coreContext = scope.ServiceProvider.GetRequiredService<CoreContext>();

            var clientUrls = await coreContext.Clients
                .Where(c => c.Active && !string.IsNullOrEmpty(c.Url))
                .Select(c => c.Url)
                .Distinct()
                .ToListAsync(cancellationToken);

            foreach (var url in clientUrls)
            {
                if (!string.IsNullOrWhiteSpace(url))
                {
                    origins.Add(url);
                    _logger.LogDebug("CORS: Origem do banco (Client.Url): {Origin}", url);
                }
            }

            _logger.LogInformation("CORS: {Count} URLs de Clients carregadas do banco", clientUrls.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CORS: Erro ao carregar URLs do banco de dados");
        }
    }

    /// <summary>
    /// Expande as origens para incluir variações (http/https, com porta, etc.)
    /// </summary>
    private HashSet<string> ExpandOrigins(HashSet<string> origins)
    {
        var expanded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var origin in origins)
        {
            // Se já tem protocolo, adiciona como está
            if (origin.StartsWith("http://") || origin.StartsWith("https://"))
            {
                expanded.Add(origin);

                // Adiciona a versão com o outro protocolo também
                if (origin.StartsWith("http://"))
                    expanded.Add(origin.Replace("http://", "https://"));
                else
                    expanded.Add(origin.Replace("https://", "http://"));
            }
            else
            {
                // Adiciona com ambos os protocolos
                expanded.Add($"http://{origin}");
                expanded.Add($"https://{origin}");
            }
        }

        return expanded;
    }

    /// <summary>
    /// Normaliza uma origem removendo trailing slash e convertendo para minúsculo.
    /// </summary>
    private static string NormalizeOrigin(string origin)
    {
        if (string.IsNullOrWhiteSpace(origin))
            return string.Empty;

        return origin.TrimEnd('/').ToLowerInvariant();
    }

    /// <summary>
    /// Verifica se os hosts de duas origens correspondem (ignora protocolo e porta).
    /// </summary>
    private static bool HostsMatch(string origin1, string origin2)
    {
        try
        {
            var host1 = ExtractHost(origin1);
            var host2 = ExtractHost(origin2);

            // Verifica correspondência direta
            if (string.Equals(host1, host2, StringComparison.OrdinalIgnoreCase))
                return true;

            // Verifica se é subdomínio (ex: bfs.localhost vs localhost)
            if (host1.EndsWith($".{host2}") || host2.EndsWith($".{host1}"))
                return true;

            // Verifica padrão wildcard (*.localhost)
            if (host2.StartsWith("*."))
            {
                var pattern = host2[2..]; // Remove "*."
                if (host1.EndsWith(pattern) || host1 == pattern)
                    return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extrai apenas o host de uma URL.
    /// </summary>
    private static string ExtractHost(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        // Remove protocolo
        var result = url
            .Replace("https://", "")
            .Replace("http://", "")
            .ToLowerInvariant();

        // Remove path
        var slashIndex = result.IndexOf('/');
        if (slashIndex > 0)
            result = result[..slashIndex];

        // Remove porta
        var colonIndex = result.IndexOf(':');
        if (colonIndex > 0)
            result = result[..colonIndex];

        return result;
    }

    private bool IsDevelopment()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase);
    }
}
