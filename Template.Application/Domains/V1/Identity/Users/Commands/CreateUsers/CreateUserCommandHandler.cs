using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Models;
using Template.Domain.Constants;

namespace Template.Application.Domains.V1.Identity.Users.Commands.CreateUsers;

public class CreateUserCommandHandler : HandlerBase<CreateUserCommand, User>
{
    private readonly IIdentityService _identity;

    public CreateUserCommandHandler(HandlerDependencies<CreateUserCommand, User> dependencies) : base(dependencies)
    {
        _identity = dependencies.IdentityService;
    }

    protected override async Task<ApiResponse<User>> RunCore(CreateUserCommand request, CancellationToken cancellationToken, object? additionalData)
    {
        // Validação: apenas Admin pode conceder role Admin
        if (request.Roles != null && request.Roles.Any(r => r == Roles.Admin))
        {
            // Verifica se o usuário atual é Admin
            var isCurrentUserAdmin = await _identity.IsInRoleAsync(_user.Id!, Roles.Admin);
            if (!isCurrentUserAdmin)
            {
                return new ErrorResponse<User>("Apenas um Admin pode conceder a role Admin.", 403);
            }
        }

        var result = await _identity.CreateUserAsync(
            new User()
            {
                FullName = request.FullName,
                Email = request.Email,
                Roles = request.Roles!,
                Policies = request.Policies!,
                ProfileImageUrl = request.ProfileImageUrl,
                BypassIp = request.BypassIp
            },
            request.Password!
        );

        if (result != null)
            return new SuccessResponse<User>("Ok",
                new User
                {
                    Id = result.Id,
                    Email = result.Email,
                    FullName = result.FullName,
                    ProfileImageUrl = result.ProfileImageUrl,
                    Policies = result.Policies,
                    Roles = result.Roles
                }
            );

        return new ErrorResponse<User>("Deu erro");
    }
}