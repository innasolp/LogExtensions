using Microsoft.Extensions.Configuration;

namespace Serilog.Configuration.Extensions;

public static class SerilogConfigurationExtensions
{
    public static LoggerConfiguration SetSystemsSerilogConfig(this LoggerConfiguration serilogConfiguration, 
        IConfigurationBuilder configurationBuilder,
        string logContextPath)
    {
        return serilogConfiguration.SetSystemsSerilogConfig(configurationBuilder,logContextPath, "Logs");
    }

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
        var systemConf = configurationBuilder.BuildConfWithSerilogFunc(logContextPath, func, "SourceContext", "Microsoft", $"{logsPath}/system");
        serilogConfiguration = serilogConfiguration.ReadFrom.Configuration(systemConf);

        var httpConf = configurationBuilder.BuildConfWithSerilogFunc(logContextPath, func, "SourceContext", "System.Net.Http", $"{logsPath}/net.http");
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
        string? func, 
        string? propertyName,
        string? propertyValue)
    {
        var systemConf = configurationBuilder.BuildConfWithSerilogFunc(logContextPath,func, propertyName, propertyValue, logPath);
        return serilogConfiguration.ReadFrom.Configuration(systemConf);
    }
    
    public static LoggerConfiguration SetSerilogConfigProperties(this LoggerConfiguration serilogConfiguration,
       IConfigurationBuilder configurationBuilder,
       string logContextPath,
       string logPath,
       IEnumerable<SerilogPropertyExpression> expressions)
    {
        var systemConf = configurationBuilder.BuildConfWithSerilogProperties(logContextPath, expressions, logPath);
        return serilogConfiguration.ReadFrom.Configuration(systemConf);
    }
}
