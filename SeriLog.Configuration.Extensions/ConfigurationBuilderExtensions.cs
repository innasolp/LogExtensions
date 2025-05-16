using Microsoft.Extensions.Configuration;

namespace Serilog.Configuration.Extensions;

public static class ConfigurationBuilderExtensions
{
    public static IConfiguration BuildConfWithSerilogSourceContext(this IConfigurationBuilder configurationBuilder, string confPath, string context, string logPath)
    {
        var sysLogConfBuilder = configurationBuilder.AddJsonFile(confPath, optional: true, reloadOnChange: true);

        var sysLogconf = sysLogConfBuilder.Build();
        sysLogconf.SetSourceContext(context);
        sysLogconf.SetPath(logPath);

        return sysLogconf;
    }

    public static IConfiguration BuildConfWithSerilogProperties(this IConfigurationBuilder configurationBuilder,
        string confPath, 
        string logPath,
        IEnumerable<SerilogPropertyExpression>? expressions = null,
        IEnumerable<string>? properties = null)
    {
        var sysLogConfBuilder = configurationBuilder.AddJsonFile(confPath, optional: true, reloadOnChange: true);

        var sysLogconf = sysLogConfBuilder.Build(); 
        
        sysLogconf.SetPath(logPath);

        expressions?.ToList().ForEach(e=>sysLogconf.AddExpressionFilter(e));

        if(properties != null) sysLogconf.SetOutputTemplateProperties(properties);       

        return sysLogconf;
    }   
    

    public static List<IConfiguration> GetSerilogConfs(this IConfigurationBuilder configurationBuilder, string path)
    {
        var configurations = new List<IConfiguration>();
        var jsonFiles = Directory.GetFiles(path, "*.json");
        foreach (var jsonFile in jsonFiles)
        {
            var logConfBuilder = configurationBuilder.AddJsonFile(jsonFile, optional: true, reloadOnChange: true);
            var logconf = logConfBuilder.Build();
            var serilogSection = logconf.GetSection("Serilog");
            if (serilogSection != null && serilogSection.GetChildren().Any())
                configurations.Add(logconf);
        }
        return configurations;
    }
}
