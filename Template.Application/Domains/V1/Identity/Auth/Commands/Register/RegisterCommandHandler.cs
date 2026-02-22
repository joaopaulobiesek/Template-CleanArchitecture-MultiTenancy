using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Interfaces.Services;
using Template.Application.Common.Models;
using Template.Application.Domains.V1.ViewModels.Users;
using Template.Domain.Constants;

namespace Template.Application.Domains.V1.Identity.Auth.Commands.Register;

public class RegisterCommandHandler : HandlerBase<RegisterCommand, RegisterResultVm>
{
    private readonly IIdentityService _identityService;
    private readonly IClientRepository _repositoryClient;
    private readonly IEmailSystemService _emailSystemService;
    private readonly ITenantCacheService _tenantCacheService;

    public RegisterCommandHandler(
        HandlerDependencies<RegisterCommand, RegisterResultVm> dependencies,
        IClientRepository repositoryClient,
        IEmailSystemService emailSystemService,
        ITenantCacheService tenantCacheService) : base(dependencies)
    {
        _identityService = dependencies.IdentityService;
        _repositoryClient = repositoryClient;
        _emailSystemService = emailSystemService;
        _tenantCacheService = tenantCacheService;
    }

    protected override async Task<ApiResponse<RegisterResultVm>> RunCore(
        RegisterCommand request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        var newUser = new UserVm
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Roles = new List<string> { Roles.User },
            Policies = new List<string> { Policies.CanList, Policies.CanView, Policies.CanCreate, Policies.CanEdit, Policies.CanDelete, Policies.CanViewReports }
        };

        var createdUser = await _identityService.CreateUserAsync(newUser, request.Password);

        if (string.IsNullOrEmpty(createdUser.Id))
        {
            return new ErrorResponse<RegisterResultVm>("Falha ao criar usuário.", 400);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Gera token de confirmação de e-mail
        var confirmationToken = await _identityService.GenerateEmailConfirmationTokenAsync(createdUser.Id);

        if (string.IsNullOrEmpty(confirmationToken))
        {
            return new ErrorResponse<RegisterResultVm>("Falha ao gerar token de confirmação.", 500);
        }

        // Busca URL do tenant para montar link de confirmação
        var systemUrl = await _tenantCacheService.GetTenantUrlByIdAsync(_user.X_Tenant_ID, cancellationToken);
        if (string.IsNullOrWhiteSpace(systemUrl))
        {
            systemUrl = "https://Template.cc";
        }

        if (!systemUrl.StartsWith("http://") && !systemUrl.StartsWith("https://"))
        {
            systemUrl = $"https://{systemUrl}";
        }

        // Monta URL de confirmação com token Base64 URL-safe
        // Converte para Base64 URL-safe (substitui +/ por -_ e remove padding =)
        var tokenBytes = System.Text.Encoding.UTF8.GetBytes(confirmationToken);
        var base64Token = Convert.ToBase64String(tokenBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
        var confirmationUrl = $"{systemUrl}/confirm-email?userId={createdUser.Id}&token={base64Token}";

        // Envia e-mail de confirmação
        var emailNotification = new EmailConfirmationNotification
        {
            UserEmail = request.Email,
            UserName = request.FullName,
            ConfirmationUrl = confirmationUrl,
            RequestedAt = DateTime.UtcNow
        };

        await _emailSystemService.SendEmailConfirmationAsync(emailNotification, cancellationToken);

        var result = new RegisterResultVm
        {
            Email = request.Email,
            FullName = request.FullName,
            RequiresEmailConfirmation = true
        };

        return new SuccessResponse<RegisterResultVm>(
            "Usuário registrado com sucesso! Um e-mail de confirmação foi enviado para seu endereço de e-mail.",
            result);
    }
}
