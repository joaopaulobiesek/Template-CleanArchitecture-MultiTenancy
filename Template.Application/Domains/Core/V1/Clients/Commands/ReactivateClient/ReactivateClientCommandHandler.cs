using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Models;
using Template.Application.Common.Security;
using Template.Domain.Constants;

namespace Template.Application.Domains.Core.V1.Clients.Commands.ReactivateClient;

[Authorize(Roles = $"{Roles.Admin},{Roles.TI}")]
[Authorize(Policy = Policies.CanEdit)]
public class ReactivateClientCommand
{
    public Guid Id { get; set; }
}

public class ReactivateClientCommandHandler : HandlerBase<ReactivateClientCommand, string>
{
    private readonly IClientRepository _repository;

    public ReactivateClientCommandHandler(HandlerDependencies<ReactivateClientCommand, string> dependencies, IClientRepository repository) : base(dependencies)
    {
        _repository = repository;
    }

    protected override async Task<ApiResponse<string>> RunCore(ReactivateClientCommand request, CancellationToken cancellationToken, object? additionalData = null)
    {
        var client = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (client == null)
            return new ErrorResponse<string>($"Cliente com o ID '{request.Id}' não encontrado.");

        client.Activate();

        await _context.SaveChangesAsync(cancellationToken);

        return new SuccessResponse<string>($"Cliente com o ID '{request.Id}' foi reativado com sucesso!");
    }
}
