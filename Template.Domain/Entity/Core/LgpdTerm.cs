using Template.Domain.Validation;

namespace Template.Domain.Entity.Core;

/// <summary>
/// Termo LGPD centralizado no Core - gerenciado uma única vez para todos os tenants
/// Contém Termo de Uso e Política de Privacidade separados
/// </summary>
public sealed class LgpdTerm : Entity
{
    // Versionamento
    public string Version { get; private set; } // "1.0", "2.0", "2.1"

    // Conteúdo - Termo de Uso
    public string TermsOfUseContent { get; private set; } // Texto completo (pode ser HTML)
    public string TermsOfUseHash { get; private set; } // SHA256 do conteúdo

    // Conteúdo - Política de Privacidade
    public string PrivacyPolicyContent { get; private set; } // Texto completo (pode ser HTML)
    public string PrivacyPolicyHash { get; private set; } // SHA256 do conteúdo

    // Metadata
    public DateTime PublishedAt { get; private set; }
    public bool IsActive { get; private set; } // Apenas 1 pode estar ativo
    public DateTime? InactivatedAt { get; private set; }

    // Quem publicou
    public Guid? PublishedByUserId { get; private set; }

    private LgpdTerm() { }

    public static LgpdTerm Create(
        string version,
        string termsOfUseContent,
        string privacyPolicyContent,
        Guid publishedByUserId)
    {
        DomainExceptionValidation.When(
            string.IsNullOrWhiteSpace(version),
            "Versão do termo é obrigatória");

        DomainExceptionValidation.When(
            !System.Text.RegularExpressions.Regex.IsMatch(version, @"^\d+\.\d+(\.\d+)?$"),
            "Versão deve estar no formato X.Y ou X.Y.Z (ex: 1.0 ou 2.1.3)");

        ValidateTermsOfUseContent(termsOfUseContent);
        ValidatePrivacyPolicyContent(privacyPolicyContent);

        var term = new LgpdTerm
        {
            Version = version,
            TermsOfUseContent = termsOfUseContent,
            TermsOfUseHash = GenerateContentHash(termsOfUseContent),
            PrivacyPolicyContent = privacyPolicyContent,
            PrivacyPolicyHash = GenerateContentHash(privacyPolicyContent),
            PublishedAt = DateTime.UtcNow,
            IsActive = false,
            InactivatedAt = null,
            PublishedByUserId = publishedByUserId
        };

        return term;
    }

    public static string GenerateContentHash(string content)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public void UpdateTermsOfUse(string newContent)
    {
        DomainExceptionValidation.When(
            IsActive,
            "Não é possível atualizar um termo já ativo. Crie uma nova versão.");

        ValidateTermsOfUseContent(newContent);

        TermsOfUseContent = newContent;
        TermsOfUseHash = GenerateContentHash(newContent);
        Updated();
    }

    public void UpdatePrivacyPolicy(string newContent)
    {
        DomainExceptionValidation.When(
            IsActive,
            "Não é possível atualizar um termo já ativo. Crie uma nova versão.");

        ValidatePrivacyPolicyContent(newContent);

        PrivacyPolicyContent = newContent;
        PrivacyPolicyHash = GenerateContentHash(newContent);
        Updated();
    }

    public void Activate()
    {
        DomainExceptionValidation.When(
            IsActive,
            "Termo já está ativo");

        IsActive = true;
        Updated();
    }

    public void Deactivate()
    {
        DomainExceptionValidation.When(
            !IsActive,
            "Termo já está inativo");

        IsActive = false;
        InactivatedAt = DateTime.UtcNow;
        Updated();
    }

    private static void ValidateTermsOfUseContent(string content)
    {
        DomainExceptionValidation.When(
            string.IsNullOrWhiteSpace(content),
            "Conteúdo do Termo de Uso é obrigatório");

        DomainExceptionValidation.When(
            content.Length < 100,
            "Conteúdo do Termo de Uso deve ter pelo menos 100 caracteres");

        DomainExceptionValidation.When(
            content.Length > 50000,
            "Conteúdo do Termo de Uso não pode exceder 50.000 caracteres");
    }

    private static void ValidatePrivacyPolicyContent(string content)
    {
        DomainExceptionValidation.When(
            string.IsNullOrWhiteSpace(content),
            "Conteúdo da Política de Privacidade é obrigatório");

        DomainExceptionValidation.When(
            content.Length < 100,
            "Conteúdo da Política de Privacidade deve ter pelo menos 100 caracteres");

        DomainExceptionValidation.When(
            content.Length > 50000,
            "Conteúdo da Política de Privacidade não pode exceder 50.000 caracteres");
    }
}
