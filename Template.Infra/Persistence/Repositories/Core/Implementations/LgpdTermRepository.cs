using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Domain.Entity.Core;
using Template.Infra.Persistence.Contexts.Core;
using Template.Infra.Persistence.Repositories.Core.Base;
using Microsoft.EntityFrameworkCore;

namespace Template.Infra.Persistence.Repositories.Core.Implementations;

public class LgpdTermRepository : Repository<LgpdTerm>, ILgpdTermRepository
{
    private readonly CoreContext _context;

    public LgpdTermRepository(CoreContext context) : base(context)
    {
        _context = context;
    }

    public IQueryable<LgpdTerm> SearchIQueryable(string? src, Dictionary<string, string>? customFilter = null)
    {
        IQueryable<LgpdTerm> query = _context.LgpdTerms.Where(x => x.Active);

        if (!string.IsNullOrWhiteSpace(src))
        {
            query = query.Where(x =>
                x.Version.Contains(src) ||
                x.TermsOfUseContent.Contains(src) ||
                x.PrivacyPolicyContent.Contains(src));
        }

        return query;
    }

    public async Task<LgpdTerm?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.LgpdTerms
            .FirstOrDefaultAsync(x => x.Id == id && x.Active, cancellationToken);

    public async Task<LgpdTerm?> GetActiveAsync(CancellationToken cancellationToken = default)
        => await _context.LgpdTerms
            .FirstOrDefaultAsync(x => x.Active && x.IsActive, cancellationToken);

    public async Task<bool> ExistsVersionAsync(string version, CancellationToken cancellationToken = default)
        => await _context.LgpdTerms
            .AnyAsync(x => x.Active && x.Version == version, cancellationToken);

    public async Task CreateAsync(LgpdTerm entity, CancellationToken cancellationToken = default)
    {
        await _context.LgpdTerms.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(LgpdTerm entity, CancellationToken cancellationToken = default)
    {
        _context.LgpdTerms.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAllAsync(CancellationToken cancellationToken = default)
    {
        var activeTerms = await _context.LgpdTerms
            .Where(x => x.Active && x.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var term in activeTerms)
        {
            term.Deactivate();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
