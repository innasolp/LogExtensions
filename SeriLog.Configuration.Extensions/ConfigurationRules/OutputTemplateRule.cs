using CustomConfigurationProvider;

namespace Serilog.Configuration.Extensions.ConfigurationRules;

internal class OutputTemplateRule(IEnumerable<string> properties) : ICustomConfigurationRule
{
    private readonly IEnumerable<string> _properties = properties;
    public bool Check(string sectionName, string value)
    {
        return sectionName.Contains($"{WriteToSections.WriteToSectionName}:")
            && sectionName.Contains($"Args:{WriteToSections.ConfigureLoggerSectionName}:{WriteToSections.WriteToSectionName}")
            && sectionName.Contains($"Args:{WriteToSections.OutputTemplateSectionName}")
            && value?.Contains(ContextVariables.PropertyNameContextStr) == true;
    }

    public string TransformValue(string value)
    {
        var propertyString = string.Join(" ", _properties.Select(p => $"{p}"));
        return value.Replace(ContextVariables.PropertyNameContextStr, propertyString);
    }
}