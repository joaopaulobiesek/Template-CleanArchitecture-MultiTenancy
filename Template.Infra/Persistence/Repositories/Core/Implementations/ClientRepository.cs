using Template.Application.Common.Extensions;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Domain;
using Template.Domain.Entity.Core;
using Template.Infra.Persistence.Contexts.Core;
using Template.Infra.Persistence.Repositories.Core.Base;

namespace Template.Infra.Persistence.Repositories.Core.Implementations
{
    public class ClientRepository : Repository<Client>, IClientRepository
    {
        private readonly CoreContext _context;

        public ClientRepository(CoreContext context) : base(context)
        {
            _context = context;
        }

        public async Task CreateAsync(Client client, CancellationToken cancellationToken = default)
        {
            await _context.Clients.AddAsync(client, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public IQueryable<Client> SearchIQueryable(string? src, Dictionary<string, string>? customFilter)
        {
            IQueryable<Client> query = _context.Clients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(src))
            {
                var documentOrPhone = StringFormatter.RemoveNonNumericCharacters(src);

                query = query.Where(x => x.Active &&
                        x.FullName != null && x.FullName.Contains(src) ||
                        !string.IsNullOrWhiteSpace(documentOrPhone) && x.DocumentNumber.Replace(".", "").Replace("/", "").Replace("-", "").Contains(documentOrPhone) ||
                        !string.IsNullOrWhiteSpace(documentOrPhone) && x.Phone != null && x.Phone.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "").Contains(src)
                );
            }

            if (customFilter != null && customFilter.Any())
            {
                var filteredQuery = query.ApplyCustomFiltersWithWhitelist(
                    customFilter,
                    "Paid"
                );

                if (filteredQuery != null)
                    query = filteredQuery;
            }

            return query;
        }

        public async Task<List<string>> GetActiveModulesAsync(Guid userId)
        {
            return await _context.ClientModules
                .Where(cm => cm.ClientId == userId && cm.DeactivatedAt == null)
                .Select(cm => cm.Module)
                .ToListAsync();
        }

        public async Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == id && c.Active, cancellationToken);

        public async Task<Client?> GetByUrlAsync(string host, CancellationToken cancellationToken = default)
            => await _context.Clients
                .FirstOrDefaultAsync(c => c.Active && c.Url != null && c.Url.ToLower() == host.ToLower(), cancellationToken);

        public async Task<string?> GetConnectionStringByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Clients
                .Where(c => c.Id == id && c.Active)
                .Select(c => c.ConnectionString)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<string?> GetStorageConfigurationByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Clients
                .Where(c => c.Id == id && c.Active)
                .Select(c => c.StorageConfiguration)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<string?> GetSendGridConfigurationByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Clients
                .Where(c => c.Id == id && c.Active)
                .Select(c => c.SendGridConfiguration)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<string?> GetTimeZoneIdByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Clients
                .Where(c => c.Id == id && c.Active)
                .Select(c => c.TimeZoneId)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<string?> GetUrlByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Clients
                .Where(c => c.Id == id && c.Active)
                .Select(c => c.Url)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<string?> GetAllowedIpsByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => await _context.Clients
                .Where(c => c.Id == id && c.Active)
                .Select(c => c.AllowedIpsJson)
                .FirstOrDefaultAsync(cancellationToken);
    }
}