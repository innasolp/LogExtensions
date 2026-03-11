using CustomConfigurationProvider.Rules;

namespace Serilog.Configuration.Extensions.ConfigurationRules;

internal class LogPathRule(string logPath) : CustomOrdinaryRule
{
    private readonly string _logPath = logPath;

    public override bool Check(IDictionary<string, string?> data, string sectionName, string? value)
    {
        return sectionName.Contains($"{WriteToSections.WriteToSectionName}:")
                    && sectionName.Contains($"Args:{WriteToSections.ConfigureLoggerSectionName}:{WriteToSections.WriteToSectionName}")
             && value?.Contains(ContextVariables.LoggerPathStr) == true;
    }

    protected override string? GetValue(string? value)
    {
        return value?.Replace(ContextVariables.LoggerPathStr, _logPath);
    }
}