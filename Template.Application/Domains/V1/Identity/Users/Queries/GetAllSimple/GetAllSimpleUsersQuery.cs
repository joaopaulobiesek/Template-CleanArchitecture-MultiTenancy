using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Models;
using Template.Application.Common.Security;
using Template.Application.Domains.V1.ViewModels.Users;
using Template.Domain.Constants;
using static Template.Domain.Constants.Policies;

namespace Template.Application.Domains.V1.Identity.Users.Queries.GetAllSimple;

[Authorize(Roles = $"{Roles.Admin},{Roles.TI}")]
[Authorize(Policy = $"{CanList},{CanView},{CanManageUsers}", PolicyRequirementType = RequirementType.All)]
public class GetAllSimpleUsersQuery
{
}

public class GetAllSimpleUsersQueryHandler : HandlerBase<GetAllSimpleUsersQuery, IEnumerable<UserSimpleVM>>
{
    private readonly IIdentityService _identityService;

    public GetAllSimpleUsersQueryHandler(
        HandlerDependencies<GetAllSimpleUsersQuery, IEnumerable<UserSimpleVM>> dependencies,
        IIdentityService identityService) : base(dependencies)
    {
        _identityService = identityService;
    }

    protected override async Task<ApiResponse<IEnumerable<UserSimpleVM>>> RunCore(
        GetAllSimpleUsersQuery request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        var users = await _identityService.ListUsersSimpleAsync();

        return new SuccessResponse<IEnumerable<UserSimpleVM>>(
            "Consulta realizada com sucesso.",
            users
        );
    }
}
