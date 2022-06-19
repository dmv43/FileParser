using FileParser.Models;
using Microsoft.Extensions.Hosting;
using static FileParser.Constants.Constants;

namespace FileParser.BackgroundServices;

public class FileParsingService : BackgroundService
{
    private readonly ISqlCommandExecutor _sqlCommandExecutor;
    private readonly IFileParser _fileParser;
    private readonly IHostApplicationLifetime _appLifetime;

    public FileParsingService(ISqlCommandExecutor sqlCommandExecutor, IFileParser fileParser, IHostApplicationLifetime appLifetime)
    {
        _sqlCommandExecutor = sqlCommandExecutor ?? throw new ArgumentNullException(nameof(sqlCommandExecutor));
        _fileParser = fileParser ?? throw new ArgumentNullException(nameof(fileParser));
        _appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await ProcessFilesAsync(cancellationToken);
        Console.WriteLine("File processing is completed");
        _appLifetime.StopApplication();
    }

    private async Task ProcessFilesAsync(CancellationToken cancellationToken = default)
    {
        await _sqlCommandExecutor.CreateDatabaseIfNotExists(cancellationToken);

        string[] configurationFiles = Directory.GetFiles(SpecsFilePath, SpecsFileExtension);
        string?[] dataFiles = Directory.GetFiles(DataFilePath, DataFileExtension);

        foreach (string configurationFilePath in configurationFiles)
        {
            string fileName = _fileParser.GetFileNameFromPath(configurationFilePath);
            string[] configurationLines = await File.ReadAllLinesAsync(configurationFilePath, cancellationToken);
            IList<Configuration> configurations = _fileParser.ParseConfigurations(configurationLines);
            await _sqlCommandExecutor.GenerateDatabaseTableSchema(configurations, fileName, cancellationToken);
            IList<IList<(string, Configuration)>> data = await _fileParser.ParseDataAsync(
                dataFiles?.FirstOrDefault(s => s != null && s.Contains(fileName)),
                configurations,
                cancellationToken);

            await _sqlCommandExecutor.FillDataIntoTable(fileName, data, cancellationToken);
        }
    }
}
