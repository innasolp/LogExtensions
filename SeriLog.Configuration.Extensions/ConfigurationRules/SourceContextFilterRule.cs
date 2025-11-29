namespace Serilog.Configuration.Extensions.ConfigurationRules;

internal class SourceContextFilterRule(string context)
    : PropertyExpressionFilterRule(new PropertyExpression(SerilogFunc.Contains, [SerilogExpressions.SourceContext, context]))
{
    protected override bool Check(string sectionName, string value)
    {
        return base.Check(sectionName, value)
            && value?.Contains(SerilogExpressions.SourceContext.Name) == true;
    }
}