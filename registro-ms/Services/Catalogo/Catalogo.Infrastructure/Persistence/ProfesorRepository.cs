using System.Data.Common;
using Catalogo.Domain.Entities;
using Catalogo.Domain.Ports;
using Catalogo.Infrastructure.Persistence.Sql;
using Persistence.Abstractions;

namespace Catalogo.Infrastructure.Persistence;

public sealed class ProfesorRepository : IProfesorRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ProfesorSqlDialectFactory _dialectFactory;

    public ProfesorRepository(IDbConnectionFactory connectionFactory, ProfesorSqlDialectFactory dialectFactory)
    {
        _connectionFactory = connectionFactory;
        _dialectFactory = dialectFactory;
    }

    public async Task<IReadOnlyList<Profesor>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.SelectAll;
        return await ReadAllAsync(cmd, cancellationToken);
    }

    public async Task<Profesor?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.SelectById;
        AddParameter(cmd, "@id", id);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<int> AddAsync(Profesor profesor, CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.Insert;
        AddParameter(cmd, "@nombre", profesor.Nombre);

        if (_connectionFactory.Provider == DatabaseProvider.PostgreSql)
            return Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));

        await cmd.ExecuteNonQueryAsync(cancellationToken);
        await using var idCmd = conn.CreateCommand();
        idCmd.CommandText = sql.LastInsertId;
        return Convert.ToInt32(await idCmd.ExecuteScalarAsync(cancellationToken));
    }

    public async Task UpdateAsync(Profesor profesor, CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.Update;
        AddParameter(cmd, "@id", profesor.Id);
        AddParameter(cmd, "@nombre", profesor.Nombre);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.Delete;
        AddParameter(cmd, "@id", id);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> HasMateriasAsync(int id, CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.HasMaterias;
        AddParameter(cmd, "@id", id);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken)) > 0;
    }

    private static async Task<IReadOnlyList<Profesor>> ReadAllAsync(DbCommand cmd, CancellationToken cancellationToken)
    {
        var list = new List<Profesor>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            list.Add(Map(reader));
        return list;
    }

    private static Profesor Map(DbDataReader reader) =>
        Profesor.Rehydrate(
            reader.GetInt32(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("nombre")));

    private static void AddParameter(DbCommand cmd, string name, object value)
    {
        var param = cmd.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        cmd.Parameters.Add(param);
    }
}
