using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Configuration.Extensions;
using Serilog.Context;

namespace Serilog.ContextFileSink.Test;

public class SerilogContextPropertyFileSinkTest
{
    private readonly IConfigurationBuilder _configurationBuilder = new ConfigurationBuilder();

    private readonly LoggerConfiguration _loggerConfiguration;   

    private readonly HostApplicationBuilder _builder;

    private readonly string _logPath;

    public SerilogContextPropertyFileSinkTest()
    {
        _builder = new HostApplicationBuilder();
        _loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(_builder.Configuration);
        _logPath = $"{Directory.GetCurrentDirectory()}\\Logs";
    }

    private void SetLogContext(string logContextFileName)
    {
        var logContextRootPath = _builder.Environment.ContentRootPath;
        var logContextPath = $"{logContextRootPath}\\{logContextFileName}";
       
        var confBuilder = _configurationBuilder.AddJsonFile(logContextPath, optional: true, reloadOnChange: true);
        var conf = confBuilder.Build();
        conf.SetPath(["path", "pathFormat"], _logPath);
        _loggerConfiguration.ReadFrom.Configuration(conf);
    }

    private void SetLogContextWithProperty(string logContextFileName, string propertyName)
    {
        var logContextRootPath = _builder.Environment.ContentRootPath;
        var logContextPath = $"{logContextRootPath}\\{logContextFileName}";

        var confBuilder = _configurationBuilder.AddJsonFile(logContextPath, optional: true, reloadOnChange: true);
        var conf = confBuilder.Build();

        conf.SetWriteToContextPropertyName(propertyName);
        conf.SetPath(["path", "pathFormat"], _logPath);

        _loggerConfiguration.ReadFrom.Configuration(conf);
    }

    private void SetSerilog()
    {
        _builder.Logging.ClearProviders();
        
        var serilogLogger = _loggerConfiguration.CreateLogger();
        _builder.Logging.AddSerilog(serilogLogger);
    }    

    [Fact]
    public void TestContextPropertyFileSinkConfiguration()
    {
        var customProperty = "TestLoggerConf";

        //_builder.Services.AddLogInterception<TestLogger<double>>(logger =>
        //{
        //    return new SerilogPropertyLogger(logger, "CustomProperty", customProperty);
        //});

        _builder.Services.AddSingleton<TestLogger<double>>(); 
        _builder.Services.AddSingleton<TestLogger<double>>();

        SetLogContext("log.customproperty.json");     

        SetSerilog();

        var app = _builder.Build();

        var testLoggers = app.Services.GetServices<TestLogger<double>>();
        using(LogContext.PushProperty("CustomProperty", customProperty))
            foreach (var testLog in testLoggers)
            {
                testLog.LogWarning($"Warning {DateTime.Now} ");
            }

        var path = $"{_logPath}\\{customProperty}\\Warning";
        Assert.True(Directory.Exists(path));
        Assert.NotEmpty(Directory.EnumerateFiles(path));
    }

    [Fact]
    public void TestContextPropertyFileSink()
    {
        var customProperty = "TestLoggerWrite";

        //_builder.Services.AddLogInterception<TestLogger<string>>(logger =>
        //{
        //    return new SerilogPropertyLogger(logger, "CustomProperty", customProperty);
        //});

        _builder.Services.AddSingleton<TestLogger<string>>(); 
        _builder.Services.AddSingleton<TestLogger<string>>();        

        _loggerConfiguration.WriteTo.ContextPropertyFile("CustomProperty", $"{_logPath}" + "\\{CustomProperty}\\Info\\info_.log",
                    restrictedToMinimumLevel: Events.LogEventLevel.Information,
                     outputTemplate: "{Timestamp:o} [Thread:{ThreadId}] [{Level:u3}] {EventId} ({CustomProperty}) {Message}{NewLine}{Exception}",
                     rollingInterval: RollingInterval.Hour, retainedFileCountLimit:31, retainedFileTimeLimit:TimeSpan.FromHours(2));

        _loggerConfiguration.WriteTo.ContextPropertyFile("CustomProperty", $"{_logPath}" + "\\{CustomProperty}\\Warning\\warn_.log",
                    restrictedToMinimumLevel: Events.LogEventLevel.Warning,
                     outputTemplate: "{Timestamp:o} [Thread:{ThreadId}] [{Level:u3}] {EventId} ({CustomProperty}) {Message}{NewLine}{Exception}",
                     rollingInterval: RollingInterval.Hour, retainedFileCountLimit: 31, retainedFileTimeLimit: TimeSpan.FromHours(2));

        SetSerilog();

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

        //_builder.Services.AddLogInterception<TestLogger<object>>(logger =>
        //{
        //    return new SerilogPropertyLogger(logger, new Dictionary<string, object>() { { "CustomProperty", customProperty }, { "TestPushProp", "push" } });
        //});

        _builder.Services.AddSingleton<TestLogger<object>>();
        _builder.Services.AddSingleton<TestLogger<object>>();       

        SetLogContextWithProperty("log.contextproperty.json", "CustomProperty");

        SetSerilog();

        var app = _builder.Build();
        
        var testLoggers = app.Services.GetServices<TestLogger<object>>();
        using (LogContext.PushProperty("CustomProperty", customProperty))
            foreach (var testLog in testLoggers)
            {
                testLog.LogWarning($"Warning {DateTime.Now} ");
            }

        var path = $"{_logPath}\\{customProperty}\\Warning";
        Assert.True(Directory.Exists(path));
        Assert.NotEmpty(Directory.EnumerateFiles(path));
    }
}