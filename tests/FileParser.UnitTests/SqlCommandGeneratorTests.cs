using System.ComponentModel.DataAnnotations;
using FileParser.Models;
using FileParser.Sql;
using Xunit;

namespace FileParser.UnitTests;

public class SqlCommandGeneratorTests
{
    private readonly DynamicSqlCommandGenerator _generator;
    public SqlCommandGeneratorTests()
    {
        _generator = new DynamicSqlCommandGenerator();
    }

    [Theory]
    [MemberData(nameof(GenerateDatabaseSchemaTestData))]
    public void GenerateDatabaseSchemaShouldGenerateSql(List<Configuration> configurations, string tableName)
    {
       string result = _generator.GenerateDatabaseTableSql(configurations, tableName);

      Assert.NotEmpty(result);
    }

    [Theory]
    [MemberData(nameof(GenerateDatabaseSchemaInvalidTestData))]
    public void GenerateDatabaseSchemaShouldThrowValidationException(List<Configuration> configurations, string tableName)
    {
        Assert.Throws<ValidationException>(() => _generator.GenerateDatabaseTableSql(configurations, tableName));
    }

    [Theory]
    [MemberData(nameof(GenerateInsertionSqlTestData))]
    public void GenerateInsertionSqlShouldGenerateSql(IList<IList<(string,Configuration)>> data, string tableName)
    {
        string result = _generator.GenerateInsertionSql(
            tableName,
            data);

        Assert.NotEmpty(result);
    }

    [Theory]
    [MemberData(nameof(GenerateInsertionSqlInvalidTestData))]
    public void GenerateInsertionSqlShouldThrowException(IList<IList<(string,Configuration)>> data, string tableName)
    {
        Assert.Throws<ValidationException>(()=>_generator.GenerateInsertionSql(
            tableName,
            data));
    }

    public static IEnumerable<object[]> GenerateDatabaseSchemaTestData
    {
        get
        {
            yield return new object[] {new List<Configuration> { new("name", 10, DataTypeValue.TEXT) }, "test1"};
        }
    }

    public static IEnumerable<object[]> GenerateDatabaseSchemaInvalidTestData
    {
        get
        {
            yield return new object[] {new List<Configuration> { new("SELECT * FROM", 10, DataTypeValue.TEXT) }, "test1"};
            yield return new object[] {new List<Configuration> { new("left;right", 10, DataTypeValue.TEXT) }, "test1"};
        }
    }

    public static IEnumerable<object[]> GenerateInsertionSqlTestData
    {
        get
        {
            yield return new object[] {new List<IList<(string, Configuration)>> { new List<(string, Configuration)> { (string.Empty, new Configuration("name", 10, DataTypeValue.TEXT)) } }, "test1" };
        }
    }

    public static IEnumerable<object[]> GenerateInsertionSqlInvalidTestData
    {
        get
        {
            yield return new object[] {new List<IList<(string, Configuration)>> { new List<(string, Configuration)> { ("select * from", new Configuration("name", 10, DataTypeValue.TEXT)) } }, "test1" };
            yield return new object[] {new List<IList<(string, Configuration)>> { new List<(string, Configuration)> { (";Truncate", new Configuration("name", 10, DataTypeValue.TEXT)) } }, "test1" };
        }
    }
}
