using Persistence.Abstractions;

namespace Estudiantes.Infrastructure.Persistence.Sql;

/// <summary>
/// Strategy Pattern: dialectos SQL por proveedor de BD.
/// </summary>
public interface IEstudianteSqlDialect
{
    string SelectAll { get; }
    string SelectById { get; }
    string ExistsByEmail { get; }
    string ExistsByEmailExcluding { get; }
    string Insert { get; }
    string Update { get; }
    string Delete { get; }
    string LastInsertId { get; }
}

public sealed class MySqlEstudianteSqlDialect : IEstudianteSqlDialect
{
    public string SelectAll => "SELECT id, nombre, email, fecha_registro FROM estudiantes ORDER BY nombre";
    public string SelectById => "SELECT id, nombre, email, fecha_registro FROM estudiantes WHERE id = @id";
    public string ExistsByEmail => "SELECT COUNT(*) FROM estudiantes WHERE email = @email";
    public string ExistsByEmailExcluding => "SELECT COUNT(*) FROM estudiantes WHERE email = @email AND id != @excludeId";
    public string Insert => "INSERT INTO estudiantes (nombre, email, fecha_registro) VALUES (@nombre, @email, @fechaRegistro)";
    public string Update => "UPDATE estudiantes SET nombre = @nombre, email = @email WHERE id = @id";
    public string Delete => "DELETE FROM estudiantes WHERE id = @id";
    public string LastInsertId => "SELECT LAST_INSERT_ID()";
}

public sealed class PostgreSqlEstudianteSqlDialect : IEstudianteSqlDialect
{
    public string SelectAll => "SELECT id, nombre, email, fecha_registro FROM estudiantes ORDER BY nombre";
    public string SelectById => "SELECT id, nombre, email, fecha_registro FROM estudiantes WHERE id = @id";
    public string ExistsByEmail => "SELECT COUNT(*) FROM estudiantes WHERE email = @email";
    public string ExistsByEmailExcluding => "SELECT COUNT(*) FROM estudiantes WHERE email = @email AND id != @excludeId";
    public string Insert => "INSERT INTO estudiantes (nombre, email, fecha_registro) VALUES (@nombre, @email, @fechaRegistro) RETURNING id";
    public string Update => "UPDATE estudiantes SET nombre = @nombre, email = @email WHERE id = @id";
    public string Delete => "DELETE FROM estudiantes WHERE id = @id";
    public string LastInsertId => string.Empty;
}

public sealed class EstudianteSqlDialectFactory
{
    private readonly DatabaseProvider _provider;

    public EstudianteSqlDialectFactory(IDbConnectionFactory connectionFactory)
    {
        _provider = connectionFactory.Provider;
    }

    public IEstudianteSqlDialect Create() => _provider switch
    {
        DatabaseProvider.MySql => new MySqlEstudianteSqlDialect(),
        DatabaseProvider.PostgreSql => new PostgreSqlEstudianteSqlDialect(),
        _ => throw new NotSupportedException($"Provider '{_provider}' not supported.")
    };
}
