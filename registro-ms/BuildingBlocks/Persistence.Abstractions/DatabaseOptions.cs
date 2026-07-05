namespace Persistence.Abstractions;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    public DatabaseProvider Provider { get; set; } = DatabaseProvider.MySql;
    public string ConnectionString { get; set; } = string.Empty;
}
