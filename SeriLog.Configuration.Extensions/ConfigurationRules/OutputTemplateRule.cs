using CustomConfigurationProvider.Rules;

namespace Serilog.Configuration.Extensions.ConfigurationRules;

internal class OutputTemplateRule(IEnumerable<string> properties) : CustomOrdinaryRule
{
    private readonly IEnumerable<string> _properties = properties;
    
    public override bool Check(IDictionary<string, string?> data, string sectionName, string? value)
    {
        return sectionName.Contains($"{WriteToSections.WriteToSectionName}:")
            && sectionName.Contains($"Args:{WriteToSections.ConfigureLoggerSectionName}:{WriteToSections.WriteToSectionName}")
            && sectionName.Contains($"Args:{WriteToSections.OutputTemplateSectionName}")
            && value?.Contains(ContextVariables.PropertyNameContextStr) == true;
    }

    protected override string? GetValue(string? value)
    {
        var propertyString = string.Join(" ", _properties.Select(p => $"{p}"));
        return value?.Replace(ContextVariables.PropertyNameContextStr, propertyString);
    }
}