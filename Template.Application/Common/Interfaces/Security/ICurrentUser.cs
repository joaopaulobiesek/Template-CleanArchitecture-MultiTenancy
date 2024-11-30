namespace Template.Application.Common.Interfaces.Security;

public interface ICurrentUser
{
    string? Id { get; }
    Guid X_Tenant_ID { get; }
}