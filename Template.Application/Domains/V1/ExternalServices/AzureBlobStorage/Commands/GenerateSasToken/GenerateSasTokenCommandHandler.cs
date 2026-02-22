using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.Services;
using Template.Application.Common.Models;

namespace Template.Application.Domains.V1.ExternalServices.AzureBlobStorage.Commands.GenerateSasToken;

public class GenerateSasTokenCommandHandler : HandlerBase<GenerateSasTokenCommand, string>
{
    private readonly IAzureStorage _storage;

    public GenerateSasTokenCommandHandler(HandlerDependencies<GenerateSasTokenCommand, string> dependencies, IAzureStorage storage) : base(dependencies)
    {
        _storage = storage;
    }

    protected override async Task<ApiResponse<string>> RunCore(GenerateSasTokenCommand request, CancellationToken cancellationToken, object? additionalData = null)
    {
        //TODO: Implementar a variação de tempo conforme o tamanho do arquivo.

        // Gera nome único com GUID para evitar colisão de nomes
        var extension = Path.GetExtension(request.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";

        var result = _storage.GenerateSasToken(uniqueFileName);

        return new SuccessResponse<string>("Token Gerado com Sucesso!", result);
    }
}