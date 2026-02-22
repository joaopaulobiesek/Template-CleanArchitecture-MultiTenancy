namespace Template.Application.Common.Persistence;

public interface ITenantContext
{
    void SetConnectionString(string connectionString);
    string GetCurrentConnectionString();
    Task ApplyMigrations();
    DbSet<T> Set<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}