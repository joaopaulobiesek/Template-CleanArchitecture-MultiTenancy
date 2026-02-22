namespace Template.Application.Common.Interfaces.Services;

/// <summary>
/// Servico de envio de emails do SISTEMA via SendGrid.
/// Usa configuracao do appsettings (SendGrid:Email, SendGrid:API_KEY).
/// Usado para: confirmacao de email, notificacoes de acesso, etc.
/// NAO usa configuracao do tenant.
/// </summary>
public interface IEmailSystemService
{
    /// <summary>
    /// Envia email de confirmacao de email para o usuario.
    /// </summary>
    Task<EmailResult> SendEmailConfirmationAsync(
        EmailConfirmationNotification notification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia email de redefinicao de senha para o usuario.
    /// </summary>
    Task<EmailResult> SendPasswordResetAsync(
        PasswordResetNotification notification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia notificacao de solicitacao de demonstracao do site para o admin.
    /// </summary>
    Task<EmailResult> SendDemoRequestNotificationAsync(
        DemoRequestNotification notification,
        CancellationToken cancellationToken = default);
}
