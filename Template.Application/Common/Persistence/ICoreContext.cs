using Template.Domain.Entity.Core;

namespace Template.Application.Common.Persistence;

public interface ICoreContext
{
    DbSet<Client> Clients { get; }
    DbSet<ClientModule> ClientModules { get; }
    DbSet<LgpdTerm> LgpdTerms { get; set; }
    DbSet<DemoRequest> DemoRequests { get; set; }
    void SetConnectionString(string connectionString);
    Task ApplyMigrations();
    DbSet<T> Set<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}