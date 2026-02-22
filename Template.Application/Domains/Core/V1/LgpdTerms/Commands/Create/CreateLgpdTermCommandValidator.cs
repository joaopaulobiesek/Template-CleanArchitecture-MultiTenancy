using FluentValidation;

namespace Template.Application.Domains.Core.V1.LgpdTerms.Commands.Create;

public class CreateLgpdTermCommandValidator : AbstractValidator<CreateLgpdTermCommand>
{
    public CreateLgpdTermCommandValidator()
    {
        RuleFor(x => x.Version)
            .NotEmpty().WithMessage("Versão do termo é obrigatória.")
            .Matches(@"^\d+\.\d+(\.\d+)?$").WithMessage("Versão deve estar no formato X.Y ou X.Y.Z (ex: 1.0 ou 2.1.3)");

        RuleFor(x => x.TermsOfUseContent)
            .NotEmpty().WithMessage("Conteúdo do Termo de Uso é obrigatório.")
            .MinimumLength(100).WithMessage("Conteúdo do Termo de Uso deve ter pelo menos 100 caracteres.")
            .MaximumLength(50000).WithMessage("Conteúdo do Termo de Uso não pode exceder 50.000 caracteres.");

        RuleFor(x => x.PrivacyPolicyContent)
            .NotEmpty().WithMessage("Conteúdo da Política de Privacidade é obrigatório.")
            .MinimumLength(100).WithMessage("Conteúdo da Política de Privacidade deve ter pelo menos 100 caracteres.")
            .MaximumLength(50000).WithMessage("Conteúdo da Política de Privacidade não pode exceder 50.000 caracteres.");
    }
}
