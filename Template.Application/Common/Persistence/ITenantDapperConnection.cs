using System.Data;

namespace Template.Application.Common.Persistence;

public interface ITenantDapperConnection
{
    Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<T> QuerySingleAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<int> ExecuteAsync(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executa múltiplas operações em uma transação.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<IDbTransaction, Task> action, CancellationToken cancellationToken = default);
}
