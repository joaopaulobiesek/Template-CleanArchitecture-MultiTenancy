using Template.Application.Common.Interfaces.IRepositories.Core.Base;
using Template.Domain.Entity.Core;

namespace Template.Application.Common.Interfaces.IRepositories.Core.Implementations;

public interface IClientRepository : IRepository<Client>
{
    IQueryable<Client> SearchIQueryable(string? src, Dictionary<string, string>? customFilter = null);
    Task CreateAsync(Client client, CancellationToken cancellationToken = default);
    Task<List<string>> GetActiveModulesAsync(Guid userId);
    Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Client?> GetByUrlAsync(string url, CancellationToken cancellationToken = default);
    Task<string?> GetConnectionStringByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string?> GetStorageConfigurationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string?> GetSendGridConfigurationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string?> GetTimeZoneIdByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string?> GetUrlByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string?> GetAllowedIpsByIdAsync(Guid id, CancellationToken cancellationToken = default);
}