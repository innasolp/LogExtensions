using CustomConfigurationProvider.Rules;

namespace Serilog.Configuration.Extensions.ConfigurationRules;

internal class PropertyExpressionFilterRule(PropertyExpression serilogPropertyExpression) : CustomOrdinaryRule
{
    private readonly PropertyExpression _serilogPropertyExpression = serilogPropertyExpression;

    internal const string And = "and";

    protected override string? GetValue(string? value)
    {
        var expressionStr = _serilogPropertyExpression.GetExpression();
        if (string.IsNullOrEmpty(expressionStr))
            return value;
        if (!string.IsNullOrEmpty(value))
            value += $" {And} ";
        return $"{value ?? ""}{expressionStr}";
    }

    public override bool Check(IDictionary<string, string?> data, string sectionName, string? value)
    {
        return sectionName.Contains($"{WriteToSections.WriteToSectionName}:")
            && sectionName.Contains($"Args:{WriteToSections.ConfigureLoggerSectionName}:{WriteToSections.FilterSectionName}")
            && sectionName.Contains($"Args:{WriteToSections.ExpressionSectionName}");
    }
}