namespace Template.Application.Domains.Tenant.V1.ViewModels.Audit;

/// <summary>
/// ViewModel com opções de filtros disponíveis para o frontend
/// </summary>
public class AuditFiltersVM
{
    /// <summary>
    /// Lista de categorias disponíveis para filtro
    /// </summary>
    public List<string> Categories { get; set; } = new();

    /// <summary>
    /// Lista de usuários disponíveis para filtro
    /// </summary>
    public List<AuditUserFilterVM> Users { get; set; } = new();
}

/// <summary>
/// Usuário disponível para filtro
/// </summary>
public class AuditUserFilterVM
{
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }

    public AuditUserFilterVM() { }

    public AuditUserFilterVM(string userId, string? userName)
    {
        UserId = userId;
        UserName = userName;
    }
}
