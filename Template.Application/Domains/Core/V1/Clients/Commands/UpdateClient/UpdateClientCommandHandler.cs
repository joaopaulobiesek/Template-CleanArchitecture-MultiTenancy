using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Interfaces.Services;
using Template.Application.Common.Models;
using Template.Application.Domains.Core.V1.ViewModels;
using Template.Domain.Constants;

namespace Template.Application.Domains.Core.V1.Clients.Commands.UpdateClient;

public class UpdateClientCommandHandler : HandlerBase<UpdateClientCommand, ClientVM>
{
    private readonly IClientRepository _repository;
    private readonly ICorsOriginService _corsOriginService;
    private readonly ITenantCacheService _tenantCacheService;

    public UpdateClientCommandHandler(
        HandlerDependencies<UpdateClientCommand, ClientVM> dependencies,
        IClientRepository repository,
        ICorsOriginService corsOriginService,
        ITenantCacheService tenantCacheService) : base(dependencies)
    {
        _repository = repository;
        _corsOriginService = corsOriginService;
        _tenantCacheService = tenantCacheService;
    }

    protected override async Task<ApiResponse<ClientVM>> RunCore(UpdateClientCommand request, CancellationToken cancellationToken, object? additionalData = null)
    {
        var client = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (client == null)
            return new ErrorResponse<ClientVM>($"Cliente com o ID '{request.Id}' não encontrado.");

        // Por padrão, só pode editar clients associados ao seu UserId
        // Admin e TI podem editar qualquer client
        if (!_user.HasRole(Roles.Admin) && !_user.HasRole(Roles.TI) && client.UserId != _user.Id)
            return new ErrorResponse<ClientVM>("Acesso negado. Você só pode editar seus próprios clientes.", 403);

        // Guarda a URL anterior para verificar se mudou
        var previousUrl = client.Url;

        // Por padrão, preserva campos administrativos (não pode alterar)
        // Só Admin e TI podem alterar esses campos
        if (!_user.HasRole(Roles.Admin) && !_user.HasRole(Roles.TI))
        {
            request.Paid = client.Paid; // Mantém o valor original
            request.ConnectionString = null; // Não permite alterar
            request.StorageConfiguration = null; // Não permite alterar
            request.SendGridConfiguration = null; // Não permite alterar
            request.UserId = client.UserId; // Mantém o valor original
            request.Url = client.Url; // Usuário comum não pode alterar URL
        }

        client.UpdateClient(request);

        _repository.Update(client);

        await _context.SaveChangesAsync(cancellationToken);

        // Invalida todo o cache do tenant (ConnectionString, Storage, D4Sign, SendGrid, AzureSpeech, TimeZone, URL, AllowedIps, FeaturedExpo)
        _tenantCacheService.InvalidateTenantCache(client.Id);

        // Se a URL mudou (foi adicionada, alterada ou removida), invalida o cache de CORS
        if (!string.Equals(previousUrl, client.Url, StringComparison.OrdinalIgnoreCase))
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