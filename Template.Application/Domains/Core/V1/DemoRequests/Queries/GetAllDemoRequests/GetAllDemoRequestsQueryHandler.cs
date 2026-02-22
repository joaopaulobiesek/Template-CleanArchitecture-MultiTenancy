using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Models;
using Template.Application.Domains.Core.V1.ViewModels.DemoRequests;
using Microsoft.EntityFrameworkCore;

namespace Template.Application.Domains.Core.V1.DemoRequests.Queries.GetAllDemoRequests;

public class GetAllDemoRequestsQueryHandler : HandlerBase<GetAllDemoRequestsQuery, IEnumerable<DemoRequestVM>>
{
    private readonly IDemoRequestRepository _repository;

    public GetAllDemoRequestsQueryHandler(
        HandlerDependencies<GetAllDemoRequestsQuery, IEnumerable<DemoRequestVM>> dependencies,
        IDemoRequestRepository repository) : base(dependencies)
    {
        _repository = repository;
    }

    protected override async Task<ApiResponse<IEnumerable<DemoRequestVM>>> RunCore(
        GetAllDemoRequestsQuery request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        var customFilter = request.GetCustomFilterDictionary();

        var query = _repository.SearchIQueryable(request.Src, customFilter);

        // Ordenação
        query = ApplyOrdering(query, request.ColumnName, request.AscDesc);

        // Contagem total
        var totalCount = await query.CountAsync(cancellationToken);

        // Paginação
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var viewModels = items.Select(DemoRequestVM.FromDomain).ToList();

        var result = new PaginatedList<DemoRequestVM>(
            viewModels,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return result;
    }

    private static IQueryable<Domain.Entity.Core.DemoRequest> ApplyOrdering(
        IQueryable<Domain.Entity.Core.DemoRequest> query,
        string? columnName,
        int ascDesc)
    {
        var isDescending = ascDesc == -1;

        return columnName?.ToLower() switch
        {
            "fullname" => isDescending
                ? query.OrderByDescending(x => x.FullName)
                : query.OrderBy(x => x.FullName),
            "email" => isDescending
                ? query.OrderByDescending(x => x.Email)
                : query.OrderBy(x => x.Email),
            "companyname" => isDescending
                ? query.OrderByDescending(x => x.CompanyName)
                : query.OrderBy(x => x.CompanyName),
            "status" => isDescending
                ? query.OrderByDescending(x => x.Status)
                : query.OrderBy(x => x.Status),
            "createdat" => isDescending
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt),
            "contactedat" => isDescending
                ? query.OrderByDescending(x => x.ContactedAt)
                : query.OrderBy(x => x.ContactedAt),
            _ => query.OrderByDescending(x => x.CreatedAt) // Default: mais recentes primeiro
        };
    }
}
