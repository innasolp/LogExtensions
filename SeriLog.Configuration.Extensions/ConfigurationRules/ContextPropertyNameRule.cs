using CustomConfigurationProvider;

namespace Serilog.Configuration.Extensions.ConfigurationRules;

internal class ContextPropertyNameRule(string contextPropertyName) : ICustomConfigurationRule
{
    private readonly string _contextPropertyName = contextPropertyName;

    public bool Check(string sectionName, string value)
    {
        return sectionName.Contains($"{WriteToSections.WriteToSectionName}:") &&
            value.Contains(ContextVariables.PropertyNameContextStr);
    }

    public string TransformValue(string value)
    {
        return value.Replace(ContextVariables.PropertyNameContextStr, _contextPropertyName);
    }
}