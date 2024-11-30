using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Template.Infra.Persistence.Contexts.Core;

public class CoreDapper : ICoreDapperConnection, IDisposable
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private CoreContext _context => _serviceProvider.GetRequiredService<CoreContext>();

    public CoreDapper(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
    }

    private IDbConnection _connection;
    private IDbConnection Connection
    {
        get
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                var connectionString = GetTenantConnectionString();
                _connection = new SqlConnection(connectionString);
                _connection.Open();
            }
            return _connection;
        }
    }

    private string GetTenantConnectionString()
    {
        if (_httpContextAccessor.HttpContext?.Items["X-Tenant-ID"] is Guid tenantId)
        {
            var connectionString = BuscarStringDeConexaoDoTenant(tenantId);

            if (!string.IsNullOrEmpty(connectionString))
            {
                return connectionString;
            }
        }

        throw new Exception("Tenant ID not found or invalid.");
    }

    private string BuscarStringDeConexaoDoTenant(Guid tenantId)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .Build();

        if (tenantId == Guid.Empty)
            return string.Empty;

        var connectionString = configuration.GetConnectionString(tenantId.ToString());

        if (!string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }

        throw new Exception($"Tenant {tenantId} não encontrado.");
    }

    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        => (await _connection.QueryAsync<T>(sql, param, transaction)).AsList();

    public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
     => await _connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction);

    public async Task<T> QuerySingleAsync<T>(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
        => await _connection.QuerySingleAsync<T>(sql, param, transaction);

    public async Task<int> ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, CancellationToken cancellationToken = default)
     => await _context.Database.ExecuteSqlInterpolatedAsync($"{sql}", cancellationToken);

    public void Dispose()
        => _connection.Dispose();
}