using Template.Domain.Entity.Core;

namespace Template.Application.Domains.Core.V1.ViewModels;

public class ClientSimpleVM
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; }

    public ClientSimpleVM() { }

    public ClientSimpleVM(Guid id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }

    public static ClientSimpleVM FromDomain(Client? entity)
    {
        if (entity == null) return new ClientSimpleVM();

        return new ClientSimpleVM(
            entity.Id,
            $"{entity.FullName} - {entity.DocumentNumber}"
        );
    }
}
