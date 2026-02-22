using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Models;
using Template.Application.Common.Security;
using Template.Application.Domains.Core.V1.ViewModels;
using Template.Domain.Constants;

namespace Template.Application.Domains.Core.V1.LgpdTerms.Queries.GetById;

[Authorize(Roles = $"{Roles.Admin},{Roles.TI},{Roles.User}")]
[Authorize(Policy = $"{Policies.CanView}")]
public class GetByIdLgpdTermQuery
{
    public Guid Id { get; set; }
}

public class GetByIdLgpdTermQueryHandler : HandlerBase<GetByIdLgpdTermQuery, LgpdTermVM>
{
    private readonly ILgpdTermRepository _repository;

    public GetByIdLgpdTermQueryHandler(
        HandlerDependencies<GetByIdLgpdTermQuery, LgpdTermVM> dependencies,
        ILgpdTermRepository repository) : base(dependencies)
    {
        _repository = repository;
    }

    protected override async Task<ApiResponse<LgpdTermVM>> RunCore(
        GetByIdLgpdTermQuery request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        var term = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (term == null)
        {
            return new ErrorResponse<LgpdTermVM>("Termo não encontrado.", 404);
        }

        return new SuccessResponse<LgpdTermVM>(
            "Termo encontrado.",
            LgpdTermVM.FromDomain(term)
        );
    }
}
