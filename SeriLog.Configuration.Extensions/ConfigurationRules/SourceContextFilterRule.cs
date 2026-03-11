namespace Serilog.Configuration.Extensions.ConfigurationRules;

internal class SourceContextFilterRule(string context)
    : PropertyExpressionFilterRule(new PropertyExpression(SerilogFunc.Contains, [SerilogExpressions.SourceContext, context]))
{
     
    public override bool Check(IDictionary<string, string?> data, string sectionName, string? value)
    {
        return base.Check(data, sectionName, value)
            && value?.Contains(SerilogExpressions.SourceContext.Name) == true;
    }
}