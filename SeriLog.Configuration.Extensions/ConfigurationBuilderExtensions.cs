using Microsoft.Extensions.Configuration;

namespace Serilog.Configuration.Extensions;

public static class ConfigurationBuilderExtensions
{
    public static IConfiguration BuildConfWithSerilogSourceContext(this IConfigurationBuilder configurationBuilder, string confPath, string context, string logPath)
    {
        var sysLogConfBuilder = configurationBuilder.AddJsonFile(confPath, optional: true, reloadOnChange: true);//.AddEnvironmentVariables();

        var sysLogconf = sysLogConfBuilder.Build();
        sysLogconf.SetSerilogLoggersFilterSourceContext(context);
        sysLogconf.SetSerilogLoggersPath(logPath);

        return sysLogconf;
    }

    public static IConfiguration BuildConfWithSerilogFunc(this IConfigurationBuilder configurationBuilder, string confPath, string? func, string? propertyName, string? propertyValue, string logPath)
    {
        var sysLogConfBuilder = configurationBuilder.AddJsonFile(confPath, optional: true, reloadOnChange: true);//.AddEnvironmentVariables();

        var sysLogconf = sysLogConfBuilder.Build();
        sysLogconf.SetSerilogLoggersFilterFunc(func, propertyName, propertyValue);
        sysLogconf.SetSerilogOutputTemplateProperty(propertyName);
        sysLogconf.SetSerilogLoggersPath(logPath);

        return sysLogconf;
    }
   
    public static IConfiguration BuildConfWithSerilogProperties(this IConfigurationBuilder configurationBuilder,
        string confPath,
        IEnumerable<SerilogPropertyExpression> expressions,
        string logPath)
    {
        var sysLogConfBuilder = configurationBuilder.AddJsonFile(confPath, optional: true, reloadOnChange: true);//.AddEnvironmentVariables();

        var conf = sysLogConfBuilder.Build();

        var first = expressions.First();
        conf.SetSerilogLoggersFilterFunc(first.Func, first.PropertyName, first.Value);

        foreach (var expresssion in expressions.Skip(1))
        {
            conf.AddSerilogExpressionFilter(expresssion);
        }

        var outputs = expressions.Where(e => e.SetToOutput).Select(e => e.PropertyName).ToArray();
        conf.SetSerilogOutputTemplateProperties(outputs);

        conf.SetSerilogLoggersPath(logPath);

        return conf;
    }

    public static List<IConfiguration> GetSerilogConfs(this IConfigurationBuilder configurationBuilder, string path)
    {
        var configurations = new List<IConfiguration>();
        var jsonFiles = Directory.GetFiles(path, "*.json");
        foreach (var jsonFile in jsonFiles)
        {
            var logConfBuilder = configurationBuilder.AddJsonFile(jsonFile, optional: true, reloadOnChange: true);//.AddEnvironmentVariables();
            var logconf = logConfBuilder.Build();
            var serilogSection = logconf.GetSection("Serilog");
            if (serilogSection != null && serilogSection.GetChildren().Any())
                configurations.Add(logconf);
        }
        return configurations;
    }
}
