using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Models;
using Template.Application.Common.Security;
using Template.Application.Domains.V1.ViewModels.Users;
using Template.Domain.Constants;
using static Template.Domain.Constants.Policies;

namespace Template.Application.Domains.V1.Identity.Users.Queries.GetById;

[Authorize(Roles = $"{Roles.Admin},{Roles.TI}")]
[Authorize(Policy = $"{CanView},{CanManageSettings},{CanManageUsers}", PolicyRequirementType = RequirementType.All)]
public class GetByIdQuery
{
    public string UserId { get; set; } = string.Empty;
}

public class GetByIdQueryHandler : HandlerBase<GetByIdQuery, UserVm>
{
    private readonly IIdentityService _identity;

    public GetByIdQueryHandler(HandlerDependencies<GetByIdQuery, UserVm> dependencies) : base(dependencies)
    {
        _identity = dependencies.IdentityService;
    }

    protected override async Task<ApiResponse<UserVm>> RunCore(GetByIdQuery request, CancellationToken cancellationToken, object? additionalData)
    {
        var user = await _identity.GetUserByIdAsync(request.UserId);

        if (user is null)
        {
            return new ErrorResponse<UserVm>("Usuário não encontrado.", 404);
        }

        user.Roles = await _identity.GetUserRole(request.UserId);
        user.Policies = await _identity.GetUserPolicies(request.UserId);

        return new SuccessResponse<UserVm>("Usuário encontrado com sucesso.", user);
    }
}
