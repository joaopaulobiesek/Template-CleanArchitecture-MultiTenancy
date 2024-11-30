using Template.Domain.Entity.Tenant;

namespace Template.Application.Common.Persistence;

public interface ITenantContext
{
    DbSet<Client> Clients { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}