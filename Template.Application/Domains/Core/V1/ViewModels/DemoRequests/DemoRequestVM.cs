using Template.Domain.Entity.Core;

namespace Template.Application.Domains.Core.V1.ViewModels.DemoRequests;

public class DemoRequestVM
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? EventType { get; set; }
    public string? EstimatedAudience { get; set; }
    public string? Message { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusDisplayName { get; set; } = string.Empty;
    public string? AdminNotes { get; set; }
    public DateTime? ContactedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public DemoRequestVM() { }

    public DemoRequestVM(
        Guid id,
        string fullName,
        string email,
        string phone,
        string? companyName,
        string? eventType,
        string? estimatedAudience,
        string? message,
        string status,
        string? adminNotes,
        DateTime? contactedAt,
        DateTime createdAt)
    {
        Id = id;
        FullName = fullName;
        Email = email;
        Phone = phone;
        CompanyName = companyName;
        EventType = eventType;
        EstimatedAudience = estimatedAudience;
        Message = message;
        Status = status;
        StatusDisplayName = DemoRequestStatus.GetDisplayName(status);
        AdminNotes = adminNotes;
        ContactedAt = contactedAt;
        CreatedAt = createdAt;
    }

    public static DemoRequestVM FromDomain(DemoRequest? entity)
    {
        if (entity == null) return new DemoRequestVM();

        return new DemoRequestVM(
            entity.Id,
            entity.FullName,
            entity.Email,
            entity.Phone,
            entity.CompanyName,
            entity.EventType,
            entity.EstimatedAudience,
            entity.Message,
            entity.Status,
            entity.AdminNotes,
            entity.ContactedAt,
            entity.CreatedAt);
    }
}
