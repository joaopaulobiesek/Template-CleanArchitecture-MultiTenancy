using Template.Domain.Validation;

namespace Template.Domain.Entity.Core;

/// <summary>
/// Solicitação de demonstração do sistema Template.
/// Dados coletados do formulário do site institucional.
/// </summary>
public sealed class DemoRequest : Entity
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string? CompanyName { get; private set; }
    public string? EventType { get; private set; }
    public string? EstimatedAudience { get; private set; }
    public string? Message { get; private set; }

    /// <summary>
    /// Status da solicitação: Pending, Contacted, Converted, Rejected
    /// </summary>
    public string Status { get; private set; } = DemoRequestStatus.Pending;

    /// <summary>
    /// Observações internas do Admin sobre o contato
    /// </summary>
    public string? AdminNotes { get; private set; }

    /// <summary>
    /// Data em que o Admin entrou em contato
    /// </summary>
    public DateTime? ContactedAt { get; private set; }

    // Construtor para EF Core
    public DemoRequest() { }

    public void Create(
        string fullName,
        string email,
        string phone,
        string? companyName,
        string? eventType,
        string? estimatedAudience,
        string? message)
    {
        ValidateFullName(fullName);
        ValidateEmail(email);
        ValidatePhone(phone);

        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
        Phone = phone.Trim();
        CompanyName = companyName?.Trim();
        EventType = eventType?.Trim();
        EstimatedAudience = estimatedAudience?.Trim();
        Message = message?.Trim();
        Status = DemoRequestStatus.Pending;
    }

    public void MarkAsContacted(string? adminNotes = null)
    {
        Status = DemoRequestStatus.Contacted;
        ContactedAt = DateTime.UtcNow;
        AdminNotes = adminNotes?.Trim();
        Updated();
    }

    public void MarkAsConverted(string? adminNotes = null)
    {
        Status = DemoRequestStatus.Converted;
        if (!string.IsNullOrWhiteSpace(adminNotes))
            AdminNotes = adminNotes.Trim();
        Updated();
    }

    public void MarkAsRejected(string? adminNotes = null)
    {
        Status = DemoRequestStatus.Rejected;
        if (!string.IsNullOrWhiteSpace(adminNotes))
            AdminNotes = adminNotes.Trim();
        Updated();
    }

    public void UpdateAdminNotes(string? adminNotes)
    {
        AdminNotes = adminNotes?.Trim();
        Updated();
    }

    private static void ValidateFullName(string fullName)
    {
        DomainExceptionValidation.ValidateRequiredString(fullName, "Nome completo é obrigatório.");
        DomainExceptionValidation.ValidateMaxLength(fullName, 200, "Nome completo deve ter no máximo 200 caracteres.");
    }

    private static void ValidateEmail(string email)
    {
        DomainExceptionValidation.ValidateRequiredString(email, "E-mail é obrigatório.");
        DomainExceptionValidation.ValidateMaxLength(email, 200, "E-mail deve ter no máximo 200 caracteres.");
        DomainExceptionValidation.When(
            !email.Contains('@') || !email.Contains('.'),
            "E-mail inválido.");
    }

    private static void ValidatePhone(string phone)
    {
        DomainExceptionValidation.ValidateRequiredString(phone, "Telefone é obrigatório.");
        DomainExceptionValidation.ValidateMaxLength(phone, 20, "Telefone deve ter no máximo 20 caracteres.");
    }
}

/// <summary>
/// Status possíveis para DemoRequest
/// </summary>
public static class DemoRequestStatus
{
    public const string Pending = "Pending";
    public const string Contacted = "Contacted";
    public const string Converted = "Converted";
    public const string Rejected = "Rejected";

    public static IReadOnlyList<string> GetAll() =>
        new[] { Pending, Contacted, Converted, Rejected };

    public static string GetDisplayName(string status) => status switch
    {
        Pending => "Pendente",
        Contacted => "Contatado",
        Converted => "Convertido",
        Rejected => "Rejeitado",
        _ => status
    };
}
