namespace Template.Application.Common.Models;

/// <summary>
/// Configuration settings for SendGrid email service integration.
/// Each tenant has its own sender email and name configuration.
/// The API Key is global (shared) in appsettings.
/// </summary>
public class SendGridConfiguration
{
    /// <summary>
    /// Configuration key used for binding from appsettings.
    /// </summary>
    public const string Key = "SendGrid";

    /// <summary>
    /// Email address authorized to send emails for this tenant.
    /// Example: "noreply@aiba.org.br"
    /// </summary>
    public string SenderEmail { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the sender.
    /// Example: "AIBA"
    /// </summary>
    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// Validates if the configuration has all required fields populated.
    /// </summary>
    /// <returns>True if configuration is valid, false otherwise.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(SenderEmail)
            && !string.IsNullOrWhiteSpace(SenderName);
    }
}
