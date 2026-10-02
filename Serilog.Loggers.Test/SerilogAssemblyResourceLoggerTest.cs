using CustomConfigurationProvider;
using CustomJsonConfigurationProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Configuration.Extensions;

namespace Serilog.Loggers.Test;

public class SerilogAssemblyResourceLoggerTest
{
    private readonly HostApplicationBuilder _builder;

    private readonly string _logPath;

    private readonly LoggerConfiguration _loggerConfiguration;

    public SerilogAssemblyResourceLoggerTest()
    {
        _builder = new HostApplicationBuilder();
        _logPath = $"{Directory.GetCurrentDirectory()}\\Logs";
        _loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(_builder.Configuration);
    }

    private LoggerConfiguration BuildSerilogConfiguraion(IEnumerable<Action<ICustomConfigurationSource>> addRuleActions)
    {
        var _configurationBuilder = new ConfigurationBuilder();
        var source = _configurationBuilder.AddCustomJsonConfigurationProvider("log.assemblyresource.json");
        foreach (var action in addRuleActions)
        {
            action(source);
        }
        var configuration = _configurationBuilder.Build();
        return _loggerConfiguration.ReadFrom.Configuration(configuration);
    }

    private ILoggingBuilder SetSerilog(ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.ClearProviders();
        var logger = _loggerConfiguration.CreateLogger();
        return loggingBuilder.AddSerilog(logger);
    }

    [Fact]
    public void LogDirectoryContainsFolderWithLogsNamedResourceProperty()
    {
        var logPath = $"{_logPath}\\TestAssemblyResource";

        var serilogConfiguration = BuildSerilogConfiguraion([(source) => source.AddLogPathRule(logPath)]);
        SetSerilog(_builder.Logging);

        var app = _builder.Build();

        var logger = app.Services.GetRequiredService<ILogger<SerilogAssemblyResourceLoggerTest>>();

        var assemblyResourceLogger = new SerilogAssemblyResourcePropertyLogger(logger,
             new Dictionary<string, System.Reflection.Assembly>()
            {
                { "Serilog.Loggers.Test.LogMessages", typeof(SerilogAssemblyResourceLoggerTest).Assembly }
            },
            new Dictionary<string, Dictionary<string, object>>
            {
                { "Serilog.Loggers.Test.LogMessages", new Dictionary<string, object>{ { "AssemblyResource", true } } }
            });
        var guidAssemblyResource = Guid.NewGuid();
        assemblyResourceLogger.LogInformation(LogMessages.TestMessageForGuid, [guidAssemblyResource]);

        var propertyLogger = new SerilogPropertyLogger(logger, "AnyProperty", true);
        var anyGuid = Guid.NewGuid();
        propertyLogger.LogInformation(LogMessages.TestMessageForGuid, [anyGuid]);

        var path = $"{logPath}\\Info";
        Assert.True(Directory.Exists(path));

        var filePathes = Directory.GetFiles(path);
        Assert.NotEmpty(filePathes);

        foreach (var filePath in filePathes.Where(p => Path.GetExtension(p) == ".log" && p.Contains(DateTime.Now.ToString("yyyymmddhh"))))
        {
            Common.AssertFileContent(filePath, guidAssemblyResource.ToString());
            Common.AssertFileContentNotContains(filePath, anyGuid.ToString());
        }
    }
}