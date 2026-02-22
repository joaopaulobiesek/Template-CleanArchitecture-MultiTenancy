using Template.Domain.Entity.Core;

namespace Template.Application.Domains.Core.V1.ViewModels;

public class ClientVM
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string DocumentNumber { get; set; }
    public string? Phone { get; set; }
    public bool Paid { get; set; }
    public bool Active { get; set; }

    public ClientVM() { }

    public ClientVM(Guid id, string fullName, string documentNumber, string? phone, bool paid, bool active)
    {
        Id = id;
        FullName = fullName;
        DocumentNumber = documentNumber;
        Phone = phone;
        Paid = paid;
        Active = active;
    }

    public static ClientVM FromEntity(Client client)
    => new ClientVM
    {
        Id = client.Id,
        FullName = client.FullName,
        DocumentNumber = client.DocumentNumber,
        Phone = client.Phone,
        Paid = client.Paid,
        Active = client.Active
    };

}