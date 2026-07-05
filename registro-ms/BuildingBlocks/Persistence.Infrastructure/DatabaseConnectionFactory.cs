using System.Data.Common;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Npgsql;
using Persistence.Abstractions;

namespace Persistence.Infrastructure;

/// <summary>
/// Factory Pattern: selecciona el adaptador de BD según configuración (MySQL / PostgreSQL).
/// </summary>
public sealed class DatabaseConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseOptions _options;

    public DatabaseConnectionFactory(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }

    public DatabaseProvider Provider => _options.Provider;

    public async Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        if (_options.Provider == DatabaseProvider.MySql && connection is MySqlConnection)
        {
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = "SET NAMES utf8mb4 COLLATE utf8mb4_unicode_ci;";
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        return connection;
    }

    private DbConnection CreateConnection() => _options.Provider switch
    {
        DatabaseProvider.MySql => new MySqlConnection(_options.ConnectionString),
        DatabaseProvider.PostgreSql => new NpgsqlConnection(_options.ConnectionString),
        _ => throw new NotSupportedException($"Database provider '{_options.Provider}' is not supported.")
    };
}
