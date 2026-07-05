using Persistence.Abstractions;

namespace Catalogo.Infrastructure.Persistence.Sql;

public interface IProfesorSqlDialect
{
    string SelectAll { get; }
    string SelectById { get; }
    string Insert { get; }
    string Update { get; }
    string Delete { get; }
    string HasMaterias { get; }
    string LastInsertId { get; }
}

public sealed class MySqlProfesorSqlDialect : IProfesorSqlDialect
{
    public string SelectAll => "SELECT id, nombre FROM profesores ORDER BY nombre";
    public string SelectById => "SELECT id, nombre FROM profesores WHERE id = @id";
    public string Insert =>
        """
        INSERT INTO profesores (nombre) VALUES (@nombre);
        SELECT LAST_INSERT_ID();
        """;
    public string Update => "UPDATE profesores SET nombre = @nombre WHERE id = @id";
    public string Delete => "DELETE FROM profesores WHERE id = @id";
    public string HasMaterias => "SELECT COUNT(1) FROM materias WHERE profesor_id = @id";
    public string LastInsertId => "SELECT LAST_INSERT_ID()";
}

public sealed class PostgreSqlProfesorSqlDialect : IProfesorSqlDialect
{
    public string SelectAll => "SELECT id, nombre FROM profesores ORDER BY nombre";
    public string SelectById => "SELECT id, nombre FROM profesores WHERE id = @id";
    public string Insert => "INSERT INTO profesores (nombre) VALUES (@nombre) RETURNING id";
    public string Update => "UPDATE profesores SET nombre = @nombre WHERE id = @id";
    public string Delete => "DELETE FROM profesores WHERE id = @id";
    public string HasMaterias => "SELECT COUNT(1) FROM materias WHERE profesor_id = @id";
    public string LastInsertId => "SELECT 0";
}

public sealed class ProfesorSqlDialectFactory
{
    private readonly DatabaseProvider _provider;

    public ProfesorSqlDialectFactory(IDbConnectionFactory connectionFactory) =>
        _provider = connectionFactory.Provider;

    public IProfesorSqlDialect Create() => _provider switch
    {
        DatabaseProvider.MySql => new MySqlProfesorSqlDialect(),
        DatabaseProvider.PostgreSql => new PostgreSqlProfesorSqlDialect(),
        _ => throw new NotSupportedException($"Provider '{_provider}' not supported.")
    };
}
