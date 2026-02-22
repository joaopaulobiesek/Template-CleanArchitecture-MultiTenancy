using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Models;

namespace Template.Application.Domains.V1.Identity.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : HandlerBase<ResetPasswordCommand, string>
{
    private readonly IIdentityService _identityService;

    public ResetPasswordCommandHandler(
        HandlerDependencies<ResetPasswordCommand, string> dependencies) : base(dependencies)
    {
        _identityService = dependencies.IdentityService;
    }

    protected override async Task<ApiResponse<string>> RunCore(
        ResetPasswordCommand request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return new ErrorResponse<string>("ID do usuário é obrigatório.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return new ErrorResponse<string>("Token é obrigatório.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return new ErrorResponse<string>("Nova senha é obrigatória.", 400);
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            return new ErrorResponse<string>("As senhas não conferem.", 400);
        }

        // Decodifica token de Base64 URL-safe para string original
        string decodedToken;
        try
        {
            var base64Token = request.Token
                .Replace('-', '+')
                .Replace('_', '/');

            // Adiciona padding se necessário
            switch (base64Token.Length % 4)
            {
                case 2: base64Token += "=="; break;
                case 3: base64Token += "="; break;
            }

            var tokenBytes = Convert.FromBase64String(base64Token);
            decodedToken = System.Text.Encoding.UTF8.GetString(tokenBytes);
        }
        catch
        {
            // Tenta usar o token como veio (compatibilidade)
            decodedToken = request.Token;
        }

        // Reseta a senha
        var result = await _identityService.ResetPasswordAsync(request.UserId, decodedToken, request.NewPassword);

        if (!result.Success)
        {
            return new ErrorResponse<string>(result.Message ?? "Falha ao redefinir senha. O link pode ter expirado.", 400);
        }

        return new SuccessResponse<string>("Senha redefinida com sucesso! Você já pode fazer login com a nova senha.");
    }
}
