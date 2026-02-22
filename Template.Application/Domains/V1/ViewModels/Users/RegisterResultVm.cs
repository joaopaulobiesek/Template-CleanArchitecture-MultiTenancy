namespace Template.Application.Domains.V1.ViewModels.Users;

/// <summary>
/// ViewModel de resultado do registro de usuário.
/// </summary>
public class RegisterResultVm
{
    /// <summary>
    /// E-mail do usuário registrado.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Nome completo do usuário.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Indica se o usuário precisa confirmar o e-mail.
    /// </summary>
    public bool RequiresEmailConfirmation { get; set; }

    public RegisterResultVm() { }

    public RegisterResultVm(string email, string fullName, bool requiresEmailConfirmation)
    {
        Email = email;
        FullName = fullName;
        RequiresEmailConfirmation = requiresEmailConfirmation;
    }
}
