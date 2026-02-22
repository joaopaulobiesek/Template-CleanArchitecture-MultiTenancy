using Dapper;
using Template.Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Template.Infra.Persistence.Contexts.Tenant;

public class TenantDapper : ITenantDapperConnection, IDisposable
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;

    public TenantDapper(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
    }

    private IDbConnection? _connection;
    private IDbConnection Connection
    {
        get
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                var connectionString = GetTenantConnectionStringAsync().GetAwaiter().GetResult();
                _connection = new SqlConnection(connectionString);
                _connection.Open();
            }
            return _connection;
        }
    }

    private async Task<string> GetTenantConnectionStringAsync()
    {
        // Primeiro tenta buscar do TenantId que o middleware ja setou
        if (_httpContextAccessor.HttpContext?.Items["TenantId"] is Guid tenantId && tenantId != Guid.Empty)
        {
            var tenantCacheService = _serviceProvider.GetRequiredService<ITenantCacheService>();
            var connectionString = await tenantCacheService.GetConnectionStringAsync(tenantId);

            if (!string.IsNullOrEmpty(connectionString))
            {
                return connectionString;
            }
        }

        // Fallback: tenta header X-Tenant-ID
        if (_httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("X-Tenant-ID", out var headerValue) == true
            && Guid.TryParse(headerValue, out var headerTenantId)
            && headerTenantId != Guid.Empty)
        {
            var tenantCacheService = _serviceProvider.GetRequiredService<ITenantCacheService>();
            var connectionString = await tenantCacheService.GetConnectionStringAsync(headerTenantId);

            if (!string.IsNullOrEmpty(connectionString))
            {
                return connectionString;
            }
        }

        throw new Exception("Tenant ID not found or invalid. Ensure 'X-Tenant-ID' header is provided.");
    }

    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
        => (await Connection.QueryAsync<T>(sql, param, transaction)).AsList();

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
        => await Connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction);

    public async Task<T> QuerySingleAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
        => await Connection.QuerySingleAsync<T>(sql, param, transaction);

    public async Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
        => await Connection.ExecuteAsync(sql, param, transaction);

    public async Task ExecuteInTransactionAsync(Func<IDbTransaction, Task> action, CancellationToken cancellationToken = default)
    {
        using var transaction = Connection.BeginTransaction();
        try
        {
            await action(transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void Dispose()
        => _connection?.Dispose();
}
