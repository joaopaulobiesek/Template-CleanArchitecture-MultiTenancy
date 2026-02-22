using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Models;
using Template.Application.Common.Security;
using Template.Application.Domains.Core.V1.ViewModels;
using Template.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using static Template.Domain.Constants.Policies;

namespace Template.Application.Domains.Core.V1.Clients.Queries.GetSimple;

[Authorize(Roles = $"{Roles.Admin},{Roles.TI},{Roles.User}")]
[Authorize(Policy = $"{CanList},{CanView}")]
public class GetClientsSimpleQuery
{
    public string? Src { get; set; }
}

public class GetClientsSimpleQueryHandler : HandlerBase<GetClientsSimpleQuery, IEnumerable<ClientSimpleVM>>
{
    private readonly IClientRepository _repository;

    public GetClientsSimpleQueryHandler(
        HandlerDependencies<GetClientsSimpleQuery, IEnumerable<ClientSimpleVM>> dependencies,
        IClientRepository repository) : base(dependencies)
    {
        _repository = repository;
    }

    protected override async Task<ApiResponse<IEnumerable<ClientSimpleVM>>> RunCore(
        GetClientsSimpleQuery request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        var query = _repository.SearchIQueryable(null, null)
            .Where(x => x.Active);

        // Por padrão filtra apenas clients do usuário (segurança)
        // Só Admin e TI veem todos os clients
        if (!_user.HasRole(Roles.Admin) && !_user.HasRole(Roles.TI))
        {
            query = query.Where(x => x.UserId == _user.Id);
        }

        // Filtrar por src se informado (busca em FullName, DocumentNumber e Email)
        if (!string.IsNullOrWhiteSpace(request.Src))
        {
            var searchTerm = request.Src.ToLower();
            query = query.Where(x =>
                x.FullName.ToLower().Contains(searchTerm) ||
                x.DocumentNumber.ToLower().Contains(searchTerm) ||
                x.Email.ToLower().Contains(searchTerm));
        }

        var clients = await query
            .OrderBy(x => x.FullName)
            .Select(x => new ClientSimpleVM(x.Id, x.FullName + " - " + x.DocumentNumber))
            .ToListAsync(cancellationToken);

        return new SuccessResponse<IEnumerable<ClientSimpleVM>>(
            "Clientes obtidos com sucesso.",
            clients
        );
    }
}
