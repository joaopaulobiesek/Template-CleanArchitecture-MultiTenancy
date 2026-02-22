namespace Template.Application.Common.Interfaces.Services;

/// <summary>
/// Serviço para gerenciar origens CORS dinamicamente.
/// Carrega URLs do appsettings + URLs dos Clients do banco de dados.
/// </summary>
public interface ICorsOriginService
{
    /// <summary>
    /// Obtém todas as origens permitidas (cache + appsettings).
    /// </summary>
    Task<IReadOnlyList<string>> GetAllowedOriginsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se uma origem é permitida.
    /// </summary>
    Task<bool> IsOriginAllowedAsync(string origin, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalida o cache de origens (chamar quando Client.Url for alterado).
    /// </summary>
    void InvalidateCache();

    /// <summary>
    /// Carrega/recarrega as origens do banco de dados para o cache.
    /// </summary>
    Task RefreshCacheAsync(CancellationToken cancellationToken = default);
}
