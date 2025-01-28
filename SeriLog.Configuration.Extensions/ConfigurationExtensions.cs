using Microsoft.Extensions.Configuration;
using System.Numerics;

namespace Serilog.Configuration.Extensions;

public static class ConfigurationExtensions
{
    internal const string SourceContextStr = "$[SourceContext]";

    internal const string PropertyNameContextStr = "$[PropertyName]";
    internal const string PropertyValueContextStr = "$[PropertyValue]";
    internal const string FuncContextStr = "$[Func]";
    internal const string And = "and";
    

    public static bool SetSerilogLoggersFilterSourceContext(this IConfiguration configuration, string context)
    {
        var writeToSections = configuration.GetWriteToSections();
        if (writeToSections.Count == 0) return false;

        foreach (var writeTo in writeToSections)
        {
            var expressions = writeTo.GetSection("Args:configureLogger:Filter").GetChildren()
                .Select(c => c.GetSection("Args:expression"))
                .Where(e => e.Value != null && e.Value.Contains(SourceContextStr)).ToList();

            foreach (var expression in expressions)
            {
                expression.Value = expression.Value?.Replace(SourceContextStr, context);
            }
        }

        return true;
    }

    public static bool SetSerilogLoggersFilterFunc(this IConfiguration configuration, string? func, string? propertyName, object? propertyValue)
    {
        if(string.IsNullOrEmpty(func) && string.IsNullOrEmpty(propertyName) && propertyValue == null) return false;
       
        var writeToSections = configuration.GetWriteToSections();
        if (writeToSections.Count == 0) return false;

        foreach (var writeTo in writeToSections)
        {
            var expressions = writeTo.GetSection("Args:configureLogger:Filter").GetChildren()
                .Select(c => c.GetSection("Args:expression"))
                .Where(e => e.Value != null &&
                ( (!string.IsNullOrEmpty(func) && !e.Value.Contains(FuncContextStr))
                || (!string.IsNullOrEmpty(propertyName) && !e.Value.Contains(PropertyNameContextStr))
                || (propertyValue != null && e.Value.Contains(PropertyValueContextStr)) )).ToList();

            foreach (var expression in expressions)
            {
                expression.Value = expression.Value?
                    .Replace(FuncContextStr, !string.IsNullOrEmpty(func) ? func : "")
                    .Replace(PropertyNameContextStr, !string.IsNullOrEmpty(propertyName) ? propertyName : "")
                    .Replace(PropertyValueContextStr, propertyValue != null ? propertyValue.ToString() : "");
            }
        }

        return true;
    }

    public static bool SetSerilogOutputTemplateProperty(this IConfiguration configuration, string propertyName)
    {
        var writeToSections = configuration.GetWriteToSections();
        if (writeToSections.Count == 0) return false;

        foreach (var writeTo in writeToSections)
        {
            var pathes = writeTo.GetSection("Args:configureLogger:WriteTo").GetChildren()
                .Select(c => c.GetSection("Args:outputTemplate"))
               .Where(e => e.Value != null && e.Value.Contains(PropertyNameContextStr)).ToList();

            foreach (var expression in pathes)
            {
                expression.Value = expression.Value?.Replace(PropertyNameContextStr, propertyName);
            }
        }

        return true;
    }

    public static bool SetSerilogOutputTemplateProperties(this IConfiguration configuration, IEnumerable<string> properties)
    {
        var outputTemplates = configuration.GetWriteToOutputTemplateSections()
            .Where(e => e.Value != null && e.Value.Contains(PropertyNameContextStr)).ToList();

        var propertyString = string.Join(" ", properties.Select(p => $"{p}"));

        foreach (var outputTemplate in outputTemplates)
        {
            outputTemplate.Value = outputTemplate.Value?.Replace(PropertyNameContextStr, propertyString);            
        }

        return true;
    }

    public static bool AddSerilogEventIdFilters(this IConfiguration configuration, int eventId)
    {
        var expressions = configuration.GetWriteToExpressionSections();

        if (!expressions.Any()) return false;

        foreach (var expression in expressions)
        {
            if (expression.Value != null && expression.Value.Contains(And))
                expression.Value += $" {And} ";
            expression.Value = $"{expression.Value ?? ""}{SerilogExpressions.EventId}={eventId}";
        }

        return true;
    }

    public static bool AddSerilogExpressionFilter(this IConfiguration configuration, SerilogPropertyExpression serilogPropertyExpression)
    {
        var expressions = configuration.GetWriteToExpressionSections();

        if (!expressions.Any()) return false;

        foreach (var expression in expressions)
        {
            if (expression.Value != null && expression.Value.Contains(And))
                expression.Value += $" {And} ";
            expression.Value = $"{expression.Value ?? ""}{serilogPropertyExpression.GetExpression()}";
        }

        return true;
    }

    internal const string LoggerPathStr = "$[LoggerPath]";
    public static bool SetSerilogLoggersPath(this IConfiguration configuration, string path)
    {
        var writeToSections = configuration.GetWriteToSections();
        if (writeToSections.Count == 0) return false;

        foreach (var writeTo in writeToSections)
        {
            var pathes = writeTo.GetSection("Args:configureLogger:WriteTo").GetChildren()
                .Select(c => c.GetSection("Args:path"))
                .Where(e => e.Value != null && e.Value.Contains(LoggerPathStr)).ToList();

            foreach (var expression in pathes)
            {
                expression.Value = expression.Value?.Replace(LoggerPathStr, path);
            }
        }

        return true;
    }

    private static IList<IConfigurationSection> GetWriteToSections(this IConfiguration configuration)
    {
        var serilogSection = configuration.GetSection("Serilog");
        if (serilogSection == null) return [];

        return serilogSection.GetSection("WriteTo").GetChildren().ToList();
    }
    
    private static IList<IConfigurationSection> GetWriteToExpressionSections(this IConfiguration configuration)
    {
        var writeToSections = configuration.GetWriteToSections();
        return writeToSections.Select(writeTo => writeTo.GetSection("Args:configureLogger:Filter")).SelectMany(f => f.GetChildren())
                .Select(c => c.GetSection("Args:expression")).ToList();
    }
    
    private static IList<IConfigurationSection> GetWriteToOutputTemplateSections(this IConfiguration configuration)
    {
        var writeToSections = configuration.GetWriteToSections();
        return writeToSections.SelectMany(writeTo => writeTo.GetSection("Args:configureLogger:WriteTo").GetChildren())
               .Select(c => c.GetSection("Args:outputTemplate")).ToList();
        //      .Where(e => e.Value != null && e.Value.Contains(PropertyNameContextStr)).ToList();
    }

    public static string BuildLogPath(this string appPath)
    {
        return !string.IsNullOrEmpty(appPath) ? $"{appPath}/Logs" : "Logs";
    }    
}
