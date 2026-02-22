using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Interfaces.Services;
using Template.Application.Common.Models;
using Template.Application.Domains.V1.ViewModels.Users;
using Template.Domain.Constants;
using Template.Domain.Entity.Core;

namespace Template.Application.Domains.V1.Identity.Auth.Commands.RegisterCore;

public class RegisterCoreCommandHandler : HandlerBase<RegisterCoreCommand, RegisterCoreResultVm>
{
    private readonly IIdentityService _identityService;
    private readonly IClientRepository _repositoryClient;
    private readonly IEmailSystemService _emailSystemService;

    public RegisterCoreCommandHandler(
        HandlerDependencies<RegisterCoreCommand, RegisterCoreResultVm> dependencies,
        IClientRepository repositoryClient,
        IEmailSystemService emailSystemService) : base(dependencies)
    {
        _identityService = dependencies.IdentityService;
        _repositoryClient = repositoryClient;
        _emailSystemService = emailSystemService;
    }

    protected override async Task<ApiResponse<RegisterCoreResultVm>> RunCore(
        RegisterCoreCommand request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        // 0. Verifica se e-mail já existe
        var existingUser = await _identityService.GetUserByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new ErrorResponse<RegisterCoreResultVm>("Este e-mail já está cadastrado no sistema.", 400);
        }

        // 1. Cria o usuário
        var newUser = new UserVm
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.Phone,
            Roles = new List<string> { Roles.User },
            Policies = new List<string> { Policies.CanList, Policies.CanView, Policies.CanCreate, Policies.CanEdit, Policies.CanDelete, Policies.CanViewReports }
        };

        var createdUser = await _identityService.CreateUserAsync(newUser, request.Password);

        if (string.IsNullOrEmpty(createdUser.Id))
        {
            return new ErrorResponse<RegisterCoreResultVm>(
                "Falha ao criar usuário. Verifique se a senha atende os requisitos: mínimo 8 caracteres, uma letra maiúscula, uma minúscula, um número e um caractere especial.",
                400);
        }

        // 2. Cria o Client associado ao usuário
        var client = new Client();
        client.RegisterClient(request, createdUser.Id);
        await _repositoryClient.CreateAsync(client, cancellationToken);

        // 3. Gera token de confirmação de e-mail
        var confirmationToken = await _identityService.GenerateEmailConfirmationTokenAsync(createdUser.Id);

        if (string.IsNullOrEmpty(confirmationToken))
        {
            return new ErrorResponse<RegisterCoreResultVm>("Falha ao gerar token de confirmação.", 500);
        }

        // 4. Monta URL de confirmação
        var systemUrl = "https://admin.Template.cc";
        var tokenBytes = System.Text.Encoding.UTF8.GetBytes(confirmationToken);
        var base64Token = Convert.ToBase64String(tokenBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
        var confirmationUrl = $"{systemUrl}/confirm-email?userId={createdUser.Id}&token={base64Token}";

        // 5. Envia e-mail de confirmação
        var emailNotification = new EmailConfirmationNotification
        {
            UserEmail = request.Email,
            UserName = request.FullName,
            ConfirmationUrl = confirmationUrl,
            RequestedAt = DateTime.UtcNow
        };

        await _emailSystemService.SendEmailConfirmationAsync(emailNotification, cancellationToken);

        var result = new RegisterCoreResultVm(
            request.Email,
            request.FullName,
            client.Id,
            true
        );

        return new SuccessResponse<RegisterCoreResultVm>(
            "Usuário registrado com sucesso! Um e-mail de confirmação foi enviado.",
            result);
    }
}
