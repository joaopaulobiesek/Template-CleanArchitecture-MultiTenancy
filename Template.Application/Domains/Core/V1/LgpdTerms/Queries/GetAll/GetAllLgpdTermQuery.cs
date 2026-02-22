using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Models;
using Template.Application.Common.Security;
using Template.Application.Domains.Core.V1.ViewModels;
using Template.Domain.Constants;
using Template.Domain.Entity.Core;
using System.Linq.Expressions;

namespace Template.Application.Domains.Core.V1.LgpdTerms.Queries.GetAll;

[Authorize(Roles = $"{Roles.Admin},{Roles.TI},{Roles.User}")]
[Authorize(Policy = $"{Policies.CanList},{Policies.CanView}")]
public class GetAllLgpdTermQuery : BasePaginatedQuery { }

public class GetAllLgpdTermQueryHandler : HandlerBase<GetAllLgpdTermQuery, IEnumerable<LgpdTermVM>>
{
    private readonly ILgpdTermRepository _repository;

    public GetAllLgpdTermQueryHandler(
        HandlerDependencies<GetAllLgpdTermQuery, IEnumerable<LgpdTermVM>> dependencies,
        ILgpdTermRepository repository) : base(dependencies)
    {
        _repository = repository;
    }

    protected override async Task<ApiResponse<IEnumerable<LgpdTermVM>>> RunCore(
        GetAllLgpdTermQuery request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        var query = _repository.SearchIQueryable(request.Src, request.GetCustomFilterDictionary());

        query = request.AscDesc == -1
            ? query.OrderByDescending(GetSortProperty(request.ColumnName))
            : query.OrderBy(GetSortProperty(request.ColumnName));

        var list = await PaginatedList<LgpdTerm>.CreateAsync(
            query, request.PageNumber, request.PageSize, cancellationToken);

        var viewModels = list.Data?.Select(x => LgpdTermVM.FromDomain(x)).ToList();

        return new PaginatedList<LgpdTermVM>(
            viewModels,
            list.TotalItens,
            list.PageNumber,
            request.PageSize
        );
    }

    private static Expression<Func<LgpdTerm, object>> GetSortProperty(string? param)
        => param?.ToLower() switch
        {
            "publishedat" => t => t.PublishedAt,
            "isactive" => t => t.IsActive,
            _ => t => t.Version
        };
}
