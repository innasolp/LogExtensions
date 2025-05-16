using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog.Configuration.Extensions;
using Serilog.Context;

namespace Serilog.Configuration.Test;

public class PropertyLogTest
{
    private readonly HostApplicationBuilder _builder;

    private readonly string _logPath;

    private readonly SerilogConfigurationBuilder _serilogConfigurationBuilder;

    public PropertyLogTest()
    {
        _builder = new HostApplicationBuilder();
        _serilogConfigurationBuilder = new SerilogConfigurationBuilder(_builder.Configuration);
        _logPath = $"{Directory.GetCurrentDirectory()}\\Logs";
    }    

    [Fact]
    public void LogDirectoryIsNamedBySourceContext()
    {
        _builder.Services.AddSingleton<TestLogger<double>>();
        _builder.Services.AddSingleton<TestLogger<double>>();

        var logContextRootPath = _builder.Environment.ContentRootPath;
        var logContextPath = $"{logContextRootPath}\\log.property.json";

        var logPath = $"{_logPath}\\Test1";

        _serilogConfigurationBuilder.AddSourceContextContainsLogConfig(logContextPath,
            logPath,
            "TestLogger");

        _serilogConfigurationBuilder.SetSerilog(_builder.Logging);

        var app = _builder.Build();

        var testLoggers = app.Services.GetServices<TestLogger<double>>();

       //using (LogContext.PushProperty(propertyName, propertyValue))
        //{
            foreach (var testLog in testLoggers)
            {
                testLog.LogWarning($"Warning {DateTime.Now} ");
            }
       // }

        var path = $"{logPath}\\Warning";
        Assert.True(Directory.Exists(path));
        Assert.NotEmpty(Directory.EnumerateFiles(path));
    }

    [Fact]
    public void LogDirectoryIsNamedByProperty()
    {
        _builder.Services.AddSingleton<TestLogger<double>>();
        _builder.Services.AddSingleton<TestLogger<double>>();

        var logContextRootPath = _builder.Environment.ContentRootPath;
        var logContextPath = $"{logContextRootPath}\\log.property.json";

        var propertyName = "Test2";
        var propertyValue = "TestTest2";

        _serilogConfigurationBuilder.AddPropertiesLogConfig(logContextPath,
            $"{_logPath}\\{propertyName}",
            outputs: [propertyName],
            expressions: [
                new SerilogPropertyExpression(SerilogFunc.Contains, [new ContextProperty(propertyName),  propertyValue]), 
                new SerilogPropertyExpression(SerilogFunc.Contains, [SerilogExpressions.SourceContext,  "TestLogger"]) 
                ]);

        _serilogConfigurationBuilder.SetSerilog(_builder.Logging);

        var app = _builder.Build();

        var testLoggers = app.Services.GetServices<TestLogger<double>>();

        using (LogContext.PushProperty(propertyName, propertyValue))
        {
            foreach (var testLog in testLoggers)
            {
                testLog.LogInfo($"Info {DateTime.Now} ");
            }
        }

        var path = $"{_logPath}\\{propertyName}\\Info";
        Assert.True(Directory.Exists(path));
        Assert.NotEmpty(Directory.EnumerateFiles(path));
    }
}