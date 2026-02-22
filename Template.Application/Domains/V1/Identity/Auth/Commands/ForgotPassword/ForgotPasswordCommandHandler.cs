using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Interfaces.Services;
using Template.Application.Common.Models;

namespace Template.Application.Domains.V1.Identity.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : HandlerBase<ForgotPasswordCommand, string>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailSystemService _emailSystemService;
    private readonly ITenantCacheService _tenantCacheService;

    public ForgotPasswordCommandHandler(
        HandlerDependencies<ForgotPasswordCommand, string> dependencies,
        IEmailSystemService emailSystemService,
        ITenantCacheService tenantCacheService) : base(dependencies)
    {
        _identityService = dependencies.IdentityService;
        _emailSystemService = emailSystemService;
        _tenantCacheService = tenantCacheService;
    }

    protected override async Task<ApiResponse<string>> RunCore(
        ForgotPasswordCommand request,
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
            return new SuccessResponse<string>("Se o e-mail estiver cadastrado, você receberá um link para redefinir sua senha.");
        }

        // Verifica se o e-mail foi confirmado
        var isConfirmed = await _identityService.IsEmailConfirmedAsync(user.Id!);
        if (!isConfirmed)
        {
            // Por segurança, retorna a mesma mensagem
            return new SuccessResponse<string>("Se o e-mail estiver cadastrado, você receberá um link para redefinir sua senha.");
        }

        // Gera token de reset de senha
        var resetToken = await _identityService.GeneratePasswordResetTokenAsync(user.Id!);

        if (string.IsNullOrEmpty(resetToken))
        {
            return new ErrorResponse<string>("Falha ao gerar token de recuperação.", 500);
        }

        // Busca URL do tenant para montar link de reset
        var systemUrl = await _tenantCacheService.GetTenantUrlByIdAsync(_user.X_Tenant_ID, cancellationToken);
        if (string.IsNullOrWhiteSpace(systemUrl))
        {
            systemUrl = "https://Template.cc";
        }

        if (!systemUrl.StartsWith("http://") && !systemUrl.StartsWith("https://"))
        {
            systemUrl = $"https://{systemUrl}";
        }

        // Monta URL de reset com token Base64 URL-safe
        var tokenBytes = System.Text.Encoding.UTF8.GetBytes(resetToken);
        var base64Token = Convert.ToBase64String(tokenBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
        var resetUrl = $"{systemUrl}/reset-password?userId={user.Id}&token={base64Token}";

        // Envia e-mail de reset de senha
        var emailNotification = new PasswordResetNotification
        {
            UserEmail = request.Email,
            UserName = user.FullName ?? request.Email,
            ResetUrl = resetUrl,
            RequestedAt = DateTime.UtcNow
        };

        var emailResult = await _emailSystemService.SendPasswordResetAsync(emailNotification, cancellationToken);

        if (!emailResult.Success)
        {
            return new ErrorResponse<string>("Falha ao enviar e-mail de recuperação. Tente novamente.", 500);
        }

        return new SuccessResponse<string>("Se o e-mail estiver cadastrado, você receberá um link para redefinir sua senha.");
    }
}
