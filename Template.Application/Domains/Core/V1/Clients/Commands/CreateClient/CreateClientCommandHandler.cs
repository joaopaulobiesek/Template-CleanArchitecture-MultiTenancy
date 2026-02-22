using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Interfaces.Services;
using Template.Application.Common.Models;
using Template.Application.Domains.Core.V1.ViewModels;
using Template.Domain;
using Template.Domain.Entity.Core;

namespace Template.Application.Domains.Core.V1.Clients.Commands.CreateClient;

public class CreateClientCommandHandler : HandlerBase<CreateClientCommand, ClientVM>
{
    private readonly IAzureStorage _storage;
    private readonly IClientRepository _repository;
    private readonly ICorsOriginService _corsOriginService;

    public CreateClientCommandHandler(
        HandlerDependencies<CreateClientCommand, ClientVM> dependencies,
        IAzureStorage storage,
        IClientRepository repository,
        ICorsOriginService corsOriginService) : base(dependencies)
    {
        _storage = storage;
        _repository = repository;
        _corsOriginService = corsOriginService;
    }

    protected override async Task<ApiResponse<ClientVM>> RunCore(CreateClientCommand request, CancellationToken cancellationToken, object? additionalData = null)
    {
        var checkClient = await _context.Clients.FirstOrDefaultAsync(x =>
                    x.DocumentNumber.Replace(".", "").Replace("/", "").Replace("-", "")
                    .Contains(StringFormatter.RemoveNonNumericCharacters(request.DocumentNumber)),
                cancellationToken);

        if (checkClient != null)
            return new ErrorResponse<ClientVM>("Já existe um cliente cadastrado com este número de documento.");

        var client = new Client();

        client.CreateClient(request);

        await _repository.AddAsync(client, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        // Se o cliente tem URL, invalida o cache de CORS para incluir a nova origem
        if (!string.IsNullOrWhiteSpace(client.Url))
        {
            _corsOriginService.InvalidateCache();
        }

        return new SuccessResponse<ClientVM>(
            "Cadastro efetuado com sucesso.",
            new ClientVM(
                client.Id,
                client.FullName,
                client.DocumentNumber,
                client.Phone,
                client.Paid,
                client.Active
            )
        );
    }
}