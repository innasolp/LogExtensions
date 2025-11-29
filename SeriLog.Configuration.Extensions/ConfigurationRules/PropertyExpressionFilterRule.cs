using CustomConfigurationProvider;

namespace Serilog.Configuration.Extensions.ConfigurationRules;

internal class PropertyExpressionFilterRule(PropertyExpression serilogPropertyExpression) : ICustomConfigurationRule
{
    private readonly PropertyExpression _serilogPropertyExpression = serilogPropertyExpression;

    internal const string And = "and";

    protected virtual bool Check(string sectionName, string value)
    {
        return sectionName.Contains($"{WriteToSections.WriteToSectionName}:")
            && sectionName.Contains($"Args:{WriteToSections.ConfigureLoggerSectionName}:{WriteToSections.FilterSectionName}")
            && sectionName.Contains($"Args:{WriteToSections.ExpressionSectionName}");
    }

    public string TransformValue(string value)
    {
        var expressionStr = _serilogPropertyExpression.GetExpression();
        if (string.IsNullOrEmpty(expressionStr))
            return value;
        if (!string.IsNullOrEmpty(value))
            value += $" {And} ";
        return $"{value ?? ""}{expressionStr}";
    }

    bool ICustomConfigurationRule.Check(string sectionName, string value)
    {
        return Check(sectionName, value);
    }
}