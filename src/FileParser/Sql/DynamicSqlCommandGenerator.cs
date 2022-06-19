using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;
using FileParser.Models;

namespace FileParser.Sql;

public class DynamicSqlCommandGenerator : IDynamicSqlCommandGenerator
{
    private readonly Regex _regex = new("^[a-zA-Z0-9- ]*$");

    public string GenerateDatabaseTableSql(IList<Configuration> configurations, string tableName)
    {
        if (!_regex.IsMatch(tableName))
        {
            throw new ValidationException("Input string for Table Name did not match the criteria");
        }

        var tableStructureSubstringBuilder = new StringBuilder();
        foreach (Configuration configuration in configurations)
        {
            if (!_regex.IsMatch(configuration.ColumnName))
            {
                throw new ValidationException("Input string for Column Name did not match the criteria");
            }

            switch (configuration.DataType)
            {
                case DataTypeValue.TEXT:
                    tableStructureSubstringBuilder.Append($" {configuration.ColumnName} NVARCHAR(100), ");
                    break;
                case DataTypeValue.BOOLEAN:
                    tableStructureSubstringBuilder.Append($" {configuration.ColumnName} BIT, ");
                    break;
                case DataTypeValue.INTEGER:
                    tableStructureSubstringBuilder.Append($" {configuration.ColumnName} INT, ");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        string createTableSql = @$"
            Use ParserDatabase
            IF OBJECT_ID('{tableName}', 'U') IS NULL
            BEGIN
              CREATE TABLE dbo.{tableName}
              (
                {tableStructureSubstringBuilder}
              );
            END;";

        return createTableSql;
    }

    public string GenerateInsertionSql(string tableName, IList<IList<(string, Configuration)>> data)
    {
        var columnNamesBuilder = new StringBuilder();
        var valuesBuilder = new StringBuilder();
        var uniqueColumnNames = new HashSet<string>();
        for (int dataIterator = 0; dataIterator < data.Count; dataIterator++)
        {
            IList<(string, Configuration)> dataLine = data[dataIterator];
            foreach (string columnName in dataLine.GroupBy(s => s.Item2.ColumnName).Select(s => s.Key))
            {
                if (!_regex.IsMatch(columnName))
                {
                    throw new ValidationException("Input string for Data Column Value did not match the criteria");
                }

                uniqueColumnNames.Add(columnName);
            }

            valuesBuilder.Append('(');

            for (int i = 0; i < dataLine.Count; i++)
            {
                if (i > 0)
                {
                    valuesBuilder.Append(',');
                }

                string dataForInsertion = dataLine[i].Item1;

                if (!_regex.IsMatch(dataForInsertion))
                {
                    throw new ValidationException("Input string for Data Column Value did not match the criteria");
                }

                valuesBuilder.Append($@"'{dataLine[i].Item1}'");
            }

            valuesBuilder.Append(')');
            if (dataIterator != data.Count - 1)
            {
                valuesBuilder.Append(',');
            }

            valuesBuilder.AppendLine();
        }

        string listOfNames = GenerateColumnNames(uniqueColumnNames, columnNamesBuilder);

        //TODO this can be rewritten using Table Valued Parameters in SQL Server to completely prevent SQL injection and remove limitation of Regex validation of incoming values
        string sql = $@"
            Use ParserDatabase
            INSERT INTO {tableName} ({listOfNames})
            values {valuesBuilder}";

        return sql;
    }

    private static string GenerateColumnNames(HashSet<string> uniqueColumnNames, StringBuilder columnNamesBuilder)
    {
        var listOfNames = uniqueColumnNames.ToList();
        for (int i = 0; i < uniqueColumnNames.Count; i++)
        {
            if (i > 0)
            {
                columnNamesBuilder.Append($",");
            }

            columnNamesBuilder.Append($"{listOfNames[i]}");
        }

        return columnNamesBuilder.ToString();
    }
}
