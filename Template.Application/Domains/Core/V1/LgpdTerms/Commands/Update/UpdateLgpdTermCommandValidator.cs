using FluentValidation;

namespace Template.Application.Domains.Core.V1.LgpdTerms.Commands.Update;

public class UpdateLgpdTermCommandValidator : AbstractValidator<UpdateLgpdTermCommand>
{
    public UpdateLgpdTermCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID do termo é obrigatório.");

        // Termo de Uso - valida somente se informado
        RuleFor(x => x.TermsOfUseContent)
            .MinimumLength(100).WithMessage("Conteúdo do Termo de Uso deve ter pelo menos 100 caracteres.")
            .MaximumLength(50000).WithMessage("Conteúdo do Termo de Uso não pode exceder 50.000 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.TermsOfUseContent));

        // Política de Privacidade - valida somente se informado
        RuleFor(x => x.PrivacyPolicyContent)
            .MinimumLength(100).WithMessage("Conteúdo da Política de Privacidade deve ter pelo menos 100 caracteres.")
            .MaximumLength(50000).WithMessage("Conteúdo da Política de Privacidade não pode exceder 50.000 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.PrivacyPolicyContent));
    }
}
