using System.Data.Common;
using Catalogo.Domain.Entities;
using Catalogo.Domain.Ports;
using Catalogo.Domain.ReadModels;
using Catalogo.Infrastructure.Persistence.Sql;
using Persistence.Abstractions;

namespace Catalogo.Infrastructure.Persistence;

public sealed class MateriaRepository : IMateriaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly MateriaSqlDialectFactory _dialectFactory;

    public MateriaRepository(IDbConnectionFactory connectionFactory, MateriaSqlDialectFactory dialectFactory)
    {
        _connectionFactory = connectionFactory;
        _dialectFactory = dialectFactory;
    }

    public async Task<IReadOnlyList<MateriaConProfesor>> GetAllWithProfesorAsync(CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.SelectAllWithProfesor;
        return await ReadAllAsync(cmd, cancellationToken);
    }

    public async Task<MateriaConProfesor?> GetByIdWithProfesorAsync(int id, CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.SelectByIdWithProfesor;
        AddParameter(cmd, "@id", id);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<int> CountByProfesorAsync(int profesorId, int? excludeMateriaId = null, CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = excludeMateriaId.HasValue ? sql.CountByProfesorExcluding : sql.CountByProfesor;
        AddParameter(cmd, "@profesorId", profesorId);
        if (excludeMateriaId.HasValue) AddParameter(cmd, "@excludeId", excludeMateriaId.Value);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<int> CountAllAsync(CancellationToken cancellationToken = default)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM materias";
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
    }

    public async Task<int> AddAsync(Materia materia, CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.Insert;
        AddParameter(cmd, "@nombre", materia.Nombre);
        AddParameter(cmd, "@creditos", materia.Creditos);
        AddParameter(cmd, "@profesorId", materia.ProfesorId);
        AddParameter(cmd, "@programaId", materia.ProgramaId);

        if (_connectionFactory.Provider == DatabaseProvider.PostgreSql)
            return Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));

        await cmd.ExecuteNonQueryAsync(cancellationToken);
        await using var idCmd = conn.CreateCommand();
        idCmd.CommandText = sql.LastInsertId;
        return Convert.ToInt32(await idCmd.ExecuteScalarAsync(cancellationToken));
    }

    public async Task UpdateAsync(Materia materia, CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.Update;
        AddParameter(cmd, "@id", materia.Id);
        AddParameter(cmd, "@nombre", materia.Nombre);
        AddParameter(cmd, "@creditos", materia.Creditos);
        AddParameter(cmd, "@profesorId", materia.ProfesorId);
        AddParameter(cmd, "@programaId", materia.ProgramaId);
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

    public async Task<int?> GetActiveProgramaIdAsync(CancellationToken cancellationToken = default)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id FROM programas_creditos WHERE activo = 1 LIMIT 1";
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result is null or DBNull ? null : Convert.ToInt32(result);
    }

    private static async Task<IReadOnlyList<MateriaConProfesor>> ReadAllAsync(DbCommand cmd, CancellationToken cancellationToken)
    {
        var list = new List<MateriaConProfesor>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
            list.Add(Map(reader));
        return list;
    }

    private static MateriaConProfesor Map(DbDataReader reader) =>
        MateriaConProfesor.Rehydrate(
            reader.GetInt32(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("nombre")),
            reader.GetInt32(reader.GetOrdinal("creditos")),
            reader.GetInt32(reader.GetOrdinal("profesor_id")),
            reader.GetString(reader.GetOrdinal("profesor_nombre")),
            reader.GetInt32(reader.GetOrdinal("programa_id")));

    private static void AddParameter(DbCommand cmd, string name, object value)
    {
        var param = cmd.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        cmd.Parameters.Add(param);
    }
}
