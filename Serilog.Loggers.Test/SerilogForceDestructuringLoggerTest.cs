using CustomConfigurationProvider;
using CustomJsonConfigurationProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Configuration.Extensions;

namespace Serilog.Loggers.Test;

public class SerilogForceDestructuringLoggerTest
{
    private readonly HostApplicationBuilder _builder;

    private readonly string _logPath;

    private readonly LoggerConfiguration _loggerConfiguration;

    public SerilogForceDestructuringLoggerTest()
    {
        _builder = new HostApplicationBuilder();
        _logPath = $"{Directory.GetCurrentDirectory()}\\Logs";
        _loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(_builder.Configuration);
    }

    private LoggerConfiguration BuildSerilogConfiguraion(IEnumerable<Action<ICustomConfigurationSource>> addRuleActions)
    {
        var _configurationBuilder = new ConfigurationBuilder();
        var source = _configurationBuilder.AddCustomJsonConfigurationProvider("log.property.json");
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
    public void LogFileAfterDestructiongLoggingContainsParametersWithAmpersatPrefix()
    {
        //_builder.Services.AddSingleton<TestLogger<double>>();

        var logPath = $"{_logPath}\\TestForceDestructuring";

        var serilogConfiguration = BuildSerilogConfiguraion([(source) => source.AddLogPathRule(logPath)]);
        SetSerilog(_builder.Logging);

        var app = _builder.Build();

        var testLogger = app.Services.GetRequiredService<ILogger<SerilogForceDestructuringLoggerTest>>();
        var forceDestructingLogger = new SerilogForceDestructuringLogger(testLogger);

        var guid = Guid.NewGuid();
        forceDestructingLogger.LogWarning(LogMessages.TestMessageForGuid, guid);

        var path = $"{logPath}\\Warning";
        Assert.True(Directory.Exists(path));

        var filePathes = Directory.GetFiles(path);
        Assert.NotEmpty(filePathes);

        foreach (var filePath in filePathes.Where(p => Path.GetExtension(p) == ".log" && p.Contains(DateTime.Now.ToString("yyyymmddhh"))))
        {
            Common.AssertFileContent(filePath, guid.ToString());
        }
    }
}