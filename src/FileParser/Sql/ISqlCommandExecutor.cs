using FileParser.Models;

namespace FileParser;

public interface ISqlCommandExecutor
{
    Task CreateDatabaseIfNotExists(CancellationToken cancellationToken);
    Task GenerateDatabaseTableSchema(IList<Configuration> configurations, string tableName, CancellationToken cancellationToken);
    Task FillDataIntoTable(string tableName, IList<IList<(string, Configuration)>> data, CancellationToken cancellationToken);
}
