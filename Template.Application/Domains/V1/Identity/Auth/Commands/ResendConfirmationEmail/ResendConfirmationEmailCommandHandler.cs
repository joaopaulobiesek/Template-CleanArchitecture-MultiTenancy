using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Interfaces.Services;
using Template.Application.Common.Models;

namespace Template.Application.Domains.V1.Identity.Auth.Commands.ResendConfirmationEmail;

public class ResendConfirmationEmailCommandHandler : HandlerBase<ResendConfirmationEmailCommand, string>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailSystemService _emailSystemService;
    private readonly ITenantCacheService _tenantCacheService;

    public ResendConfirmationEmailCommandHandler(
        HandlerDependencies<ResendConfirmationEmailCommand, string> dependencies,
        IEmailSystemService emailSystemService,
        ITenantCacheService tenantCacheService) : base(dependencies)
    {
        _identityService = dependencies.IdentityService;
        _emailSystemService = emailSystemService;
        _tenantCacheService = tenantCacheService;
    }

    protected override async Task<ApiResponse<string>> RunCore(
        ResendConfirmationEmailCommand request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new ErrorResponse<string>("Email é obrigatório.", 400);
        }

        // Busca usuário por e-mail
        var user = await _identityService.GetUserByEmailAsync(request.Email);

        if (user == null)
        {
            // Por segurança, não informamos se o e-mail existe ou não
            return new SuccessResponse<string>("Se o e-mail estiver cadastrado, um novo e-mail de confirmação será enviado.");
        }

        // Verifica se o e-mail já foi confirmado
        var isConfirmed = await _identityService.IsEmailConfirmedAsync(user.Id!);
        if (isConfirmed)
        {
            return new SuccessResponse<string>("Seu e-mail já foi confirmado. Você pode fazer login normalmente.");
        }

        // Gera novo token de confirmação
        var confirmationToken = await _identityService.GenerateEmailConfirmationTokenAsync(user.Id!);

        if (string.IsNullOrEmpty(confirmationToken))
        {
            return new ErrorResponse<string>("Falha ao gerar token de confirmação.", 500);
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
        var confirmationUrl = $"{systemUrl}/confirm-email?userId={user.Id}&token={base64Token}";

        // Envia e-mail de confirmação
        var emailNotification = new EmailConfirmationNotification
        {
            UserEmail = request.Email,
            UserName = user.FullName ?? request.Email,
            ConfirmationUrl = confirmationUrl,
            RequestedAt = DateTime.UtcNow
        };

        var emailResult = await _emailSystemService.SendEmailConfirmationAsync(emailNotification, cancellationToken);

        if (!emailResult.Success)
        {
            return new ErrorResponse<string>("Falha ao enviar e-mail de confirmação. Tente novamente.", 500);
        }

        return new SuccessResponse<string>("Um novo e-mail de confirmação foi enviado para o seu endereço de e-mail.");
    }
}
