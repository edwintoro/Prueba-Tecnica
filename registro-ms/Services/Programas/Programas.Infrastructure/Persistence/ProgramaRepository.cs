using System.Data.Common;
using Programas.Domain.Entities;
using Programas.Domain.Ports;
using Programas.Infrastructure.Persistence.Sql;
using Persistence.Abstractions;

namespace Programas.Infrastructure.Persistence;

/// <summary>
/// Adaptador de persistencia (Hexagonal) usando ADO.NET.
/// </summary>
public sealed class ProgramaRepository : IProgramaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ProgramaSqlDialectFactory _dialectFactory;

    public ProgramaRepository(IDbConnectionFactory connectionFactory, ProgramaSqlDialectFactory dialectFactory)
    {
        _connectionFactory = connectionFactory;
        _dialectFactory = dialectFactory;
    }

    public async Task<ProgramaCredito?> GetActivoAsync(CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.SelectActivo;
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapPrograma(reader) : null;
    }

    public async Task<EstudiantePrograma?> GetAdhesionByEstudianteIdAsync(int estudianteId, CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.SelectAdhesionByEstudianteId;
        AddParameter(cmd, "@estudianteId", estudianteId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? MapAdhesion(reader) : null;
    }

    public async Task<bool> IsEstudianteAdheridoAsync(int estudianteId, CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.ExistsAdhesionByEstudianteId;
        AddParameter(cmd, "@estudianteId", estudianteId);
        var count = Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));
        return count > 0;
    }

    public async Task<int> AdherirEstudianteAsync(EstudiantePrograma adhesion, CancellationToken cancellationToken = default)
    {
        var sql = _dialectFactory.Create();
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql.InsertAdhesion;
        AddParameter(cmd, "@estudianteId", adhesion.EstudianteId);
        AddParameter(cmd, "@programaId", adhesion.ProgramaId);
        AddParameter(cmd, "@fechaAdhesion", adhesion.FechaAdhesion);

        if (_connectionFactory.Provider == DatabaseProvider.PostgreSql)
            return Convert.ToInt32(await cmd.ExecuteScalarAsync(cancellationToken));

        await cmd.ExecuteNonQueryAsync(cancellationToken);
        await using var idCmd = conn.CreateCommand();
        idCmd.CommandText = sql.LastInsertId;
        return Convert.ToInt32(await idCmd.ExecuteScalarAsync(cancellationToken));
    }

    private static ProgramaCredito MapPrograma(DbDataReader reader) =>
        ProgramaCredito.Rehydrate(
            reader.GetInt32(reader.GetOrdinal("id")),
            reader.GetString(reader.GetOrdinal("nombre")),
            reader.GetInt32(reader.GetOrdinal("creditos_por_materia")),
            reader.GetInt32(reader.GetOrdinal("max_materias")),
            reader.GetInt32(reader.GetOrdinal("total_creditos")),
            Convert.ToBoolean(reader.GetValue(reader.GetOrdinal("activo"))));

    private static EstudiantePrograma MapAdhesion(DbDataReader reader) =>
        EstudiantePrograma.Rehydrate(
            reader.GetInt32(reader.GetOrdinal("id")),
            reader.GetInt32(reader.GetOrdinal("estudiante_id")),
            reader.GetInt32(reader.GetOrdinal("programa_id")),
            reader.GetDateTime(reader.GetOrdinal("fecha_adhesion")));

    private static void AddParameter(DbCommand cmd, string name, object value)
    {
        var param = cmd.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        cmd.Parameters.Add(param);
    }
}
