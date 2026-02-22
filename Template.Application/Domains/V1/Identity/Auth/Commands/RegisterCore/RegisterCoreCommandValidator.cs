using FluentValidation;

namespace Template.Application.Domains.V1.Identity.Auth.Commands.RegisterCore;

public class RegisterCoreCommandValidator : AbstractValidator<RegisterCoreCommand>
{
    public RegisterCoreCommandValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Nome completo é obrigatório.")
            .MaximumLength(100).WithMessage("Nome pode ter no máximo 100 caracteres.");

        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("CPF/CNPJ é obrigatório.")
            .MaximumLength(20).WithMessage("CPF/CNPJ pode ter no máximo 20 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.")
            .MaximumLength(254).WithMessage("Email pode ter no máximo 254 caracteres.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Telefone pode ter no máximo 20 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}
