using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Models;
using Template.Application.Domains.V1.ViewModels.Users;

namespace Template.Application.Domains.V1.Identity.Auth.Commands.LoginUser;

public class LoginUserCommandHandler : HandlerBase<LoginUserCommand, LoginUserVm>
{
    private readonly IIdentityService _service;
    private readonly IClientRepository _repositoryClient;

    public LoginUserCommandHandler(HandlerDependencies<LoginUserCommand, LoginUserVm> dependencies, IClientRepository repositoryClient) : base(dependencies)
    {
        _service = dependencies.IdentityService;
        _repositoryClient = repositoryClient;
    }

    protected override async Task<ApiResponse<LoginUserVm>> RunCore(LoginUserCommand request, CancellationToken cancellationToken, object? additionalData = null)
    {
        var loginResult = await _service.LoginAsync(request.Email, request.Password, _user.X_Tenant_ID);

        if (loginResult is null)
            return new ErrorResponse<LoginUserVm>("Senha ou email inválido!");

        // Verifica se o e-mail não foi confirmado
        if (loginResult.AccessToken == "EMAIL_NOT_CONFIRMED")
        {
            return new ErrorResponse<LoginUserVm>(
                "Seu e-mail ainda não foi confirmado. Verifique sua caixa de entrada ou solicite um novo e-mail de confirmação.",
                401,
                new LoginUserVm { Email = loginResult.Email, EmailNotConfirmed = true });
        }

        var roles = await _service.GetUserRole(loginResult.UserId);
        var policies = await _service.GetUserPolicies(loginResult.UserId);

        var modules = new List<string>();
        if (this._user.X_Tenant_ID != Guid.Empty)
        {
            modules = await _repositoryClient.GetActiveModulesAsync(this._user.X_Tenant_ID);
        }

        var userVm = new LoginUserVm
        {
            Name = loginResult.FullName,
            Email = loginResult.Email,
            Modules = modules,
            Roles = roles,
            Policies = policies,
            Token = loginResult.AccessToken,
            RefreshToken = loginResult.RefreshToken,
            AccessTokenExpires = loginResult.AccessTokenExpires,
            RefreshTokenExpires = loginResult.RefreshTokenExpires
        };

        return new SuccessResponse<LoginUserVm>("Login realizado com sucesso!", userVm);
    }
}
