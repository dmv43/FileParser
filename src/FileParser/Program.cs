// See https://aka.ms/new-console-template for more information

using FileParser.BackgroundServices;
using FileParser.Sql;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FileParser;

public class Program
{
    public static void Main(string[] args)
    {
        Host.CreateDefaultBuilder(args)
            .UseConsoleLifetime()
            .ConfigureServices(
                (hostContext, services) =>
                {
                    services.AddHostedService<FileParsingService>();
                    services.AddSingleton<ISqlCommandExecutor, SqlCommandExecutor>();
                    services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
                    services.AddSingleton<IDynamicSqlCommandGenerator, DynamicSqlCommandGenerator>();
                    services.AddSingleton<IFileParser, FileParser>();
                }).Build().Run();
    }
}
