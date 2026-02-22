using Template.Domain.Entity.Core;

namespace Template.Application.Domains.Core.V1.ViewModels;

public class LgpdTermVM
{
    public Guid Id { get; set; }
    public string Version { get; set; } = string.Empty;

    // Termo de Uso
    public string TermsOfUseContent { get; set; } = string.Empty;
    public string TermsOfUseHash { get; set; } = string.Empty;

    // Política de Privacidade
    public string PrivacyPolicyContent { get; set; } = string.Empty;
    public string PrivacyPolicyHash { get; set; } = string.Empty;

    // Metadata
    public DateTime PublishedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime? InactivatedAt { get; set; }
    public Guid? PublishedByUserId { get; set; }

    public LgpdTermVM() { }

    public LgpdTermVM(
        Guid id,
        string version,
        string termsOfUseContent,
        string termsOfUseHash,
        string privacyPolicyContent,
        string privacyPolicyHash,
        DateTime publishedAt,
        bool isActive,
        DateTime? inactivatedAt,
        Guid? publishedByUserId)
    {
        Id = id;
        Version = version;
        TermsOfUseContent = termsOfUseContent;
        TermsOfUseHash = termsOfUseHash;
        PrivacyPolicyContent = privacyPolicyContent;
        PrivacyPolicyHash = privacyPolicyHash;
        PublishedAt = publishedAt;
        IsActive = isActive;
        InactivatedAt = inactivatedAt;
        PublishedByUserId = publishedByUserId;
    }

    public static LgpdTermVM FromDomain(LgpdTerm? term)
    {
        if (term == null) return new LgpdTermVM();

        return new LgpdTermVM(
            term.Id,
            term.Version,
            term.TermsOfUseContent,
            term.TermsOfUseHash,
            term.PrivacyPolicyContent,
            term.PrivacyPolicyHash,
            term.PublishedAt,
            term.IsActive,
            term.InactivatedAt,
            term.PublishedByUserId
        );
    }
}
