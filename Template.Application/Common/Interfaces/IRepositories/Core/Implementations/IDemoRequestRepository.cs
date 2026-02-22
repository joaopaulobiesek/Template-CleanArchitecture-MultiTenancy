using Template.Application.Common.Interfaces.IRepositories.Core.Base;
using Template.Domain.Entity.Core;

namespace Template.Application.Common.Interfaces.IRepositories.Core.Implementations;

public interface IDemoRequestRepository : IRepository<DemoRequest>
{
    IQueryable<DemoRequest> SearchIQueryable(string? src, Dictionary<string, string>? customFilter = null);
    Task<DemoRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(DemoRequest entity, CancellationToken cancellationToken);
    Task UpdateAsync(DemoRequest entity, CancellationToken cancellationToken);
}
