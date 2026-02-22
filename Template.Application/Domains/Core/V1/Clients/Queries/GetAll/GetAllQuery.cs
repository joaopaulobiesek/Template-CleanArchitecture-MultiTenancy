using System.Linq.Expressions;
using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Models;
using Template.Application.Common.Security;
using Template.Application.Domains.Core.V1.ViewModels;
using Template.Domain.Constants;
using Template.Domain.Entity.Core;
using static Template.Domain.Constants.Policies;

namespace Template.Application.Domains.Core.V1.Clients.Queries.GetAll;

[Authorize(Roles = $"{Roles.Admin},{Roles.TI},{Roles.User}")]
[Authorize(Policy = $"{CanList},{CanView}")]
public class GetAllQuery : BasePaginatedQuery
{
}

public class GetAllQueryHandler : HandlerBase<GetAllQuery, IEnumerable<ClientVM>>
{
    private readonly IClientRepository _repository;

    public GetAllQueryHandler(HandlerDependencies<GetAllQuery, IEnumerable<ClientVM>> dependencies, IClientRepository repository) : base(dependencies)
    {
        _repository = repository;
    }

    protected override async Task<ApiResponse<IEnumerable<ClientVM>>> RunCore(GetAllQuery request, CancellationToken cancellationToken, object? additionalData = null)
    {
        var query = _repository.SearchIQueryable(request.Src!, request.GetCustomFilterDictionary());

        // Por padrão filtra apenas clients do usuário (segurança)
        // Só Admin e TI veem todos os clients
        if (!_user.HasRole(Roles.Admin) && !_user.HasRole(Roles.TI))
        {
            query = query.Where(x => x.UserId == _user.Id);
        }

        query = request.AscDesc == -1
            ? query.OrderByDescending(BuscarOrdemPropriedade(request.ColumnName))
            : query.OrderBy(BuscarOrdemPropriedade(request.ColumnName));

        var list = await PaginatedList<Client>.CreateAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        var newList = list.Data?.Select(x => new ClientVM(x.Id, x.FullName, x.DocumentNumber, x.Phone, x.Paid, x.Active)).ToList();

        var result = new PaginatedList<ClientVM>(
                newList,
                list.TotalItens,
                list.PageNumber,
                request.PageSize
            );

        return result;
    }

    private static Expression<Func<Client, object>> BuscarOrdemPropriedade(string? param)
     => param?.ToLower() switch
     {
         "document" => Client => Client.DocumentNumber,
         _ => Client => Client.FullName
     };
}