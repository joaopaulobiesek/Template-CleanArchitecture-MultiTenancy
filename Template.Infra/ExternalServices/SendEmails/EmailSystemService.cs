using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using Template.Application.Common.Interfaces.Services;
using Template.Infra.ExternalServices.SendEmails.EmailTemplates;

namespace Template.Infra.ExternalServices.SendEmails;

/// <summary>
/// Serviço de envio de emails do SISTEMA via SendGrid.
/// Usa configuração do appsettings (SendGrid:Email, SendGrid:API_KEY).
/// Usado para: confirmação de email, notificações de acesso, etc.
/// NÃO usa configuração do tenant - sempre envia como contato@wiesoo.com
/// </summary>
public class EmailSystemService : IEmailSystemService
{
    private readonly ILogger<EmailSystemService> _logger;
    private readonly string _apiKey;
    private readonly string _senderEmail;
    private readonly string _senderName;

    public EmailSystemService(
        IConfiguration configuration,
        ILogger<EmailSystemService> logger)
    {
        _logger = logger;
        _apiKey = configuration["SendGrid:API_KEY"] ?? string.Empty;
        _senderEmail = configuration["SendGrid:Email"] ?? "contato@wiesoo.com";
        _senderName = configuration["SendGrid:SenderName"] ?? "Template";
    }

    /// <summary>
    /// Envia email do sistema usando configuração do appsettings.
    /// </summary>
    private async Task<EmailResult> SendEmailAsync(
        List<EmailRecipient> recipients,
        string subject,
        string htmlContent,
        bool showAllRecipients = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validações básicas
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogError("[SYSTEM EMAIL] SendGrid API Key não configurada no appsettings");
                return EmailResult.Fail("API Key do SendGrid não configurada.");
            }

            if (recipients == null || recipients.Count == 0)
            {
                return EmailResult.Fail("Nenhum destinatário informado.");
            }

            if (string.IsNullOrWhiteSpace(subject))
            {
                return EmailResult.Fail("Assunto do email é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                return EmailResult.Fail("Conteúdo HTML do email é obrigatório.");
            }

            // Cria cliente SendGrid usando configuração do SISTEMA (appsettings)
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_senderEmail, _senderName);

            // Converte destinatários
            var toRecipients = recipients
                .Select(r => new EmailAddress(r.Email, r.Name ?? string.Empty))
                .ToList();

            // Conteúdo texto
            var plainText = StripHtml(htmlContent);

            // Cria mensagem
            SendGridMessage msg;
            if (toRecipients.Count == 1)
            {
                msg = MailHelper.CreateSingleEmail(from, toRecipients[0], subject, plainText, htmlContent);
            }
            else
            {
                msg = MailHelper.CreateSingleEmailToMultipleRecipients(
                    from,
                    toRecipients,
                    subject,
                    plainText,
                    htmlContent,
                    showAllRecipients);
            }

            // Envia
            var response = await client.SendEmailAsync(msg, cancellationToken);
            var statusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "[SYSTEM EMAIL] Email enviado com sucesso para {RecipientCount} destinatário(s). Subject: {Subject}, From: {From}",
                    toRecipients.Count, subject, _senderEmail);
                return EmailResult.Ok();
            }

            var body = await response.Body.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "[SYSTEM EMAIL] Falha ao enviar email. StatusCode: {StatusCode}, Body: {Body}, From: {From}",
                statusCode, body, _senderEmail);
            return EmailResult.Fail($"Falha ao enviar email. StatusCode: {statusCode}", statusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SYSTEM EMAIL] Erro ao enviar email");
            return EmailResult.Fail($"Erro ao enviar email: {ex.Message}");
        }
    }

    /// <summary>
    /// Remove tags HTML e retorna texto puro (fallback simples)
    /// </summary>
    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        // Remove tags HTML
        var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", string.Empty);
        // Decodifica entidades HTML comuns
        text = System.Net.WebUtility.HtmlDecode(text);
        // Remove espaços extras
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
        return text;
    }

    /// <summary>
    /// Envia email de confirmação de email para o usuário.
    /// </summary>
    public async Task<EmailResult> SendEmailConfirmationAsync(
        EmailConfirmationNotification notification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validações básicas
            if (string.IsNullOrWhiteSpace(notification.UserEmail))
            {
                _logger.LogWarning("[SYSTEM EMAIL] Email do usuário não informado para confirmação de email");
                return EmailResult.Fail("Email do usuário é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(notification.UserName))
            {
                _logger.LogWarning("[SYSTEM EMAIL] Nome do usuário não informado para confirmação de email");
                return EmailResult.Fail("Nome do usuário é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(notification.ConfirmationUrl))
            {
                _logger.LogWarning("[SYSTEM EMAIL] URL de confirmação não informada");
                return EmailResult.Fail("URL de confirmação é obrigatória.");
            }

            _logger.LogInformation(
                "[SYSTEM EMAIL] Enviando email de confirmação para {UserName} ({Email})",
                notification.UserName, notification.UserEmail);

            // Gera o HTML do email usando o template
            var htmlContent = EmailConfirmationTemplate.Generate(
                notification.UserName,
                notification.ConfirmationUrl,
                notification.RequestedAt);

            var subject = EmailConfirmationTemplate.GetSubject();

            return await SendEmailAsync(
                new List<EmailRecipient> { new(notification.UserEmail, notification.UserName) },
                subject,
                htmlContent,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SYSTEM EMAIL] Erro ao enviar email de confirmação para {UserName}", notification.UserName);
            return EmailResult.Fail($"Erro ao enviar email de confirmação: {ex.Message}");
        }
    }

    /// <summary>
    /// Envia email de redefinição de senha para o usuário.
    /// </summary>
    public async Task<EmailResult> SendPasswordResetAsync(
        PasswordResetNotification notification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validações básicas
            if (string.IsNullOrWhiteSpace(notification.UserEmail))
            {
                _logger.LogWarning("[SYSTEM EMAIL] Email do usuário não informado para redefinição de senha");
                return EmailResult.Fail("Email do usuário é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(notification.UserName))
            {
                _logger.LogWarning("[SYSTEM EMAIL] Nome do usuário não informado para redefinição de senha");
                return EmailResult.Fail("Nome do usuário é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(notification.ResetUrl))
            {
                _logger.LogWarning("[SYSTEM EMAIL] URL de redefinição não informada");
                return EmailResult.Fail("URL de redefinição é obrigatória.");
            }

            _logger.LogInformation(
                "[SYSTEM EMAIL] Enviando email de redefinição de senha para {UserName} ({Email})",
                notification.UserName, notification.UserEmail);

            // Gera o HTML do email usando o template
            var htmlContent = PasswordResetTemplate.Generate(
                notification.UserName,
                notification.ResetUrl,
                notification.RequestedAt);

            var subject = PasswordResetTemplate.GetSubject();

            return await SendEmailAsync(
                new List<EmailRecipient> { new(notification.UserEmail, notification.UserName) },
                subject,
                htmlContent,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SYSTEM EMAIL] Erro ao enviar email de redefinição de senha para {UserName}", notification.UserName);
            return EmailResult.Fail($"Erro ao enviar email de redefinição de senha: {ex.Message}");
        }
    }

    /// <summary>
    /// Envia notificação de solicitação de demonstração do site para o admin.
    /// </summary>
    public async Task<EmailResult> SendDemoRequestNotificationAsync(
        DemoRequestNotification notification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validações básicas
            if (string.IsNullOrWhiteSpace(notification.FullName))
            {
                _logger.LogWarning("[SYSTEM EMAIL] Nome do solicitante não informado para notificação de demo request");
                return EmailResult.Fail("Nome do solicitante é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(notification.Email))
            {
                _logger.LogWarning("[SYSTEM EMAIL] Email do solicitante não informado para notificação de demo request");
                return EmailResult.Fail("Email do solicitante é obrigatório.");
            }

            // Email fixo do admin para receber as notificações
            const string adminEmail = "joaopaulobiesek@gmail.com";
            const string adminName = "Admin Template";

            _logger.LogInformation(
                "[SYSTEM EMAIL] Enviando notificação de demo request para admin. Solicitante: {Name} ({Email})",
                notification.FullName, notification.Email);

            // Gera o HTML do email usando o template
            var htmlContent = EmailTemplates.DemoRequestNotificationTemplate.Generate(
                notification.FullName,
                notification.Email,
                notification.Phone,
                notification.CompanyName,
                notification.EventType,
                notification.EstimatedAudience,
                notification.Message,
                notification.RequestedAt);

            var subject = EmailTemplates.DemoRequestNotificationTemplate.GetSubject();

            return await SendEmailAsync(
                new List<EmailRecipient> { new(adminEmail, adminName) },
                subject,
                htmlContent,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SYSTEM EMAIL] Erro ao enviar notificação de demo request. Solicitante: {Name}", notification.FullName);
            return EmailResult.Fail($"Erro ao enviar notificação: {ex.Message}");
        }
    }
}
