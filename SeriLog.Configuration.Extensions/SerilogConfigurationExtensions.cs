using Microsoft.Extensions.Configuration;

namespace Serilog.Configuration.Extensions;

public static class SerilogConfigurationExtensions
{  
    public static LoggerConfiguration SetSystemsSerilogConfig(this LoggerConfiguration serilogConfiguration, 
        IConfigurationBuilder configurationBuilder, 
        string logContextPath, 
        string logsPath)
    {
        var systemConf = configurationBuilder.BuildConfWithSerilogSourceContext(logContextPath, "Microsoft", $"{logsPath}/system");
        serilogConfiguration = serilogConfiguration.ReadFrom.Configuration(systemConf);

        var httpConf = configurationBuilder.BuildConfWithSerilogSourceContext(logContextPath, "System.Net.Http", $"{logsPath}/net.http");
        return serilogConfiguration.ReadFrom.Configuration(httpConf);
    }

    public static LoggerConfiguration SetSystemsSerilogConfig(this LoggerConfiguration serilogConfiguration,
        IConfigurationBuilder configurationBuilder,
        string logContextPath,
        string logsPath,
        string? func)
    {
        var systemConf = configurationBuilder.BuildConfWithSerilogProperties(logContextPath, $"{logsPath}/system",
            expressions:[new SerilogPropertyExpression (func, ["SourceContext", "Microsoft"])]);
        serilogConfiguration = serilogConfiguration.ReadFrom.Configuration(systemConf);

        var httpConf = configurationBuilder.BuildConfWithSerilogProperties(logContextPath, $"{logsPath}/net.http",
            expressions: [new SerilogPropertyExpression(func, ["SourceContext", "System.Net.Http"])]);
        return serilogConfiguration.ReadFrom.Configuration(httpConf);
    }

    public static LoggerConfiguration SetSerilogConfigForServiceBySourceContext(this LoggerConfiguration serilogConfiguration,
        IConfigurationBuilder configurationBuilder, 
        string logContextPath, 
        string logPath, 
        string context)
    {
        var systemConf = configurationBuilder.BuildConfWithSerilogSourceContext(logContextPath, context, logPath);
        return serilogConfiguration.ReadFrom.Configuration(systemConf);
    }   
    
    public static LoggerConfiguration SetSerilogConfigForServiceByFunc(this LoggerConfiguration serilogConfiguration,
        IConfigurationBuilder configurationBuilder,
        string logContextPath,
        string logPath, 
        string func, 
        string propertyName,
        string propertyValue,
        IEnumerable<string>? outputProperties = null)
    {
        return serilogConfiguration.SetSerilogConfigForServiceByFunc(configurationBuilder,
            logContextPath, 
            logPath,
            func,
            new ContextProperty(propertyName),
            propertyValue,outputProperties);
    }

    public static LoggerConfiguration SetSerilogConfigForServiceByFunc(this LoggerConfiguration serilogConfiguration,
        IConfigurationBuilder configurationBuilder,
        string logContextPath,
        string logPath,
        string func,
        ContextProperty property,
        string propertyValue,
        IEnumerable<string>? outputProperties = null)
    {
        var systemConf = configurationBuilder.BuildConfWithSerilogProperties(logContextPath, logPath,
            expressions: [new SerilogPropertyExpression(func, [property, propertyValue])],
            outputProperties);
        return serilogConfiguration.ReadFrom.Configuration(systemConf);
    }

    public static LoggerConfiguration SetSerilogConfigProperties(this LoggerConfiguration serilogConfiguration,
       IConfigurationBuilder configurationBuilder,
       string logContextPath,
       string logPath,       
       IEnumerable<string>? propertyToOutputs = null,
       IEnumerable<SerilogPropertyExpression>? expressions = null)
    {
        var systemConf = configurationBuilder.BuildConfWithSerilogProperties(logContextPath,
            logPath,             
            expressions,
            propertyToOutputs);

        return serilogConfiguration.ReadFrom.Configuration(systemConf);
    }
}
