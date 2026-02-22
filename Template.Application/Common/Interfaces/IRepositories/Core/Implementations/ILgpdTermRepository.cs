using Template.Application.Common.Interfaces.IRepositories.Core.Base;
using Template.Domain.Entity.Core;

namespace Template.Application.Common.Interfaces.IRepositories.Core.Implementations;

public interface ILgpdTermRepository : IRepository<LgpdTerm>
{
    IQueryable<LgpdTerm> SearchIQueryable(string? src, Dictionary<string, string>? customFilter = null);
    Task<LgpdTerm?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LgpdTerm?> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsVersionAsync(string version, CancellationToken cancellationToken = default);
    Task CreateAsync(LgpdTerm entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(LgpdTerm entity, CancellationToken cancellationToken = default);
    Task DeactivateAllAsync(CancellationToken cancellationToken = default);
}
