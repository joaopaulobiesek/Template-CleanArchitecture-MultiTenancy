using FluentValidation;

namespace Template.Application.Domains.Core.V1.DemoRequests.Commands.CreateDemoRequest;

public class CreateDemoRequestCommandValidator : AbstractValidator<CreateDemoRequestCommand>
{
    public CreateDemoRequestCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Nome completo é obrigatório.")
            .MaximumLength(200).WithMessage("Nome completo deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .MaximumLength(200).WithMessage("E-mail deve ter no máximo 200 caracteres.")
            .EmailAddress().WithMessage("E-mail inválido.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefone é obrigatório.")
            .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres.");

        RuleFor(x => x.CompanyName)
            .MaximumLength(200).WithMessage("Nome da empresa deve ter no máximo 200 caracteres.");

        RuleFor(x => x.EventType)
            .MaximumLength(100).WithMessage("Tipo de evento deve ter no máximo 100 caracteres.");

        RuleFor(x => x.EstimatedAudience)
            .MaximumLength(100).WithMessage("Estimativa de público deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Message)
            .MaximumLength(2000).WithMessage("Mensagem deve ter no máximo 2000 caracteres.");
    }
}
