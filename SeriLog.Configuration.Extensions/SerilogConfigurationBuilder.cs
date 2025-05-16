using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Serilog.Configuration.Extensions;

public class SerilogConfigurationBuilder(IConfiguration configuration)
{
    private readonly IConfigurationBuilder _configurationBuilder = new ConfigurationBuilder();

    public LoggerConfiguration LoggerConfiguration { get; } = new LoggerConfiguration().ReadFrom.Configuration(configuration);
   

    public LoggerConfiguration AddPropertiesLogConfig(string logContextPath, string logPath, IEnumerable<SerilogPropertyExpression> expressions)
    {
        return LoggerConfiguration.SetSerilogConfigProperties(_configurationBuilder, logContextPath, logPath, expressions: expressions);
    }

    public LoggerConfiguration AddPropertiesLogConfig(string logContextPath, string logPath,
        IEnumerable<string>? outputs = null,
        IEnumerable<SerilogPropertyExpression>? expressions = null
        )
    {
        return LoggerConfiguration.SetSerilogConfigProperties(_configurationBuilder, 
            logContextPath, 
            logPath,             
            propertyToOutputs:outputs,
            expressions: expressions);
    }

    public LoggerConfiguration AddSourceContextContainsLogConfig(string logContextPath, string logPath, string? propertyValue)
    {
        return LoggerConfiguration.SetSerilogConfigForServiceByFunc(_configurationBuilder, 
            logContextPath, 
            logPath, 
            Enum.GetName(SerilogFunc.Contains), 
            SerilogExpressions.SourceContext, 
            propertyValue);
    }

    public LoggerConfiguration AddFuncLogConfig(string logContextPath, string logPath, string func, string propertyName, string? propertyValue,
        IEnumerable<string>? propertyToOutputs = null)
    {
        return LoggerConfiguration.SetSerilogConfigForServiceByFunc(_configurationBuilder,
            logContextPath,
            logPath,
            func,
            propertyName,
            propertyValue,
            propertyToOutputs);
    }

    public LoggerConfiguration AddSystemConfigs(string logContextPath, string logPath)
    {
        return LoggerConfiguration.SetSystemsSerilogConfig(_configurationBuilder, logContextPath, logPath);
    }    

    public ILoggingBuilder SetSerilog(ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.ClearProviders();
        var logger = LoggerConfiguration.CreateLogger();
        return loggingBuilder.AddSerilog(logger);
    }

    public LoggerConfiguration AddContextPropertyConfig(string logContextPath, 
        string logPath,
        string propertyName,
        string? sourceContext = null,
        string[]? outputProperties = null,
        IEnumerable<SerilogPropertyExpression>? expressions = null)
    {
        var conf = _configurationBuilder.AddJsonFile(logContextPath).Build();

        conf.SetPath(["path", "pathFormat"], logPath);

        conf.SetWriteToContextPropertyName(propertyName);

        if(outputProperties != null)
            conf.SetOutputTemplateProperties(outputProperties);

        if (!string.IsNullOrEmpty(sourceContext)) conf.SetSourceContext(sourceContext);

        if (expressions != null)
            foreach (var expression in expressions)
                conf.AddExpressionFilter(expression);

        return LoggerConfiguration.ReadFrom.Configuration(conf);
    }
}
