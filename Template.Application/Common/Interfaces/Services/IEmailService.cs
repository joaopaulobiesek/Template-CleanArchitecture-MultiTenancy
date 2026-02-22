namespace Template.Application.Common.Interfaces.Services;

/// <summary>
/// Servico de envio de emails via SendGrid.
/// Suporta multiplos destinatarios, anexos e conteudo HTML.
/// Usa configuracao do tenant (SenderEmail, SenderName) do cache.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envia um email para um ou mais destinatarios.
    /// </summary>
    /// <param name="request">Dados do email a ser enviado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado do envio</returns>
    Task<EmailResult> SendEmailAsync(SendEmailRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia um email para um unico destinatario (atalho).
    /// </summary>
    Task<EmailResult> SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlContent,
        string? plainTextContent = null,
        List<EmailAttachment>? attachments = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Request para envio de email
/// </summary>
public class SendEmailRequest
{
    /// <summary>
    /// Lista de destinatarios (obrigatorio pelo menos 1)
    /// </summary>
    public List<EmailRecipient> Recipients { get; set; } = new();

    /// <summary>
    /// Assunto do email
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Conteudo HTML do email
    /// </summary>
    public string HtmlContent { get; set; } = string.Empty;

    /// <summary>
    /// Conteudo texto puro (fallback para clientes sem suporte HTML)
    /// </summary>
    public string? PlainTextContent { get; set; }

    /// <summary>
    /// Lista de anexos (opcional)
    /// </summary>
    public List<EmailAttachment>? Attachments { get; set; }

    /// <summary>
    /// Se true, mostra todos os destinatarios no email (CC visivel).
    /// Se false, cada destinatario ve apenas seu email.
    /// </summary>
    public bool ShowAllRecipients { get; set; } = false;
}

/// <summary>
/// Destinatario de email
/// </summary>
public class EmailRecipient
{
    /// <summary>
    /// Email do destinatario
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Nome do destinatario (opcional)
    /// </summary>
    public string? Name { get; set; }

    public EmailRecipient() { }

    public EmailRecipient(string email, string? name = null)
    {
        Email = email;
        Name = name;
    }
}

/// <summary>
/// Anexo de email
/// </summary>
public class EmailAttachment
{
    /// <summary>
    /// Nome do arquivo (ex: "contrato.pdf")
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Conteudo do arquivo em Base64
    /// </summary>
    public string ContentBase64 { get; set; } = string.Empty;

    /// <summary>
    /// Tipo MIME do arquivo (ex: "application/pdf", "image/png")
    /// Se nulo, sera inferido do nome do arquivo.
    /// </summary>
    public string? ContentType { get; set; }

    public EmailAttachment() { }

    public EmailAttachment(string fileName, string contentBase64, string? contentType = null)
    {
        FileName = fileName;
        ContentBase64 = contentBase64;
        ContentType = contentType;
    }
}

/// <summary>
/// Resultado do envio de email
/// </summary>
public class EmailResult
{
    /// <summary>
    /// Se o envio foi bem sucedido
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensagem de erro (se falhou)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Status code HTTP retornado pelo SendGrid
    /// </summary>
    public int? StatusCode { get; set; }

    public static EmailResult Ok() => new() { Success = true };

    public static EmailResult Fail(string message, int? statusCode = null) => new()
    {
        Success = false,
        ErrorMessage = message,
        StatusCode = statusCode
    };
}

/// <summary>
/// Dados para notificacao de solicitacao de acesso
/// </summary>
public class AccessRequestNotification
{
    /// <summary>
    /// Nome da empresa (TradeName ou LegalName)
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Email da empresa
    /// </summary>
    public string CompanyEmail { get; set; } = string.Empty;

    /// <summary>
    /// Lista de emails dos representantes
    /// </summary>
    public List<RepresentativeEmail> Representatives { get; set; } = new();

    /// <summary>
    /// Nome de quem solicitou acesso (email NAO e enviado por LGPD)
    /// </summary>
    public string RequesterName { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de stakeholder (exhibitor, service_provider)
    /// </summary>
    public string StakeholderType { get; set; } = string.Empty;

    /// <summary>
    /// Mensagem enviada pelo solicitante (opcional)
    /// </summary>
    public string? RequestMessage { get; set; }

    /// <summary>
    /// Data/hora da solicitacao
    /// </summary>
    public DateTime RequestedAt { get; set; }

    /// <summary>
    /// URL do sistema para acessar
    /// </summary>
    public string SystemUrl { get; set; } = string.Empty;
}

/// <summary>
/// Email de representante da empresa
/// </summary>
public class RepresentativeEmail
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public RepresentativeEmail() { }

    public RepresentativeEmail(string email, string name)
    {
        Email = email;
        Name = name;
    }
}

/// <summary>
/// Dados para notificacao de acesso APROVADO
/// </summary>
public class AccessApprovedNotification
{
    /// <summary>
    /// Email do solicitante
    /// </summary>
    public string RequesterEmail { get; set; } = string.Empty;

    /// <summary>
    /// Nome do solicitante
    /// </summary>
    public string RequesterName { get; set; } = string.Empty;

    /// <summary>
    /// Nome da empresa
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de stakeholder (exhibitor, service_provider)
    /// </summary>
    public string StakeholderType { get; set; } = string.Empty;

    /// <summary>
    /// Data/hora da aprovacao
    /// </summary>
    public DateTime ApprovedAt { get; set; }

    /// <summary>
    /// URL do sistema para acessar
    /// </summary>
    public string SystemUrl { get; set; } = string.Empty;
}

/// <summary>
/// Dados para notificacao de acesso REJEITADO
/// </summary>
public class AccessRejectedNotification
{
    /// <summary>
    /// Email do solicitante
    /// </summary>
    public string RequesterEmail { get; set; } = string.Empty;

    /// <summary>
    /// Nome do solicitante
    /// </summary>
    public string RequesterName { get; set; } = string.Empty;

    /// <summary>
    /// Nome da empresa
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de stakeholder (exhibitor, service_provider)
    /// </summary>
    public string StakeholderType { get; set; } = string.Empty;

    /// <summary>
    /// Motivo da rejeicao (opcional)
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Data/hora da rejeicao
    /// </summary>
    public DateTime RejectedAt { get; set; }

    /// <summary>
    /// URL do sistema para acessar
    /// </summary>
    public string SystemUrl { get; set; } = string.Empty;
}

/// <summary>
/// Dados para email de confirmacao de email
/// </summary>
public class EmailConfirmationNotification
{
    /// <summary>
    /// Email do usuario
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// Nome do usuario
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// URL completa de confirmacao (com token)
    /// </summary>
    public string ConfirmationUrl { get; set; } = string.Empty;

    /// <summary>
    /// Data/hora da solicitacao
    /// </summary>
    public DateTime RequestedAt { get; set; }
}

/// <summary>
/// Dados para email de redefinicao de senha
/// </summary>
public class PasswordResetNotification
{
    /// <summary>
    /// Email do usuario
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// Nome do usuario
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// URL completa de redefinicao (com token)
    /// </summary>
    public string ResetUrl { get; set; } = string.Empty;

    /// <summary>
    /// Data/hora da solicitacao
    /// </summary>
    public DateTime RequestedAt { get; set; }
}

/// <summary>
/// Dados para notificacao de solicitacao de demonstracao do site
/// </summary>
public class DemoRequestNotification
{
    /// <summary>
    /// Nome completo do solicitante
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email do solicitante
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Telefone do solicitante
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Nome da empresa/evento (opcional)
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Tipo de evento (opcional)
    /// </summary>
    public string? EventType { get; set; }

    /// <summary>
    /// Estimativa de publico (opcional)
    /// </summary>
    public string? EstimatedAudience { get; set; }

    /// <summary>
    /// Mensagem do solicitante (opcional)
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Data/hora da solicitacao
    /// </summary>
    public DateTime RequestedAt { get; set; }
}

/// <summary>
/// Dados para notificacao de contrato aguardando analise juridica
/// </summary>
public class ContractLegalReviewNotification
{
    /// <summary>
    /// Numero do contrato
    /// </summary>
    public string ContractNumber { get; set; } = string.Empty;

    /// <summary>
    /// Nome do expositor
    /// </summary>
    public string ExhibitorName { get; set; } = string.Empty;

    /// <summary>
    /// Nome do evento/expo
    /// </summary>
    public string ExpoName { get; set; } = string.Empty;

    /// <summary>
    /// Lista de emails dos usuarios do departamento juridico
    /// </summary>
    public List<EmailRecipient> LegalDepartmentUsers { get; set; } = new();

    /// <summary>
    /// Data/hora do envio para analise
    /// </summary>
    public DateTime SubmittedAt { get; set; }

    /// <summary>
    /// URL do sistema para acessar
    /// </summary>
    public string SystemUrl { get; set; } = string.Empty;

    /// <summary>
    /// Observação/mensagem enviada pelo expositor (opcional)
    /// </summary>
    public string? ExhibitorObservation { get; set; }
}

/// <summary>
/// Dados para notificação de contrato assinado digitalmente.
/// Enviado para todos os usuários do departamento jurídico quando todas as assinaturas são coletadas.
/// </summary>
public class ContractSignedNotification
{
    /// <summary>
    /// Número do contrato
    /// </summary>
    public string ContractNumber { get; set; } = string.Empty;

    /// <summary>
    /// Nome do expositor
    /// </summary>
    public string ExhibitorName { get; set; } = string.Empty;

    /// <summary>
    /// Nome do evento/expo
    /// </summary>
    public string ExpoName { get; set; } = string.Empty;

    /// <summary>
    /// Lista de emails dos usuários do departamento jurídico
    /// </summary>
    public List<EmailRecipient> LegalDepartmentUsers { get; set; } = new();

    /// <summary>
    /// Data/hora da assinatura
    /// </summary>
    public DateTime SignedAt { get; set; }

    /// <summary>
    /// URL do sistema para acessar
    /// </summary>
    public string SystemUrl { get; set; } = string.Empty;
}

/// <summary>
/// Dados para notificação de contrato pendente de envio (aprovado internamente)
/// </summary>
public class ContractPendingSendNotification
{
    /// <summary>
    /// Número do contrato
    /// </summary>
    public string ContractNumber { get; set; } = string.Empty;

    /// <summary>
    /// Nome da empresa (TradeName ou LegalName)
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Email da empresa
    /// </summary>
    public string? CompanyEmail { get; set; }

    /// <summary>
    /// Nome do evento/expo
    /// </summary>
    public string ExpoName { get; set; } = string.Empty;

    /// <summary>
    /// Edição do evento (ex: 20)
    /// </summary>
    public short ExpoEdition { get; set; }

    /// <summary>
    /// Ano do evento (ex: 2026)
    /// </summary>
    public short ExpoYear { get; set; }

    /// <summary>
    /// Lista de emails dos representantes
    /// </summary>
    public List<RepresentativeEmail> Representatives { get; set; } = new();

    /// <summary>
    /// Data/hora da aprovação interna
    /// </summary>
    public DateTime ApprovedAt { get; set; }

    /// <summary>
    /// URL do sistema para acessar
    /// </summary>
    public string SystemUrl { get; set; } = string.Empty;
}

