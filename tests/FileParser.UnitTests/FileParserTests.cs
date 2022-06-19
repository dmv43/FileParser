using FileParser.Models;
using Xunit;

namespace FileParser.UnitTests;

public class FileParserTests
{
    [Theory]
    [MemberData(nameof(ParseDataLineTestData))]
    public void ParseDataLine_ShouldCorrectlyParseGivenLines(IList<Configuration> configs, string line, int expectedNumberOfResults)
    {
        var parser = new FileParser();
        IList<(string, Configuration)> result = parser.ParseDataLine(configs, line);

        Assert.NotNull(result);
        Assert.Equal(result.Count, expectedNumberOfResults);
    }

    [Theory]
    [MemberData(nameof(ParseConfigurationsTestData))]
    public void ParseConfigurations_ShouldCorrectlyParseGivenConfigurations(string[] configurations, int expectedNumberOfResults)
    {
        var parser = new FileParser();
        IList<Configuration> result = parser.ParseConfigurations(configurations);

        Assert.NotNull(result);
        Assert.Equal(result.Count, expectedNumberOfResults);
    }

    public static IEnumerable<object[]> ParseDataLineTestData
    {
        get
        {
            yield return new object[]
            { new List<Configuration> { new("First", 2, DataTypeValue.TEXT), new("Second", 2, DataTypeValue.TEXT), new("Third", 3, DataTypeValue.TEXT) }, "abcdefghijklmnopq", 3 };
            yield return new object[] { new List<Configuration> { new("Text", 2, DataTypeValue.TEXT), new("Number", 2, DataTypeValue.INTEGER), new("Boolean", 1, DataTypeValue.BOOLEAN) }, "ab111", 3 };
            yield return new object[] { new List<Configuration> { new("Text", 4, DataTypeValue.TEXT), new("Number", 4, DataTypeValue.INTEGER), new("Boolean", 1, DataTypeValue.BOOLEAN) }, "ab  11  1", 3 };
            yield return new object[] { new List<Configuration> { new("First", 2, DataTypeValue.TEXT), new("Second", 2, DataTypeValue.TEXT), new("Third", 3, DataTypeValue.TEXT) }, "abc", 2 };
            yield return new object[] { new List<Configuration> { new("Text", 2, DataTypeValue.TEXT), new("Number", 2, DataTypeValue.INTEGER), new("Boolean", 1, DataTypeValue.BOOLEAN) }, "abcdee", 1 };
            yield return new object[] { new List<Configuration> { new("Text", 4, DataTypeValue.TEXT), new("Number", 4, DataTypeValue.INTEGER), new("Boolean", 1, DataTypeValue.BOOLEAN) }, "ab  11 11", 2 };
            yield return new object[] { new List<Configuration> { new("Text", 4, DataTypeValue.TEXT), new("Number", 4, DataTypeValue.INTEGER), new("Boolean", 1, DataTypeValue.BOOLEAN) }, "ab  11 11", 2 };
            yield return new object[] { new List<Configuration> { new("Text", 4, DataTypeValue.TEXT), new("Number", 4, DataTypeValue.INTEGER), new("Boolean", 1, DataTypeValue.BOOLEAN) }, "", 0 };
            yield return new object[] { new List<Configuration>(), string.Empty, 0 };
        }
    }

    public static IEnumerable<object[]> ParseConfigurationsTestData
    {
        get
        {
            yield return new object[] { new[] { "name,10,TEXT" }, 1 };
            yield return new object[] { new[] { "name,10,TEXT", "value,10,TEXT" }, 2 };
            yield return new object[] { new[] { "name,10,TEXT", "name,10,TEXT" }, 1 };
            yield return new object[] { new[] { "name,10,TEXT", "isValid,1,BOOLEAN", "count,5,INTEGER" }, 3 };
        }
    }
}
