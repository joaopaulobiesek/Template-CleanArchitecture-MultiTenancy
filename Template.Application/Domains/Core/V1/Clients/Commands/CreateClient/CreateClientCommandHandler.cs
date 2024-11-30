using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Models;
using Template.Application.Domains.Core.V1.ViewModels;
using Template.Domain;
using Template.Domain.Entity.Core;

namespace Template.Application.Domains.Core.V1.Clients.Commands.CreateClient;

public class CreateClientCommandHandler : HandlerBase<CreateClientCommand, ClientVM>
{
    private readonly IClientRepository _repository;
    public CreateClientCommandHandler(HandlerDependencies<CreateClientCommand, ClientVM> dependencies, IClientRepository repository) : base(dependencies)
    {
        _repository = repository;
    }

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

        // Em casos simples, você pode utilizar diretamente o contexto (_context)
        // para adicionar o cliente à coleção DbSet, conforme demonstrado abaixo.
        // Isso é útil quando você precisa apenas de operações básicas e diretas no banco de dados.
        await _context.Clients.AddAsync(client);

        // Em cenários mais complexos, onde é necessário encapsular a lógica de acesso a dados,
        // recomenda-se o uso do repositório (_repository) para maior flexibilidade.
        // Isso permite centralizar consultas ou comandos complexos dentro do repositório,
        // facilitando a manutenção, testes e reutilização da lógica de dados em outras partes da aplicação.
        //
        // Observação: descomente a linha abaixo para usar o repositório e comente a linha de _context
        // caso precise de uma abordagem mais robusta para gerenciar operações de dados em clientes.

        //await _repository.AddAsync(client, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new SuccessResponse<ClientVM>(
            "Cadastro efetuado com sucesso.",
            new ClientVM(
                client.Id,
                client.FullName,
                client.DocumentNumber,
                client.Phone,
                client.ZipCode
            )
        );
    }
}