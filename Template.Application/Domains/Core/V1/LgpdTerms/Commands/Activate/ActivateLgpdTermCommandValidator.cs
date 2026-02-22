using FluentValidation;

namespace Template.Application.Domains.Core.V1.LgpdTerms.Commands.Activate;

public class ActivateLgpdTermCommandValidator : AbstractValidator<ActivateLgpdTermCommand>
{
    public ActivateLgpdTermCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID do termo é obrigatório.");
    }
}
