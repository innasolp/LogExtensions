using CustomConfigurationProvider;
using Serilog.Configuration.Extensions.ConfigurationRules;

namespace Serilog.Configuration.Extensions;

public static class CustomConfigurationSourceExtensions
{
    public static void AddSourceContextFilterRule(this ICustomConfigurationSource configurationSource, string context)
    {
        configurationSource.AddCustomConfigurationRule(new PropertyExpressionFilterRule(
            new PropertyExpression(Enum.GetName(SerilogFunc.Contains), [SerilogExpressions.SourceContext, context])));
    }

    public static void AddOutputTemplatePropertyRule(this ICustomConfigurationSource configurationSource, IEnumerable<string> properties)
    {
        configurationSource.AddCustomConfigurationRule(new OutputTemplateRule(properties));
    }

    public static void AddEventIdFilterRule(this ICustomConfigurationSource configurationSource, int eventId)
    {
        configurationSource.AddCustomConfigurationRule(new EventIdFilterRule(eventId));
    }

    public static void AddExpressionFilterRule(this ICustomConfigurationSource configurationSource, PropertyExpression propertyExpression)
    {
        configurationSource.AddCustomConfigurationRule(new PropertyExpressionFilterRule(propertyExpression));
    }

    public static void AddExpressionFilterRule(this ICustomConfigurationSource configurationSource, string func, string propertyName, object propertyValue)
    {
        var propertyExpression = new PropertyExpression(func, [propertyName, propertyValue]);
        configurationSource.AddCustomConfigurationRule(new PropertyExpressionFilterRule(propertyExpression));
    }

    public static void AddLogPathRule(this ICustomConfigurationSource configurationSource,  string logPath, string[]? pathSections = null)
    {
        configurationSource.AddCustomConfigurationRule(new LogPathRule(logPath, pathSections));
    }

    public static void AddContextPropertyNameRule(this ICustomConfigurationSource configurationSource, string propertyName)
    {
        configurationSource.AddCustomConfigurationRule(new ContextPropertyNameRule(propertyName));
    }
}