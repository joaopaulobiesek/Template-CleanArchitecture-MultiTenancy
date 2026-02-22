using Template.Domain.Entity.Core;
using FluentValidation;

namespace Template.Application.Domains.Core.V1.DemoRequests.Commands.UpdateDemoRequestStatus;

public class UpdateDemoRequestStatusCommandValidator : AbstractValidator<UpdateDemoRequestStatusCommand>
{
    public UpdateDemoRequestStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID é obrigatório.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status é obrigatório.")
            .Must(status => DemoRequestStatus.GetAll().Contains(status))
            .WithMessage($"Status deve ser um dos valores: {string.Join(", ", DemoRequestStatus.GetAll())}");

        RuleFor(x => x.AdminNotes)
            .MaximumLength(2000).WithMessage("Observações devem ter no máximo 2000 caracteres.");
    }
}
