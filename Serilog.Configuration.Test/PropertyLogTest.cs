using CustomConfigurationProvider;
using CustomJsonConfigurationProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Configuration.Extensions;
using Serilog.Context;

namespace Serilog.Configuration.Test;

public class PropertyLogTest
{
    private readonly HostApplicationBuilder _builder;

    private readonly string _logPath;

    private readonly  LoggerConfiguration _loggerConfiguration;

    public PropertyLogTest()
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

    private static void AssertFileContent(string filePath, string subString)
    {
        var copyFilePath = $"{Path.GetDirectoryName(filePath)}\\{Path.GetFileNameWithoutExtension(filePath)}_copy{Path.GetExtension(filePath)}";
        File.Copy(filePath, copyFilePath);

        var content = File.ReadAllText(copyFilePath);
        Assert.Contains(subString, content);

        File.Delete(copyFilePath);
    }

    [Fact]
    public void LogDirectoryIsLogPathFromConfigIfLogPathRuleAddedToConfig()
    {
        _builder.Services.AddSingleton<TestLogger<double>>();   

        var logPath = $"{_logPath}\\Test1";

        var serilogConfiguration = BuildSerilogConfiguraion([(source) => source.AddLogPathRule(logPath)]);
        SetSerilog(_builder.Logging);        

        var app = _builder.Build();

        var testLoggers = app.Services.GetServices<TestLogger<double>>();
       
        foreach (var testLog in testLoggers)   testLog.LogWarning($"Warning {DateTime.Now} ");   

        var path = $"{logPath}\\Warning";
        Assert.True(Directory.Exists(path));
        Assert.NotEmpty(Directory.EnumerateFiles(path));
    }

    [Fact]
    public void LogDirectoryIsNamedByPropertyValueIfPropertyNameRuleAddedToConfig()
    {
        _builder.Services.AddSingleton<TestLogger<double>>();        

        var propertyName = "Test2";
        var propertyValue = "TestTest2";

        var serilogConfiguration = BuildSerilogConfiguraion([
            (source) => source.AddLogPathRule($"{_logPath}\\{propertyName}"),
            (source) => source.AddOutputTemplatePropertyRule([propertyName]),
            (source) => source.AddExpressionFilterRule(new PropertyExpression(SerilogFunc.Contains, [new ContextProperty(propertyName),  propertyValue]))
            ]);
        SetSerilog(_builder.Logging);

        var app = _builder.Build();

        var testLoggers = app.Services.GetServices<TestLogger<double>>();

        using (LogContext.PushProperty(propertyName, propertyValue))
        {
            foreach (var testLog in testLoggers)
                testLog.LogInfo($"Info {DateTime.Now} ");
        }

        var path = $"{_logPath}\\{propertyName}\\Info";
        Assert.True(Directory.Exists(path));
        Assert.NotEmpty(Directory.EnumerateFiles(path));
    }

    [Fact]
    public void LogContainsPropertyNameIfPushedAndPropertyFilterAddedToConfig()
    {
        
        var propertyName = "Test3";
        var propertyValue = "TestTest3";

        var serilogConfiguration = BuildSerilogConfiguraion([
            (source) => source.AddLogPathRule($"{_logPath}\\{propertyName}"),
            (source) => source.AddContextPropertyNameRule(propertyName)
            ]);
        SetSerilog(_builder.Logging);

        using var app = _builder.Build();

        var testLoggers = app.Services.GetServices<ILogger<double>>();

        using (LogContext.PushProperty(propertyName, propertyValue))
        {
            foreach (var testLog in testLoggers)
                testLog.LogInformation($"Info {DateTime.Now} ");
        }

        var path = $"{_logPath}\\{propertyName}\\Info";
        Assert.True(Directory.Exists(path));

        var filePathes = Directory.GetFiles(path);
        Assert.NotEmpty(filePathes);

        foreach (var filePath in filePathes.Where(p => Path.GetExtension(p) == ".log"))
            AssertFileContent(filePath, propertyValue);
    }

    [Fact]
    public void LogIsInDirectoryIfSourceContextIsValid()
    {
        var propertyName = "Test4";

        var serilogConfiguration = BuildSerilogConfiguraion([
            (source) => source.AddLogPathRule($"{_logPath}\\{propertyName}"),
            (source) => source.AddContextPropertyNameRule(SerilogExpressions.SourceContext.Name),
            (source) => source.AddSourceContextFilterRule("TestLogger")
            ]);
        SetSerilog(_builder.Logging);

        _builder.Services.AddSingleton<TestLogger<double>>();

        using var app = _builder.Build();

        var testLoggers = app.Services.GetServices<TestLogger<double>>();

        foreach (var testLog in testLoggers)
                testLog.LogInfo($"Info {DateTime.Now} ");

        var path = $"{_logPath}\\{propertyName}\\Info";
        Assert.True(Directory.Exists(path));

        var filePathes = Directory.GetFiles(path);
        Assert.NotEmpty(filePathes);

        foreach (var filePath in filePathes.Where(p => Path.GetExtension(p) == ".log"))
            AssertFileContent(filePath, "TestLogger");
    }
}