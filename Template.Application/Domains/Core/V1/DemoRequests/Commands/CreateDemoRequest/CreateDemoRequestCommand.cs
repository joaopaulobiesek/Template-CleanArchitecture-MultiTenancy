namespace Template.Application.Domains.Core.V1.DemoRequests.Commands.CreateDemoRequest;

/// <summary>
/// Command para criar solicitação de demonstração.
/// NÃO requer autenticação - endpoint público do site.
/// </summary>
public class CreateDemoRequestCommand
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? EventType { get; set; }
    public string? EstimatedAudience { get; set; }
    public string? Message { get; set; }
}
