using Microsoft.Extensions.Configuration;
using System.Numerics;

namespace Serilog.Configuration.Extensions;

public static class ConfigurationExtensions
{
    internal const string SourceContextStr = "$[SourceContext]";

    internal const string PropertyNameContextStr = "$[PropertyName]";
    internal const string ContextPropertyNameContextStr = "$[ContextPropertyName]";
    internal const string PropertyValueContextStr = "$[PropertyValue]";
    internal const string FuncContextStr = "$[Func]";
    internal const string And = "and";

    internal const string ConfigureLoggerSectionName = "configureLogger";
    internal const string FilterSectionName = "Filter";
    internal const string ExpressionSectionName = "expression";
    internal const string WriteToSectionName = "WriteTo";
    internal const string OutputTemplateSectionName = "outputTemplate";
    internal const string PropertyNameSectionName = "propertyName";

    public static bool SetSerilogLoggersFilterSourceContext(this IConfiguration configuration, string context)
    {
        var expressionSections = configuration.GetWriteToExpressionSections().Where(e => e.Value != null && e.Value.Contains(SourceContextStr));

        foreach (var expression in expressionSections)
        {
            expression.Value = expression.Value?.Replace(SourceContextStr, context);            
        }

        return true;
    }

    public static bool SetSerilogLoggersFilterFunc(this IConfiguration configuration, string? func, string? propertyName, object? propertyValue)
    {
        if (string.IsNullOrEmpty(func) && string.IsNullOrEmpty(propertyName) && propertyValue == null) return false;

        var writeToSections = configuration.GetWriteToSections();
        if (writeToSections.Count == 0) return false;

        var expressionSections = configuration.GetWriteToExpressionSections().Where(e => e.Value != null &&
                ((!string.IsNullOrEmpty(func) && !e.Value.Contains(FuncContextStr))
                || (!string.IsNullOrEmpty(propertyName) && !e.Value.Contains(PropertyNameContextStr))
                || (propertyValue != null && e.Value.Contains(PropertyValueContextStr))));


        foreach (var expression in expressionSections)
        {
            expression.Value = expression.Value?
                .Replace(FuncContextStr, !string.IsNullOrEmpty(func) ? func : "")
                .Replace(PropertyNameContextStr, !string.IsNullOrEmpty(propertyName) ? propertyName : "")
                .Replace(PropertyValueContextStr, propertyValue != null ? propertyValue.ToString() : "");
        }

        return true;
    }

    public static bool SetSerilogOutputTemplateProperty(this IConfiguration configuration, string propertyName)
    {
        var outputTemplateSections = configuration.GetWriteToOutputTemplateSections().Where(e => e.Value != null && e.Value.Contains(PropertyNameContextStr));

        foreach (var expression in outputTemplateSections)
        {
            expression.Value = expression.Value?.Replace(PropertyNameContextStr, propertyName);
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
    public static bool SetSerilogLoggersPath(this IConfiguration configuration, string[] pathSectionNames, string path)
    {
        var pathSections = configuration.GetWriteToPathSections(pathSectionNames);
        if (pathSections.Count == 0) return false;

        foreach (var pathSection in pathSections.Where(e => e.Value != null && e.Value.Contains(LoggerPathStr)))
        {
            pathSection.Value = pathSection.Value?.Replace(LoggerPathStr, path);
        }

        return true;
    }

    public static bool SetSerilogLoggersPath(this IConfiguration configuration, string path)
    {
        return SetSerilogLoggersPath(configuration, ["path"], path);
    }

    public static bool SetSerilogWriteToContextPropertyName(this IConfiguration configuration, string contextPropertyName)
    {
        var writeToSections = configuration.GetWriteToSections();
        if (writeToSections.Count == 0) return false;

        foreach (var writeTo in writeToSections)
        {
            foreach (var expression in writeTo.GetWriteToExpressionSections().Where(s=>s.Value?.Contains(ContextPropertyNameContextStr) == true))
                expression.Value = expression?.Value?.Replace(ContextPropertyNameContextStr, contextPropertyName);

            foreach (var outputTemplate in writeTo.GetWriteToOutputTemplateSections().Where(s => s.Value?.Contains(ContextPropertyNameContextStr) == true))
                outputTemplate.Value = outputTemplate?.Value?.Replace(ContextPropertyNameContextStr, contextPropertyName);

            foreach (var propertyName in writeTo.GetWriteToSectionsByName(PropertyNameSectionName).Where(s => s.Value?.Contains(ContextPropertyNameContextStr) == true))
                propertyName.Value = propertyName.Value?.Replace(ContextPropertyNameContextStr, contextPropertyName);

            foreach (var path in writeTo.GetWriteToPathSections(["path", "pathFormat"]).Where(s => s.Value?.Contains(ContextPropertyNameContextStr) == true))
                path.Value = path?.Value?.Replace(ContextPropertyNameContextStr, contextPropertyName);
        }

       return true;
    }

    private static List<IConfigurationSection> GetWriteToSections(this IConfiguration configuration)
    {
        var serilogSection = configuration.GetSection("Serilog");
        if (serilogSection == null) return [];

        return serilogSection.GetSection(WriteToSectionName).GetChildren().ToList();
    }
    
    private static List<IConfigurationSection> GetWriteToExpressionSections(this IConfiguration configuration)
    {
        var writeToSections = configuration.GetWriteToSections();
        return [.. writeToSections.Select(writeTo => writeTo.GetSection($"Args:{ConfigureLoggerSectionName}:{FilterSectionName}"))
            .SelectMany(f => f.GetChildren())
            .Select(c => c.GetSection($"Args:{ExpressionSectionName}"))];
    }

    private static List<IConfigurationSection> GetWriteToExpressionSections(this IConfigurationSection writeToSection)
    {
        return [.. writeToSection.GetSection($"Args:{ConfigureLoggerSectionName}:{FilterSectionName}")
            .GetChildren()
            .Select(c => c.GetSection($"Args:{ExpressionSectionName}"))];
    }

    private static List<IConfigurationSection> GetWriteToOutputTemplateSections(this IConfiguration configuration)
    {
        var writeToSections = configuration.GetWriteToSections();
        return [.. writeToSections.SelectMany(writeTo => writeTo.GetSection($"Args:{ConfigureLoggerSectionName}:{WriteToSectionName}").GetChildren())
            .Select(c => c.GetSection($"Args:{OutputTemplateSectionName}"))];
        //      .Where(e => e.Value != null && e.Value.Contains(PropertyNameContextStr)).ToList();
    }

    private static List<IConfigurationSection> GetWriteToOutputTemplateSections(this IConfigurationSection writeToSection)
    {
        return [.. writeToSection.GetSection($"Args:{ConfigureLoggerSectionName}:{WriteToSectionName}").
            GetChildren().
            Select(c => c.GetSection($"Args:{OutputTemplateSectionName}"))];
    }

    private static List<IConfigurationSection> GetWriteToSectionsByName(this IConfigurationSection writeToSection, string sectionName)
    {
        return [.. writeToSection.GetSection($"Args:{ConfigureLoggerSectionName}:{WriteToSectionName}").
            GetChildren().
            Select(c => c.GetSection($"Args:{sectionName}"))];
    }

    private static List<IConfigurationSection> GetWriteToPathSections(this IConfiguration configuration, string[] pathSectionNames)
    {
        var writeToSections = configuration.GetWriteToSections();
        if (writeToSections.Count == 0) return [];

        var pathes = new List<IConfigurationSection>();
        foreach (var writeTo in writeToSections)
        {
            pathes.AddRange(pathSectionNames.SelectMany(s => writeTo.GetSection($"Args:{ConfigureLoggerSectionName}:{WriteToSectionName}")
            .GetChildren()
            .Select(c => c.GetSection($"Args:{s}"))));
        }

        return pathes;
    }

    private static List<IConfigurationSection> GetWriteToPathSections(this IConfigurationSection writeTo, string[] pathSectionNames)
    {
       return [.. pathSectionNames.SelectMany(s => writeTo.GetSection($"Args:{ConfigureLoggerSectionName}:{WriteToSectionName}")
        .GetChildren()
        .Select(c => c.GetSection($"Args:{s}")))];        
    }

    public static string BuildLogPath(this string appPath)
    {
        return !string.IsNullOrEmpty(appPath) ? $"{appPath}/Logs" : "Logs";
    }    
}
