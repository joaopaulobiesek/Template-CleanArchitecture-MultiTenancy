using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Models;

namespace Template.Application.Domains.V1.Identity.Auth.Commands.ConfirmEmail;

public class ConfirmEmailCommandHandler : HandlerBase<ConfirmEmailCommand, string>
{
    private readonly IIdentityService _identityService;

    public ConfirmEmailCommandHandler(
        HandlerDependencies<ConfirmEmailCommand, string> dependencies) : base(dependencies)
    {
        _identityService = dependencies.IdentityService;
    }

    protected override async Task<ApiResponse<string>> RunCore(
        ConfirmEmailCommand request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return new ErrorResponse<string>("UserId é obrigatório.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return new ErrorResponse<string>("Token de confirmação é obrigatório.", 400);
        }

        // Decodifica o token de Base64 URL-safe para string original
        // Restaura caracteres Base64 padrão e adiciona padding se necessário
        var base64Token = request.Token
            .Replace('-', '+')
            .Replace('_', '/');

        // Adiciona padding = se necessário
        switch (base64Token.Length % 4)
        {
            case 2: base64Token += "=="; break;
            case 3: base64Token += "="; break;
        }

        string decodedToken;
        try
        {
            var tokenBytes = Convert.FromBase64String(base64Token);
            decodedToken = System.Text.Encoding.UTF8.GetString(tokenBytes);
        }
        catch
        {
            // Se falhar o decode Base64, tenta usar o token direto (compatibilidade)
            decodedToken = System.Web.HttpUtility.UrlDecode(request.Token);
        }

        var result = await _identityService.ConfirmEmailAsync(request.UserId, decodedToken);

        return result;
    }
}
