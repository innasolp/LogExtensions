namespace Serilog.Configuration.Extensions.ConfigurationRules;

internal class EventIdFilterRule(int eventId) 
    : PropertyExpressionFilterRule(new PropertyExpression("=", [SerilogExpressions.EventId, eventId]))
{
}
