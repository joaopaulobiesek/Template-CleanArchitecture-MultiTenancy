using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Interfaces.Services;

namespace Template.Infra.ExternalServices.SendEmails;

/// <summary>
/// Serviço de envio de email via SendGrid.
/// A API Key vem do appsettings (global).
/// O SenderEmail e SenderName vêm do cache do tenant.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ITenantCacheService _tenantCacheService;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<EmailService> _logger;
    private readonly string _apiKey;

    public EmailService(
        ITenantCacheService tenantCacheService,
        ICurrentUser currentUser,
        IConfiguration configuration,
        ILogger<EmailService> logger)
    {
        _tenantCacheService = tenantCacheService;
        _currentUser = currentUser;
        _logger = logger;
        _apiKey = configuration["SendGrid:API_KEY"] ?? string.Empty;
    }

    public async Task<EmailResult> SendEmailAsync(SendEmailRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validações básicas
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogError("SendGrid API Key não configurada no appsettings");
                return EmailResult.Fail("API Key do SendGrid não configurada.");
            }

            if (request.Recipients == null || request.Recipients.Count == 0)
            {
                return EmailResult.Fail("Nenhum destinatário informado.");
            }

            if (string.IsNullOrWhiteSpace(request.Subject))
            {
                return EmailResult.Fail("Assunto do email é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(request.HtmlContent))
            {
                return EmailResult.Fail("Conteúdo HTML do email é obrigatório.");
            }

            // Busca configuração do tenant
            var tenantId = _currentUser.X_Tenant_ID;
            if (tenantId == Guid.Empty)
            {
                return EmailResult.Fail("TenantId não disponível.");
            }

            var sendGridConfig = await _tenantCacheService.GetSendGridConfigurationAsync(tenantId, cancellationToken);
            if (sendGridConfig == null || !sendGridConfig.IsValid())
            {
                _logger.LogError("SendGridConfiguration não encontrada ou inválida para tenant {TenantId}", tenantId);
                return EmailResult.Fail("Configuração de email do tenant não encontrada ou inválida.");
            }

            // Cria cliente SendGrid
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(sendGridConfig.SenderEmail, sendGridConfig.SenderName);

            // Converte destinatários
            var recipients = request.Recipients
                .Select(r => new EmailAddress(r.Email, r.Name ?? string.Empty))
                .ToList();

            // Conteúdo texto plano
            var plainText = request.PlainTextContent ?? StripHtml(request.HtmlContent);

            // Cria mensagem
            SendGridMessage msg;
            if (recipients.Count == 1)
            {
                msg = MailHelper.CreateSingleEmail(from, recipients[0], request.Subject, plainText, request.HtmlContent);
            }
            else
            {
                msg = MailHelper.CreateSingleEmailToMultipleRecipients(
                    from,
                    recipients,
                    request.Subject,
                    plainText,
                    request.HtmlContent,
                    request.ShowAllRecipients);
            }

            // Adiciona anexos
            if (request.Attachments != null && request.Attachments.Count > 0)
            {
                foreach (var attachment in request.Attachments)
                {
                    if (string.IsNullOrWhiteSpace(attachment.ContentBase64) || string.IsNullOrWhiteSpace(attachment.FileName))
                        continue;

                    var contentType = attachment.ContentType ?? GetMimeType(attachment.FileName);
                    msg.AddAttachment(attachment.FileName, attachment.ContentBase64, contentType);
                }
            }

            // Envia
            var response = await client.SendEmailAsync(msg, cancellationToken);
            var statusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Email enviado com sucesso para {RecipientCount} destinatário(s). Subject: {Subject}, Tenant: {TenantId}",
                    recipients.Count, request.Subject, tenantId);
                return EmailResult.Ok();
            }

            var body = await response.Body.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Falha ao enviar email. StatusCode: {StatusCode}, Body: {Body}, Tenant: {TenantId}",
                statusCode, body, tenantId);
            return EmailResult.Fail($"Falha ao enviar email. StatusCode: {statusCode}", statusCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email");
            return EmailResult.Fail($"Erro ao enviar email: {ex.Message}");
        }
    }

    public async Task<EmailResult> SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlContent,
        string? plainTextContent = null,
        List<EmailAttachment>? attachments = null,
        CancellationToken cancellationToken = default)
    {
        var request = new SendEmailRequest
        {
            Recipients = new List<EmailRecipient> { new(toEmail, toName) },
            Subject = subject,
            HtmlContent = htmlContent,
            PlainTextContent = plainTextContent,
            Attachments = attachments
        };

        return await SendEmailAsync(request, cancellationToken);
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
    /// Infere o tipo MIME pelo nome do arquivo
    /// </summary>
    private static string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            ".csv" => "text/csv",
            ".zip" => "application/zip",
            ".mp4" => "video/mp4",
            _ => "application/octet-stream"
        };
    }
}
