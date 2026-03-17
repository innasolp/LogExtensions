using CustomConfigurationProvider;
using CustomJsonConfigurationProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Configuration.Extensions;
using Serilog.Context;

namespace Serilog.ContextFileSink.Test;

public class SerilogContextPropertyFileSinkTest
{
    private readonly LoggerConfiguration _loggerConfiguration;   

    private readonly HostApplicationBuilder _builder;

    private readonly string _logPath;

    public SerilogContextPropertyFileSinkTest()
    {
        _builder = new HostApplicationBuilder();
        _loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(_builder.Configuration);
        _logPath = $"{Directory.GetCurrentDirectory()}\\Logs";
    }    

    private LoggerConfiguration BuildSerilogConfiguraion(string logContextFile, IEnumerable<Action<ICustomConfigurationSource>> addRuleActions)
    {
        var _configurationBuilder = new ConfigurationBuilder();
        var source = _configurationBuilder.AddCustomJsonConfigurationProvider(logContextFile);
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
    public void TestContextPropertyFileSinkConfiguration()
    {
        var customProperty = "TestLoggerConf";
        var logPath = $"{_logPath}\\Test1";
        
        _builder.Services.AddSingleton<TestLogger<double>>(); 
        _builder.Services.AddSingleton<TestLogger<double>>();

        var serilogConfiguration = BuildSerilogConfiguraion("log.customproperty.json", [(source) => source.AddLogPathRule(logPath)]);
        SetSerilog(_builder.Logging);

        var app = _builder.Build();

        var testLoggers = app.Services.GetServices<TestLogger<double>>();
        using(LogContext.PushProperty("CustomProperty", customProperty))
            foreach (var testLog in testLoggers)
            {
                testLog.LogWarning($"Warning {DateTime.Now} ");
            }

        var path = $"{logPath}\\{customProperty}\\Warning";
        Assert.True(Directory.Exists(path));
        Assert.NotEmpty(Directory.EnumerateFiles(path));
    }

    [Fact]
    public void TestContextPropertyFileSink()
    {
        var customProperty = "TestLoggerWrite";
        var logPath = $"{_logPath}\\Test2";

        _builder.Services.AddSingleton<TestLogger<string>>(); 
        _builder.Services.AddSingleton<TestLogger<string>>();

        _loggerConfiguration.WriteTo.ContextPropertyFile("CustomProperty", $"{_logPath}" + "\\{CustomProperty}\\Info\\info_.log",
                    restrictedToMinimumLevel: Events.LogEventLevel.Information,
                     outputTemplate: "{Timestamp:o} [Thread:{ThreadId}] [{Level:u3}] {EventId} ({CustomProperty}) {Message}{NewLine}{Exception}",
                     rollingInterval: RollingInterval.Hour, retainedFileCountLimit: 31, retainedFileTimeLimit: TimeSpan.FromHours(2));

        _loggerConfiguration.WriteTo.ContextPropertyFile("CustomProperty", $"{_logPath}" + "\\{CustomProperty}\\Warning\\warn_.log",
                    restrictedToMinimumLevel: Events.LogEventLevel.Warning,
                     outputTemplate: "{Timestamp:o} [Thread:{ThreadId}] [{Level:u3}] {EventId} ({CustomProperty}) {Message}{NewLine}{Exception}",
                     rollingInterval: RollingInterval.Hour, retainedFileCountLimit: 31, retainedFileTimeLimit: TimeSpan.FromHours(2));
        SetSerilog(_builder.Logging);

        var app = _builder.Build();

        var testLoggers = app.Services.GetServices<TestLogger<string>>();
        using (LogContext.PushProperty("CustomProperty", customProperty))
            foreach (var testLog in testLoggers)
            {
                testLog.LogInfo($"Info {DateTime.Now} ");
                testLog.LogWarning($"Warning {DateTime.Now} ");
            }

        var warnPath = $"{_logPath}\\{customProperty}\\Warning";
        Assert.True(Directory.Exists(warnPath));
        Assert.NotEmpty(Directory.EnumerateFiles(warnPath));

        var infoPath = $"{_logPath}\\{customProperty}\\Info";
        Assert.True(Directory.Exists(infoPath));
        Assert.NotEmpty(Directory.EnumerateFiles(infoPath));
    }

    [Fact]
    public void TestContextPropertyFileSinkConfigurationWithContext()
    {
        var customProperty = "TestLoggerConfContext";
        var logPath = $"{_logPath}\\Test3";

        _builder.Services.AddSingleton<TestLogger<object>>();
        _builder.Services.AddSingleton<TestLogger<object>>();

        var serilogConfiguration = BuildSerilogConfiguraion("log.contextproperty.json", 
            [
            (source) => source.AddLogPathRule(logPath),
            (source) => source.AddContextPropertyNameRule("CustomProperty")
            ]);
        SetSerilog(_builder.Logging);

        var app = _builder.Build();
        
        var testLoggers = app.Services.GetServices<TestLogger<object>>();
        using (LogContext.PushProperty("CustomProperty", customProperty))
            foreach (var testLog in testLoggers)
            {
                testLog.LogWarning($"Warning {DateTime.Now} ");
            }

        var path = $"{logPath}\\{customProperty}\\Warning";
        Assert.True(Directory.Exists(path));
        Assert.NotEmpty(Directory.EnumerateFiles(path));
    }
}