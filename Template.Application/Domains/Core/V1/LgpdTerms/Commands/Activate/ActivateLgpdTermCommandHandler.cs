using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Models;
using Template.Application.Domains.Core.V1.ViewModels;

namespace Template.Application.Domains.Core.V1.LgpdTerms.Commands.Activate;

public class ActivateLgpdTermCommandHandler : HandlerBase<ActivateLgpdTermCommand, LgpdTermVM>
{
    private readonly ILgpdTermRepository _repository;

    public ActivateLgpdTermCommandHandler(
        HandlerDependencies<ActivateLgpdTermCommand, LgpdTermVM> dependencies,
        ILgpdTermRepository repository) : base(dependencies)
    {
        _repository = repository;
    }

    protected override async Task<ApiResponse<LgpdTermVM>> RunCore(
        ActivateLgpdTermCommand request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        // Busca termo
        var term = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (term == null)
        {
            return new ErrorResponse<LgpdTermVM>("Termo não encontrado.", 404);
        }

        if (term.IsActive)
        {
            return new ErrorResponse<LgpdTermVM>("Termo já está ativo.", 400);
        }

        // Desativa todos os outros termos ativos (regra: apenas 1 pode estar ativo)
        await _repository.DeactivateAllAsync(cancellationToken);

        // Ativa o termo solicitado
        term.Activate();

        // Salva via repository
        await _repository.UpdateAsync(term, cancellationToken);

        return new SuccessResponse<LgpdTermVM>(
            "Termo LGPD ativado com sucesso.",
            LgpdTermVM.FromDomain(term)
        );
    }
}
