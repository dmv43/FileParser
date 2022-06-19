using FileParser.Models;

namespace FileParser;

public interface IFileParser
{
    string GetFileNameFromPath(string configurationFilePath);
    IList<Configuration> ParseConfigurations(string[] configurationLines);
    Task<IList<IList<(string, Configuration)>>> ParseDataAsync(string? filePath, IList<Configuration> configs, CancellationToken cancellationToken);
}
