using System.Data.Common;
using Auth.Domain;
using Auth.Domain.Ports;
using Persistence.Abstractions;

namespace Auth.Infrastructure.Persistence;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UsuarioRepository(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task<UsuarioAuth?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT u.id, u.email, u.password_hash, r.codigo, u.estudiante_id, e.nombre, u.activo
            FROM usuarios u
            INNER JOIN roles r ON r.id = u.rol_id
            LEFT JOIN estudiantes e ON e.id = u.estudiante_id
            WHERE u.email = @email
            LIMIT 1
            """;
        return await QuerySingleAsync(sql, cmd => Add(cmd, "@email", email), cancellationToken);
    }

    public async Task<UsuarioAuth?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT u.id, u.email, u.password_hash, r.codigo, u.estudiante_id, e.nombre, u.activo
            FROM usuarios u
            INNER JOIN roles r ON r.id = u.rol_id
            LEFT JOIN estudiantes e ON e.id = u.estudiante_id
            WHERE u.id = @id
            LIMIT 1
            """;
        return await QuerySingleAsync(sql, cmd => Add(cmd, "@id", id), cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(1) FROM usuarios WHERE email = @email";
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        Add(cmd, "@email", email);
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result) > 0;
    }

    public async Task<int> CreateEstudianteUsuarioAsync(
        string email,
        string passwordHash,
        string nombreEstudiante,
        CancellationToken cancellationToken = default)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var tx = await conn.BeginTransactionAsync(cancellationToken);

        var estudianteId = await InsertEstudianteAsync(conn, tx, nombreEstudiante, email, cancellationToken);
        await InsertUsuarioAsync(conn, tx, email, passwordHash, estudianteId, cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return estudianteId;
    }

    private static async Task<int> InsertEstudianteAsync(
        DbConnection conn,
        DbTransaction tx,
        string nombre,
        string email,
        CancellationToken cancellationToken)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            INSERT INTO estudiantes (nombre, email, fecha_registro)
            VALUES (@nombre, @email, UTC_TIMESTAMP());
            SELECT LAST_INSERT_ID();
            """;
        Add(cmd, "@nombre", nombre);
        Add(cmd, "@email", email);
        var id = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(id);
    }

    private static async Task InsertUsuarioAsync(
        DbConnection conn,
        DbTransaction tx,
        string email,
        string passwordHash,
        int estudianteId,
        CancellationToken cancellationToken)
    {
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            INSERT INTO usuarios (email, password_hash, rol_id, estudiante_id, activo)
            VALUES (@email, @hash, (SELECT id FROM roles WHERE codigo = 'Estudiante' LIMIT 1), @estudianteId, 1)
            """;
        Add(cmd, "@email", email);
        Add(cmd, "@hash", passwordHash);
        Add(cmd, "@estudianteId", estudianteId);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<UsuarioAuth?> QuerySingleAsync(
        string sql,
        Action<DbCommand> bind,
        CancellationToken cancellationToken)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        bind(cmd);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return new UsuarioAuth
        {
            Id = reader.GetInt32(0),
            Email = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            RolCodigo = reader.GetString(3),
            EstudianteId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
            EstudianteNombre = reader.IsDBNull(5) ? null : reader.GetString(5),
            Activo = reader.GetInt32(6) == 1
        };
    }

    private static void Add(DbCommand cmd, string name, object value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }
}

public sealed class SesionRepository : ISesionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SesionRepository(IDbConnectionFactory connectionFactory) => _connectionFactory = connectionFactory;

    public async Task CreateAsync(SesionAuth sesion, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO sesiones (id, usuario_id, refresh_token_hash, ultima_actividad, expira_en, revocada)
            VALUES (@id, @usuarioId, @hash, @ultima, @expira, 0)
            """;
        await ExecuteAsync(sql, cmd =>
        {
            Add(cmd, "@id", sesion.Id.ToString());
            Add(cmd, "@usuarioId", sesion.UsuarioId);
            Add(cmd, "@hash", sesion.RefreshTokenHash);
            Add(cmd, "@ultima", sesion.UltimaActividad);
            Add(cmd, "@expira", sesion.ExpiraEn);
        }, cancellationToken);
    }

    public async Task<SesionAuth?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, usuario_id, refresh_token_hash, ultima_actividad, expira_en, revocada
            FROM sesiones WHERE id = @id LIMIT 1
            """;
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        Add(cmd, "@id", id.ToString());
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;
        return Map(reader);
    }

    public async Task UpdateActividadAsync(
        Guid id,
        DateTime ultimaActividad,
        string refreshTokenHash,
        DateTime expiraEn,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE sesiones
            SET ultima_actividad = @ultima, refresh_token_hash = @hash, expira_en = @expira
            WHERE id = @id
            """;
        await ExecuteAsync(sql, cmd =>
        {
            Add(cmd, "@id", id.ToString());
            Add(cmd, "@ultima", ultimaActividad);
            Add(cmd, "@hash", refreshTokenHash);
            Add(cmd, "@expira", expiraEn);
        }, cancellationToken);
    }

    public async Task TouchAsync(Guid id, DateTime ultimaActividad, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE sesiones SET ultima_actividad = @ultima WHERE id = @id AND revocada = 0";
        await ExecuteAsync(sql, cmd =>
        {
            Add(cmd, "@id", id.ToString());
            Add(cmd, "@ultima", ultimaActividad);
        }, cancellationToken);
    }

    public async Task RevocarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE sesiones SET revocada = 1 WHERE id = @id";
        await ExecuteAsync(sql, cmd => Add(cmd, "@id", id.ToString()), cancellationToken);
    }

    private async Task ExecuteAsync(string sql, Action<DbCommand> bind, CancellationToken cancellationToken)
    {
        await using var conn = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        bind(cmd);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static SesionAuth Map(DbDataReader reader) => new()
    {
        Id = Guid.Parse(reader.GetString(0)),
        UsuarioId = reader.GetInt32(1),
        RefreshTokenHash = reader.GetString(2),
        UltimaActividad = reader.GetDateTime(3),
        ExpiraEn = reader.GetDateTime(4),
        Revocada = reader.GetInt32(5) == 1
    };

    private static void Add(DbCommand cmd, string name, object value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }
}
