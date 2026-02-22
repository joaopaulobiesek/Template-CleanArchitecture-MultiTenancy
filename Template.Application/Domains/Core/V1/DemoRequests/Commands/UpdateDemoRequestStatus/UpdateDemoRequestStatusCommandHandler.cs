using Template.Application.Common.Behaviours;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Models;
using Template.Application.Domains.Core.V1.ViewModels.DemoRequests;
using Template.Domain.Entity.Core;

namespace Template.Application.Domains.Core.V1.DemoRequests.Commands.UpdateDemoRequestStatus;

public class UpdateDemoRequestStatusCommandHandler : HandlerBase<UpdateDemoRequestStatusCommand, DemoRequestVM>
{
    private readonly IDemoRequestRepository _repository;

    public UpdateDemoRequestStatusCommandHandler(
        HandlerDependencies<UpdateDemoRequestStatusCommand, DemoRequestVM> dependencies,
        IDemoRequestRepository repository) : base(dependencies)
    {
        _repository = repository;
    }

    protected override async Task<ApiResponse<DemoRequestVM>> RunCore(
        UpdateDemoRequestStatusCommand request,
        CancellationToken cancellationToken,
        object? additionalData = null)
    {
        var demoRequest = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (demoRequest == null)
            return new ErrorResponse<DemoRequestVM>($"Solicitação com ID '{request.Id}' não encontrada.", 404);

        // Atualiza status conforme solicitado
        switch (request.Status)
        {
            case DemoRequestStatus.Contacted:
                demoRequest.MarkAsContacted(request.AdminNotes);
                break;
            case DemoRequestStatus.Converted:
                demoRequest.MarkAsConverted(request.AdminNotes);
                break;
            case DemoRequestStatus.Rejected:
                demoRequest.MarkAsRejected(request.AdminNotes);
                break;
            case DemoRequestStatus.Pending:
                // Volta para pendente (caso queira reverter)
                demoRequest.UpdateAdminNotes(request.AdminNotes);
                break;
            default:
                return new ErrorResponse<DemoRequestVM>($"Status '{request.Status}' inválido.", 400);
        }

        await _repository.UpdateAsync(demoRequest, cancellationToken);

        return new SuccessResponse<DemoRequestVM>(
            "Status atualizado com sucesso.",
            DemoRequestVM.FromDomain(demoRequest));
    }
}
