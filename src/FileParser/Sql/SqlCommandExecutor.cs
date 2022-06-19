using System.Data;
using System.Text.RegularExpressions;
using Dapper;
using FileParser.Models;

namespace FileParser.Sql;

public class SqlCommandExecutor : ISqlCommandExecutor
{
    private readonly Regex _regex = new("^[a-zA-Z0-9- ]*$");
    private readonly ISqlConnectionFactory _sqlConnectionFactory;
    private readonly IDynamicSqlCommandGenerator _sqlCommandGenerator;

    public SqlCommandExecutor(ISqlConnectionFactory sqlConnectionFactory, IDynamicSqlCommandGenerator sqlCommandGenerator)
    {
        _sqlConnectionFactory = sqlConnectionFactory ?? throw new ArgumentNullException(nameof(sqlConnectionFactory));
        _sqlCommandGenerator = sqlCommandGenerator ?? throw new ArgumentNullException(nameof(sqlCommandGenerator));
    }

    public async Task CreateDatabaseIfNotExists(CancellationToken cancellationToken)
    {
        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();
        string createDbString = @$"USE master
              IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'ParserDatabase')
              BEGIN
                CREATE DATABASE [ParserDatabase]
              END";
        await connection.ExecuteAsync(createDbString, cancellationToken);
    }

    public async Task GenerateDatabaseTableSchema(IList<Configuration> configurations, string tableName, CancellationToken cancellationToken)
    {
        string sql = _sqlCommandGenerator.GenerateDatabaseTableSql(configurations, tableName);

        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(sql, cancellationToken);
    }

    public async Task FillDataIntoTable(string tableName, IList<IList<(string, Configuration)>> data, CancellationToken cancellationToken)
    {
        if (!data.Any())
        {
            return;
        }
        string sql = _sqlCommandGenerator.GenerateInsertionSql(tableName, data);

        using IDbConnection connection = _sqlConnectionFactory.CreateConnection();

        await connection.ExecuteAsync(sql, cancellationToken);
    }
}
