namespace Persistence.Abstractions;

public interface IDbConnectionFactory
{
    DatabaseProvider Provider { get; }
    Task<System.Data.Common.DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
