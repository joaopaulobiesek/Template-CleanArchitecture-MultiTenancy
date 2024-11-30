﻿using Template.Application.Common.Behaviours;
using Template.Application.Common.Models;
using Template.Application.Domains.Tenant.V1.ViewModels;
using Template.Domain;
using Template.Domain.Entity.Tenant;

namespace Template.Application.Domains.Tenant.V1.Clients.Commands.CreateClient;

public class CreateClientCommandHandler : HandlerBase<CreateClientCommand, ClientVM>
{
    public CreateClientCommandHandler(HandlerDependencies<CreateClientCommand, ClientVM> dependencies) : base(dependencies) { }

    protected override async Task<ApiResponse<ClientVM>> RunCore(CreateClientCommand request, CancellationToken cancellationToken, object? additionalData = null)
    {
        var checkClient = await _context.Clients.FirstOrDefaultAsync(x =>
                    x.DocumentNumber.Replace(".", "").Replace("/", "").Replace("-", "")
                    .Contains(StringFormatter.RemoveNonNumericCharacters(request.DocumentNumber)),
                cancellationToken);

        if (checkClient != null)
            return new ErrorResponse<ClientVM>("Document Number already exists");

        var client = new Client();

        client.CreateClient(request);

        await _tenantContext.Clients.AddAsync(client);

        await _tenantContext.SaveChangesAsync(cancellationToken);

        return new SuccessResponse<ClientVM>(
            "Cadastro efetuado com sucesso.",
            new ClientVM(
                client.Id,
                client.FullName,
                client.DocumentNumber,
                client.Phone,
                client.ZipCode,
                client.Paid
            )
        );
    }
}