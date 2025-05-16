using Microsoft.Extensions.Configuration;

namespace Serilog.Configuration.Extensions;

public static class ConfigurationExtensions
{    
    internal const string And = "and";    

    public static bool SetSourceContext(this IConfiguration configuration, string context)
    {
        var expressionSections = configuration.GetWriteToExpressionSections().Where(e => e.Value != null && !e.Value.Contains(SerilogExpressions.SourceContext.Name));

        foreach (var expression in expressionSections)        
            expression.AddExpressionFilter(new SerilogPropertyExpression(SerilogFunc.Contains, [SerilogExpressions.SourceContext, context]));           
    

        return true;
    }

    public static bool SetOutputTemplateProperties(this IConfiguration configuration, IEnumerable<string> properties)
    {
        var outputTemplates = configuration.GetWriteToOutputTemplateSections()
            .Where(e => e.Value != null && e.Value.Contains(ContextVariables.PropertyNameContextStr)).ToList();

        var propertyString = string.Join(" ", properties.Select(p => $"{p}"));

        foreach (var outputTemplate in outputTemplates)
        {
            outputTemplate.Value = outputTemplate.Value?.Replace(ContextVariables.PropertyNameContextStr, propertyString);            
        }

        return true;
    }

    public static bool AddEventIdFilters(this IConfiguration configuration, int eventId)
    {
        var expressions = configuration.GetWriteToExpressionSections();

        if (expressions.Count == 0) return false;

        foreach (var expression in expressions)
        {
            if (expression.Value != null && expression.Value.Contains(And))
                expression.Value += $" {And} ";
            expression.Value = $"{expression.Value ?? ""}{SerilogExpressions.EventId}={eventId}";
        }

        return true;
    }

    public static bool AddExpressionFilter(this IConfiguration configuration, SerilogPropertyExpression serilogPropertyExpression)
    {
        var expressions = configuration.GetWriteToExpressionSections();

        if (expressions.Count == 0) return false;

        foreach (var expression in expressions)
        {
            expression.AddExpressionFilter(serilogPropertyExpression);
        }

        return true;
    }

    public static bool AddExpressionFilter(this IConfiguration configuration, string func, string propertyName, object propertyValue)
    {
        var expressions = configuration.GetWriteToExpressionSections();

        if (expressions.Count == 0) return false;

        foreach (var expression in expressions)
        {
            expression.AddExpressionFilter(func, propertyName, propertyValue);
        }

        return true;
    }

    public static void AddExpressionFilter(this IConfigurationSection expression, SerilogPropertyExpression serilogPropertyExpression)
    {
        var expressionStr = serilogPropertyExpression.GetExpression();
        if (string.IsNullOrEmpty(expressionStr))
            return;
        if (!string.IsNullOrEmpty(expression.Value))
            expression.Value += $" {And} ";
        expression.Value = $"{expression.Value ?? ""}{expressionStr}";
    }

    public static void AddExpressionFilter(this IConfigurationSection expression, string func, string propertyName, object propertyValue)
    {
        expression.AddExpressionFilter(new SerilogPropertyExpression(func,[ propertyName, propertyValue]));
    }

    public static bool SetPath(this IConfiguration configuration, string[] pathSectionNames, string path)
    {
        var pathSections = configuration.GetWriteToPathSections(pathSectionNames);
        if (pathSections.Count == 0) return false;

        foreach (var pathSection in pathSections.Where(e => e.Value != null && e.Value.Contains(ContextVariables.LoggerPathStr)))
        {
            pathSection.Value = pathSection.Value?.Replace(ContextVariables.LoggerPathStr, path);
        }

        return true;
    }

    public static bool SetPath(this IConfiguration configuration, string path)
    {
        return SetPath(configuration, ["path"], path);
    }

    public static bool SetWriteToContextPropertyName(this IConfiguration configuration, string contextPropertyName)
    {
        var writeToSections = configuration.GetWriteToSections();
        if (writeToSections.Count == 0) return false;

        foreach (var writeTo in writeToSections)
        {
            foreach (var expression in writeTo.GetWriteToExpressionSections().Where(s => s.Value?.Contains(ContextVariables.PropertyNameContextStr) == true))           
                expression.Value = expression?.Value?.Replace(ContextVariables.PropertyNameContextStr, contextPropertyName); 

            foreach (var outputTemplate in writeTo.GetWriteToOutputTemplateSections().Where(s => s.Value?.Contains(ContextVariables.PropertyNameContextStr) == true))
                outputTemplate.Value = outputTemplate?.Value?.Replace(ContextVariables.PropertyNameContextStr, contextPropertyName);

            foreach (var propertyName in writeTo.GetWriteToPropertyNameSections().Where(s => s.Value?.Contains(ContextVariables.PropertyNameContextStr) == true))
                propertyName.Value = propertyName.Value?.Replace(ContextVariables.PropertyNameContextStr, contextPropertyName);

            foreach (var path in writeTo.GetWriteToPathSections(["path", "pathFormat"]).Where(s => s.Value?.Contains(ContextVariables.PropertyNameContextStr) == true))
                path.Value = path?.Value?.Replace(ContextVariables.PropertyNameContextStr, contextPropertyName);
        }

       return true;
    }

    
}
