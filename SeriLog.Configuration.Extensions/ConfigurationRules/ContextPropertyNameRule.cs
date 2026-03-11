using CustomConfigurationProvider.Rules;

namespace Serilog.Configuration.Extensions.ConfigurationRules;

internal class ContextPropertyNameRule(string contextPropertyName) : CustomOrdinaryRule
{
    private readonly string _contextPropertyName = contextPropertyName;

    public override bool Check(IDictionary<string, string?> data, string sectionName, string? value)
    {
        return sectionName.Contains($"{WriteToSections.WriteToSectionName}:") &&
           value?.Contains(ContextVariables.PropertyNameContextStr) == true;
    }

    public string TransformValue(string value)
    {
        return value.Replace(ContextVariables.PropertyNameContextStr, _contextPropertyName);
    }

    protected override string? GetValue(string? value)
    {
        return value?.Replace(ContextVariables.PropertyNameContextStr, _contextPropertyName);
    }
}