namespace Template.Application.Domains.Core.V1.ViewModels;

/// <summary>
/// Resultado da resolução de tenant
/// </summary>
public class TenantResolveResultVM
{
    /// <summary>
    /// ID do Tenant resolvido
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// De onde a URL foi obtida (Origin, Referer)
    /// </summary>
    public string ResolvedFrom { get; set; } = string.Empty;

    /// <summary>
    /// URL que foi usada para resolver o tenant
    /// </summary>
    public string ResolvedUrl { get; set; } = string.Empty;

    /// <summary>
    /// TimeZone configurado para o Tenant (ex: "E. South America Standard Time")
    /// </summary>
    public string? TimeZoneId { get; set; }

    public TenantResolveResultVM() { }

    public TenantResolveResultVM(
        Guid tenantId,
        string resolvedFrom,
        string resolvedUrl,
        string? timeZoneId = null)
    {
        TenantId = tenantId;
        ResolvedFrom = resolvedFrom;
        ResolvedUrl = resolvedUrl;
        TimeZoneId = timeZoneId;
    }
}
