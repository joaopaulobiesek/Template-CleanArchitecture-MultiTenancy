using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Interfaces.Services;
using Template.Application.Common.Models;
using Template.Application.Domains.Core.V1.ViewModels.DemoRequests;
using Template.Domain.Entity.Core;

namespace Template.Application.Domains.Core.V1.DemoRequests.Commands.CreateDemoRequest;

public class CreateDemoRequestCommandHandler : HandlerBase<CreateDemoRequestCommand, DemoRequestVM>
{
    private readonly IDemoRequestRepository _repository;
    private readonly IEmailSystemService _emailSystemService;

    public CreateDemoRequestCommandHandler(
        HandlerDependencies<CreateDemoRequestCommand, DemoRequestVM> dependencies,
        IDemoRequestRepository repository,
        IEmailSystemService emailSystemService) : base(dependencies)
    {
        _repository = repository;
        _emailSystemService = emailSystemService;
    }

    protected override async Task<ApiResponse<DemoRequestVM>> RunCore(
        CreateDemoRequestCommand request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        var demoRequest = new DemoRequest();

        demoRequest.Create(
            request.FullName,
            request.Email,
            request.Phone,
            request.CompanyName,
            request.EventType,
            request.EstimatedAudience,
            request.Message);

        await _repository.CreateAsync(demoRequest, cancellationToken);

        // Envia e-mail de notificação para o admin (não bloqueia se falhar)
        await SendDemoRequestNotificationAsync(request, cancellationToken);

        return new SuccessResponse<DemoRequestVM>(
            "Solicitação de demonstração enviada com sucesso! Entraremos em contato em breve.",
            DemoRequestVM.FromDomain(demoRequest));
    }

    /// <summary>
    /// Envia e-mail de notificação de solicitação de demonstração para o admin.
    /// </summary>
    private async Task SendDemoRequestNotificationAsync(
        CreateDemoRequestCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var notification = new DemoRequestNotification
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                CompanyName = request.CompanyName,
                EventType = request.EventType,
                EstimatedAudience = request.EstimatedAudience,
                Message = request.Message,
                RequestedAt = DateTime.UtcNow
            };

            await _emailSystemService.SendDemoRequestNotificationAsync(notification, cancellationToken);
        }
        catch
        {
            // Não falha a operação se o e-mail não for enviado
            // O log de erro já é feito no EmailSystemService
        }
    }
}
