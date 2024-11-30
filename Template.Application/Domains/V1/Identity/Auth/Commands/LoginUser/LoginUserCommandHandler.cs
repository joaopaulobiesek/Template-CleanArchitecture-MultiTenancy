using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Models;
using Template.Application.Domains.V1.ViewModels.Users;

namespace Template.Application.Domains.V1.Identity.Auth.Commands.LoginUser;

public class LoginUserCommandHandler : HandlerBase<LoginUserCommand, LoginUserVm>
{
    private readonly IIdentityService _service;

    public LoginUserCommandHandler(HandlerDependencies<LoginUserCommand, LoginUserVm> dependencies) : base(dependencies)
    {
        _service = dependencies.IdentityService;
    }

    protected override async Task<ApiResponse<LoginUserVm>> RunCore(LoginUserCommand request, CancellationToken cancellationToken, object? additionalData = null)
    {
        var user = await _service.LoginAsync(request.Email, request.Password, _user.X_Tenant_ID);
        var userVm = new LoginUserVm
        {
            Name = user.Item1,
            Email = user.Item2,
            Token = user.Item3
        };

        return new SuccessResponse<LoginUserVm>("Login realizado com sucesso!", userVm);
    }
}