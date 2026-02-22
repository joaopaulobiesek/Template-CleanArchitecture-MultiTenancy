using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Models;
using Template.Application.Domains.Core.V1.ViewModels;

namespace Template.Application.Domains.Core.V1.LgpdTerms.Commands.Update;

public class UpdateLgpdTermCommandHandler : HandlerBase<UpdateLgpdTermCommand, LgpdTermVM>
{
    private readonly ILgpdTermRepository _repository;

    public UpdateLgpdTermCommandHandler(
        HandlerDependencies<UpdateLgpdTermCommand, LgpdTermVM> dependencies,
        ILgpdTermRepository repository) : base(dependencies)
    {
        _repository = repository;
    }

    protected override async Task<ApiResponse<LgpdTermVM>> RunCore(
        UpdateLgpdTermCommand request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        // Busca termo
        var term = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (term == null)
        {
            return new ErrorResponse<LgpdTermVM>("Termo não encontrado.", 404);
        }

        // Valida se pelo menos um conteúdo foi informado
        if (string.IsNullOrWhiteSpace(request.TermsOfUseContent) &&
            string.IsNullOrWhiteSpace(request.PrivacyPolicyContent))
        {
            return new ErrorResponse<LgpdTermVM>(
                "Informe pelo menos um conteúdo para atualizar (Termo de Uso ou Política de Privacidade).",
                400);
        }

        // Atualiza Termo de Uso se informado (validação de termo ativo está no domínio)
        if (!string.IsNullOrWhiteSpace(request.TermsOfUseContent))
        {
            term.UpdateTermsOfUse(request.TermsOfUseContent);
        }

        // Atualiza Política de Privacidade se informado
        if (!string.IsNullOrWhiteSpace(request.PrivacyPolicyContent))
        {
            term.UpdatePrivacyPolicy(request.PrivacyPolicyContent);
        }

        // Salva via repository
        await _repository.UpdateAsync(term, cancellationToken);

        return new SuccessResponse<LgpdTermVM>(
            "Termo LGPD atualizado com sucesso.",
            LgpdTermVM.FromDomain(term)
        );
    }
}
