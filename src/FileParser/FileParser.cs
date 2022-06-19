using FileParser.Models;

namespace FileParser;

public class FileParser : IFileParser
{
    public string GetFileNameFromPath(string configurationFilePath)
    {
        string substring = configurationFilePath.Substring(configurationFilePath.LastIndexOf('\\') + 1);
        string fileName = substring.Remove(substring.Length - 4, 4);
        return fileName;
    }

    public IList<Configuration> ParseConfigurations(string[] configurationLines)
    {
        var configurations = new HashSet<Configuration>();
        foreach (string line in configurationLines)
        {
            string[] values = line.Split(',');
            bool isSuccessfulIntParse = int.TryParse(values[1], out int width);
            bool isSuccessfulEnumParse = Enum.TryParse(values[2], true, out DataTypeValue type) && Enum.IsDefined(typeof(DataTypeValue), type);
            if (!isSuccessfulIntParse || !isSuccessfulEnumParse)
            {
                Console.WriteLine($"Service was not able to parse configuration file");
                continue;
            }

            var configuration = new Configuration(values[0], width, type);
            configurations.Add(configuration);
        }

        return configurations.ToList();
    }

    public async Task<IList<IList<(string, Configuration)>>> ParseDataAsync(string? filePath, IList<Configuration> configs, CancellationToken cancellationToken)
    {
        if (filePath is null)
        {
            return new List<IList<(string, Configuration)>>();
        }

        string[] lines = await File.ReadAllLinesAsync(filePath, cancellationToken);

        return lines.Select(line => ParseDataLine(configs, line)).ToList();
    }

    internal IList<(string, Configuration)> ParseDataLine(IList<Configuration> configs, string line)
    {
        int absoluteOffset = 0;
        var dataLine = new List<(string, Configuration)>();
        foreach (Configuration config in configs)
        {
            if (absoluteOffset >= line.Length)
            {
                break;
            }
            int relativeWidth = config.Width;
            if (absoluteOffset + config.Width > line.Length)
            {
                relativeWidth = line.Length - absoluteOffset;
            }

            string substring = line.Substring(absoluteOffset, relativeWidth).Trim();

            //Currently parser will skip any inconsistent data.
            if (!IsTypeMatching(substring, config))
            {
                continue;
            }
            absoluteOffset += config.Width;
            (string substring, Configuration config) data = (substring, config);
            dataLine.Add(data);
        }

        return dataLine;
    }

    private bool IsTypeMatching(string substring, Configuration config)
    {
        switch (config.DataType)
        {
            case DataTypeValue.TEXT:
                return !string.IsNullOrWhiteSpace(substring);
            case DataTypeValue.BOOLEAN:
                return substring is "0" or "1";
            case DataTypeValue.INTEGER:
                return int.TryParse(substring, out int _);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
