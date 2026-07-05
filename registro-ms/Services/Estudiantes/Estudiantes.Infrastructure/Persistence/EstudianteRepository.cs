using System.Data.Common;
using Estudiantes.Domain.Entities;
using Estudiantes.Domain.Ports;
using Estudiantes.Infrastructure.Persistence.Sql;
using Persistence.Abstractions;

namespace Estudiantes.Infrastructure.Persistence;

/// <summary>
/// Adaptador de persistencia (Hexagonal) usando ADO.NET.
/// </summary>
public sealed class EstudianteRepository : IEstudianteRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly EstudianteSqlDialectFactory _dialectFactory;

    public EstudianteRepository(IDbConnectionFactory connectionFactory, EstudianteSqlDialectFactory dialectFactory)
    {
        _connectionFactory = connectionFactory;
        _dialectFactory = dialectFactory;
    }

    public async Task<IReadOnlyList<Estudiante>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.SelectAll;
        return await ReadAllAsync(cmd, cancellationToken);
    }

    public async Task<Estudiante?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.SelectById;
        AddParameter(cmd, "@id", id);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = excludeId.HasValue ? sql.ExistsByEmailExcluding : sql.ExistsByEmail;
        AddParameter(cmd, "@email", email.ToLowerInvariant());
        if (excludeId.HasValue) AddParameter(cmd, "@excludeId", excludeId.Value);
        var count = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
        return count > 0;
    }

    public async Task<int> AddAsync(Estudiante estudiante, CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.Insert;
        AddParameter(cmd, "@nombre", estudiante.Nombre);
        AddParameter(cmd, "@email", estudiante.Email);
        AddParameter(cmd, "@fechaRegistro", estudiante.FechaRegistro);

        if (_connectionFactory.Provider == DatabaseProvider.PostgreSql)
            return Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));

        await cmd.ExecuteNonQueryAsync(cancellationToken);
        await using var idCmd = conn.CreateCommand();
        idCmd.CommandText = sql.LastInsertId;
        return Convert.ToInt32(await idCmd.ExecuteScalarAsync(cancellationToken));
    }

    public async Task UpdateAsync(Estudiante estudiante, CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.Update;
        AddParameter(cmd, "@id", estudiante.Id);
        AddParameter(cmd, "@nombre", estudiante.Nombre);
        AddParameter(cmd, "@email", estudiante.Email);
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

    private static async Task<IReadOnlyList<Estudiante>> ReadAllAsync(DbCommand cmd, CancellationToken cancellationToken)
    {
        var list = new List<Estudiante>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            list.Add(Map(reader));
        return list;
    }

    private static Estudiante Map(DbDataReader reader) =>
        Estudiante.Rehydrate(
            reader.GetInt32(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("nombre")),
            reader.GetString(reader.GetOrdinal("email")),
            reader.GetDateTime(reader.GetOrdinal("fecha_registro")));

    private static void AddParameter(DbCommand cmd, string name, object value)
    {
        var param = cmd.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        cmd.Parameters.Add(param);
    }
}
