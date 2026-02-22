using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Models;
using Template.Application.Domains.Core.V1.ViewModels;

namespace Template.Application.Domains.Core.V1.LgpdTerms.Queries.GetActiveTerm;

// Endpoint público - não requer autenticação (usuário precisa ver termo antes de aceitar)
public class GetActiveLgpdTermQuery { }

public class GetActiveLgpdTermQueryHandler : HandlerBase<GetActiveLgpdTermQuery, LgpdTermVM>
{
    private readonly ILgpdTermRepository _repository;

    public GetActiveLgpdTermQueryHandler(
        HandlerDependencies<GetActiveLgpdTermQuery, LgpdTermVM> dependencies,
        ILgpdTermRepository repository) : base(dependencies)
    {
        _repository = repository;
    }

    protected override async Task<ApiResponse<LgpdTermVM>> RunCore(
        GetActiveLgpdTermQuery request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        var term = await _repository.GetActiveAsync(cancellationToken);
        if (term == null)
        {
            return new ErrorResponse<LgpdTermVM>("Nenhum termo ativo encontrado.", 404);
        }

        return new SuccessResponse<LgpdTermVM>(
            "Termo ativo encontrado.",
            LgpdTermVM.FromDomain(term)
        );
    }
}
