using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Domain.Entity.Core;
using Template.Infra.Persistence.Contexts.Core;
using Template.Infra.Persistence.Repositories.Core.Base;
using Microsoft.EntityFrameworkCore;

namespace Template.Infra.Persistence.Repositories.Core.Implementations;

public class DemoRequestRepository : Repository<DemoRequest>, IDemoRequestRepository
{
    private readonly CoreContext _coreContext;

    public DemoRequestRepository(CoreContext context) : base(context)
    {
        _coreContext = context;
    }

    public IQueryable<DemoRequest> SearchIQueryable(string? src, Dictionary<string, string>? customFilter = null)
    {
        IQueryable<DemoRequest> query = _coreContext.DemoRequests.Where(x => x.Active);

        // Busca textual
        if (!string.IsNullOrWhiteSpace(src))
        {
            var searchLower = src.ToLower();
            query = query.Where(x =>
                x.FullName.ToLower().Contains(searchLower) ||
                x.Email.ToLower().Contains(searchLower) ||
                x.Phone.Contains(searchLower) ||
                (x.CompanyName != null && x.CompanyName.ToLower().Contains(searchLower)));
        }

        // Filtros customizados
        if (customFilter != null)
        {
            if (customFilter.TryGetValue("status", out var status) && !string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.Status == status);
            }

            if (customFilter.TryGetValue("eventType", out var eventType) && !string.IsNullOrWhiteSpace(eventType))
            {
                query = query.Where(x => x.EventType == eventType);
            }

            if (customFilter.TryGetValue("estimatedAudience", out var audience) && !string.IsNullOrWhiteSpace(audience))
            {
                query = query.Where(x => x.EstimatedAudience == audience);
            }
        }

        return query;
    }

    public new async Task<DemoRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _coreContext.DemoRequests
            .FirstOrDefaultAsync(x => x.Id == id && x.Active, cancellationToken);
    }

    public async Task CreateAsync(DemoRequest entity, CancellationToken cancellationToken)
    {
        await _coreContext.DemoRequests.AddAsync(entity, cancellationToken);
        await _coreContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DemoRequest entity, CancellationToken cancellationToken)
    {
        _coreContext.DemoRequests.Update(entity);
        await _coreContext.SaveChangesAsync(cancellationToken);
    }
}
