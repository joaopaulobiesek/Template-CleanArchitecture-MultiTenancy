using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Models;
using Template.Application.Domains.Core.V1.ViewModels;
using Template.Domain.Entity.Core;

namespace Template.Application.Domains.Core.V1.LgpdTerms.Commands.Create;

public class CreateLgpdTermCommandHandler : HandlerBase<CreateLgpdTermCommand, LgpdTermVM>
{
    private readonly ILgpdTermRepository _repository;

    public CreateLgpdTermCommandHandler(
        HandlerDependencies<CreateLgpdTermCommand, LgpdTermVM> dependencies,
        ILgpdTermRepository repository) : base(dependencies)
    {
        _repository = repository;
    }

    protected override async Task<ApiResponse<LgpdTermVM>> RunCore(
        CreateLgpdTermCommand request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        if (string.IsNullOrEmpty(_user.Id))
        {
            return new ErrorResponse<LgpdTermVM>("Usuário não autenticado.", 401);
        }

        var userId = Guid.Parse(_user.Id);

        // Valida se versão já existe
        var versionExists = await _repository.ExistsVersionAsync(request.Version, cancellationToken);
        if (versionExists)
        {
            return new ErrorResponse<LgpdTermVM>("Já existe um termo com esta versão.", 400);
        }

        // Cria termo
        var term = LgpdTerm.Create(
            request.Version,
            request.TermsOfUseContent,
            request.PrivacyPolicyContent,
            userId);

        // Salva via repository
        await _repository.CreateAsync(term, cancellationToken);

        return new SuccessResponse<LgpdTermVM>(
            "Termo LGPD criado com sucesso.",
            LgpdTermVM.FromDomain(term)
        );
    }
}
