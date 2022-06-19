using FileParser.Models;

namespace FileParser.Sql;

public interface IDynamicSqlCommandGenerator
{
    string GenerateDatabaseTableSql(IList<Configuration> configurations, string tableName);
    string GenerateInsertionSql(string tableName, IList<IList<(string, Configuration)>> data);
}
