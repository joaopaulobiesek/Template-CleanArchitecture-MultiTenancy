using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Models;
using Template.Application.Common.Security;
using Template.Application.Domains.Core.V1.ViewModels;
using Template.Domain.Constants;
using static Template.Domain.Constants.Policies;

namespace Template.Application.Domains.Core.V1.Clients.Queries.GetById;

[Authorize(Roles = $"{Roles.Admin},{Roles.TI},{Roles.User}")]
[Authorize(Policy = $"{CanList},{CanView}")]
public class GetByIdQuery
{
    public Guid Id { get; set; }
}

public class GetByIdQueryHandler : HandlerBase<GetByIdQuery, ClientDetailVM>
{
    private readonly IClientRepository _repository;

    public GetByIdQueryHandler(HandlerDependencies<GetByIdQuery, ClientDetailVM> dependencies, IClientRepository repository) : base(dependencies)
    {
        _repository = repository;
    }

    protected override async Task<ApiResponse<ClientDetailVM>> RunCore(GetByIdQuery request, CancellationToken cancellationToken, object? additionalData = null)
    {
        var client = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (client == null)
            return new ErrorResponse<ClientDetailVM>($"Cliente com o ID '{request.Id}' não encontrado.", 404);

        // Por padrão só pode ver clients associados ao seu UserId (segurança)
        // Só Admin e TI veem qualquer client
        if (!_user.HasRole(Roles.Admin) && !_user.HasRole(Roles.TI) && client.UserId != _user.Id)
            return new ErrorResponse<ClientDetailVM>($"Acesso negado. Você só pode visualizar seus próprios clientes.", 403);

        // Só Admin e TI veem dados sensíveis - por padrão vem null (segurança)
        var isAdminOrTI = _user.HasRole(Roles.Admin) || _user.HasRole(Roles.TI);
        var viewModel = ClientDetailVM.FromEntity(client, includeSensitiveData: isAdminOrTI);

        return new SuccessResponse<ClientDetailVM>("Consulta realizada com sucesso.", viewModel);
    }
}