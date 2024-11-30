﻿using FluentValidation;

namespace Template.Application.Domains.V1.Identity.Users.Commands.EditUsers;

public class EditUserCommandValidator : AbstractValidator<EditUserCommand>
{
    public EditUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.");
    }
}