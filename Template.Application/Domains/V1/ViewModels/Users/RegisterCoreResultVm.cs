namespace Template.Application.Domains.V1.ViewModels.Users;

/// <summary>
/// ViewModel de resultado do registro Core (usuário + cliente).
/// </summary>
public class RegisterCoreResultVm
{
    /// <summary>
    /// Email do usuário registrado.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Nome completo do usuário/cliente.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// ID do cliente criado.
    /// </summary>
    public Guid ClientId { get; set; }

    /// <summary>
    /// Indica se o usuário precisa confirmar o email.
    /// </summary>
    public bool RequiresEmailConfirmation { get; set; }

    public RegisterCoreResultVm() { }

    public RegisterCoreResultVm(string email, string fullName, Guid clientId, bool requiresEmailConfirmation)
    {
        Email = email;
        FullName = fullName;
        ClientId = clientId;
        RequiresEmailConfirmation = requiresEmailConfirmation;
    }
}
