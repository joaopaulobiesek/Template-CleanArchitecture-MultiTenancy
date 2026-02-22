using Microsoft.AspNetCore.Mvc;
using Template.Api.Controllers.System;
using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Interfaces.Services;
using Template.Application.Common.Models;
using Template.Application.Domains.V1.Identity.Auth.Commands.ConfirmEmail;
using Template.Application.Domains.V1.Identity.Auth.Commands.ForgotPassword;
using Template.Application.Domains.V1.Identity.Auth.Commands.LoginUser;
using Template.Application.Domains.V1.Identity.Auth.Commands.RefreshToken;
using Template.Application.Domains.V1.Identity.Auth.Commands.Register;
using Template.Application.Domains.V1.Identity.Auth.Commands.RegisterCore;
using Template.Application.Domains.V1.Identity.Auth.Commands.ResendConfirmationEmail;
using Template.Application.Domains.V1.Identity.Auth.Commands.ResetPassword;
using Template.Application.Domains.V1.ViewModels.Users;

namespace Template.Api.Controllers.V1.Identity.Auth;

/// <summary>
/// Controller responsável pelas operações de autenticação, incluindo login no sistema e integração com provedores externos.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : BaseController
{

    /// <summary>
    /// Realiza a autenticação do usuário no sistema.
    /// </summary>
    [HttpPost("Login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<LoginUserVm>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse<string>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginAsync(
        [FromServices] IHandlerBase<LoginUserCommand, LoginUserVm> handler,
        [FromBody] LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        var response = await handler.Execute(command, cancellationToken);

        if (!response.Success)
            return BadRequest(response);
        else if (string.IsNullOrEmpty(response.Data!.Token))
            return Unauthorized();
        else
        {
            // Define cookies HTTP-Only com Access Token e Refresh Token
            SetAuthCookies(
                response.Data.Token,
                response.Data.RefreshToken,
                response.Data.RefreshTokenExpires);

            return Ok(response);
        }
    }

    /// <summary>
    /// Renova os tokens usando o Refresh Token.
    /// </summary>
    /// <remarks>
    /// Este endpoint recebe o Refresh Token via cookie HTTP-Only e retorna novos tokens.
    /// Implementa Refresh Token Rotation - o token anterior é invalidado após uso.
    /// </remarks>
    [HttpPost("RefreshToken")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<TokenPairVm>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshTokenAsync(
        [FromServices] IHandlerBase<RefreshTokenCommand, TokenPairVm> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Execute(new RefreshTokenCommand(), cancellationToken);

        if (!response.Success)
            return Unauthorized(response);

        // Define novos cookies
        SetAuthCookies(
            response.Data!.AccessToken,
            response.Data.RefreshToken,
            response.Data.RefreshTokenExpires);

        return Ok(response);
    }

    /// <summary>
    /// Processa o retorno do Google OAuth 2.0 após a autenticação.
    /// </summary>
    [HttpGet("Google/Callback")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<LoginUserVm>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse<string>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse<string>))]
    public async Task<IActionResult> GoogleCallback(
        [FromServices] IGoogle google,
        [FromServices] IIdentityService service,
        string code,
        string? state,
        CancellationToken cancellationToken)
    {
        var result = await google.AuthenticateUserAsync(service, code, state);

        // Se autenticação bem-sucedida, define cookie HTTP-Only (Google usa método legado)
        if (result.Success && !string.IsNullOrEmpty(result.Data?.Token))
        {
            SetAuthCookie(result.Data.Token);
        }

        return HandleResponse(result);
    }

    /// <summary>
    /// Gera o link de login para autenticação no Google.
    /// </summary>
    [HttpGet("Google")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<string>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse<string>))]
    public async Task<IActionResult> GoogleLogin(
        [FromServices] IGoogle google,
        CancellationToken cancellationToken)
        => HandleResponse(google.GenerateAuthenticationUrl(Guid.Empty.ToString()));

    /// <summary>
    /// Registra um novo usuário no sistema.
    /// Após o registro, um email de confirmação é enviado para o endereço informado.
    /// O usuário só poderá fazer login após confirmar o email.
    /// </summary>
    [HttpPost("Register")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<RegisterResultVm>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse<string>))]
    public async Task<IActionResult> RegisterAsync(
        [FromServices] IHandlerBase<RegisterCommand, RegisterResultVm> handler,
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var response = await handler.Execute(command, cancellationToken);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Registra um novo usuário E cliente no sistema (registro Core).
    /// Após o registro, um email de confirmação é enviado para o endereço informado.
    /// O usuário recebe Role.User e 6 policies padrão.
    /// O cliente é criado com Paid = false.
    /// </summary>
    [HttpPost("RegisterCore")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<RegisterCoreResultVm>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse<string>))]
    public async Task<IActionResult> RegisterCoreAsync(
        [FromServices] IHandlerBase<RegisterCoreCommand, RegisterCoreResultVm> handler,
        [FromBody] RegisterCoreCommand command,
        CancellationToken cancellationToken)
    {
        var response = await handler.Execute(command, cancellationToken);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Confirma o email do usuário usando o token enviado por email.
    /// </summary>
    /// <remarks>
    /// Este endpoint é chamado quando o usuário clica no link de confirmação enviado por email.
    /// O token é enviado como query parameter e deve ser URL-encoded.
    /// </remarks>
    [HttpGet("ConfirmEmail")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<string>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse<string>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse<string>))]
    public async Task<IActionResult> ConfirmEmailAsync(
        [FromServices] IHandlerBase<ConfirmEmailCommand, string> handler,
        [FromQuery] string userId,
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        var command = new ConfirmEmailCommand { UserId = userId, Token = token };
        var response = await handler.Execute(command, cancellationToken);

        if (!response.Success)
        {
            if (response.StatusCode == 404)
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Reenvia o email de confirmação para o usuário.
    /// </summary>
    /// <remarks>
    /// Útil quando o usuário não recebeu o email de confirmação ou o link expirou.
    /// Por segurança, a resposta é sempre positiva mesmo se o email não existir.
    /// </remarks>
    [HttpPost("ResendConfirmationEmail")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<string>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse<string>))]
    public async Task<IActionResult> ResendConfirmationEmailAsync(
        [FromServices] IHandlerBase<ResendConfirmationEmailCommand, string> handler,
        [FromBody] ResendConfirmationEmailCommand command,
        CancellationToken cancellationToken)
    {
        var response = await handler.Execute(command, cancellationToken);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Solicita a redefinição de senha. Envia um email com link para redefinir.
    /// </summary>
    /// <remarks>
    /// Por segurança, a resposta é sempre positiva mesmo se o email não existir.
    /// O link de redefinição expira em 24 horas.
    /// </remarks>
    [HttpPost("ForgotPassword")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<string>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse<string>))]
    public async Task<IActionResult> ForgotPasswordAsync(
        [FromServices] IHandlerBase<ForgotPasswordCommand, string> handler,
        [FromBody] ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var response = await handler.Execute(command, cancellationToken);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// Redefine a senha do usuário usando o token enviado por email.
    /// </summary>
    /// <remarks>
    /// Este endpoint é chamado quando o usuário preenche o formulário de nova senha.
    /// O token e userId são obtidos da URL do link enviado por email.
    /// </remarks>
    [HttpPost("ResetPassword")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<string>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse<string>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse<string>))]
    public async Task<IActionResult> ResetPasswordAsync(
        [FromServices] IHandlerBase<ResetPasswordCommand, string> handler,
        [FromBody] ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var response = await handler.Execute(command, cancellationToken);

        if (!response.Success)
        {
            if (response.StatusCode == 404)
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Realiza o logout do usuário, removendo os cookies de autenticação e revogando o Refresh Token.
    /// </summary>
    [HttpPost("Logout")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SuccessResponse<string>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(
        [FromServices] IIdentityService identityService)
    {
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        // Tenta revogar o Refresh Token no banco
        var accessToken = Request.Cookies["auth_token"];
        if (!string.IsNullOrEmpty(accessToken))
        {
            var userId = identityService.ExtractUserIdFromToken(accessToken);
            if (!string.IsNullOrEmpty(userId))
            {
                await identityService.RevokeRefreshTokenAsync(userId);
            }
        }

        // Remove o cookie de Access Token
        Response.Cookies.Delete("auth_token", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = isDevelopment ? SameSiteMode.None : SameSiteMode.Lax,
            Path = "/"
        });

        // Remove o cookie de Refresh Token
        Response.Cookies.Delete("refresh_token", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = isDevelopment ? SameSiteMode.None : SameSiteMode.Lax,
            Path = "/"
        });

        return Ok(new SuccessResponse<string>("Logout realizado com sucesso"));
    }

    /// <summary>
    /// Define os cookies HTTP-Only para Access Token e Refresh Token.
    /// </summary>
    private void SetAuthCookies(string accessToken, string refreshToken, DateTime cookieExpires)
    {
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        // Cookie do Access Token
        Response.Cookies.Append("auth_token", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = isDevelopment ? SameSiteMode.None : SameSiteMode.Lax,
            Path = "/",
            Expires = cookieExpires
        });

        // Cookie do Refresh Token
        Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = isDevelopment ? SameSiteMode.None : SameSiteMode.Lax,
            Path = "/",
            Expires = cookieExpires
        });
    }

    /// <summary>
    /// Define o cookie HTTP-Only para autenticação JWT (método legado para Google).
    /// TODO: Atualizar Google OAuth para usar o novo sistema de tokens com Refresh Token
    /// </summary>
    private void SetAuthCookie(string token, int expiracaoEmMinutos = 60)
    {
        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = isDevelopment ? SameSiteMode.None : SameSiteMode.Lax,
            Path = "/",
            MaxAge = TimeSpan.FromMinutes(expiracaoEmMinutos)
        };

        Response.Cookies.Append("auth_token", token, cookieOptions);
    }
}
