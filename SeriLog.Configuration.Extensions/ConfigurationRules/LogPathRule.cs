using CustomConfigurationProvider;

namespace Serilog.Configuration.Extensions.ConfigurationRules;

internal class LogPathRule(string logPath, string[]? pathSections = null) : ICustomConfigurationRule
{
    private readonly string _logPath = logPath;

    private readonly string[]? _pathSections = pathSections;

    public bool Check(string sectionName, string value)
    {
        return (_pathSections == null
               || 
                 (sectionName.Contains($"{WriteToSections.WriteToSectionName}:")
                    && sectionName.Contains($"Args:{WriteToSections.ConfigureLoggerSectionName}:{WriteToSections.WriteToSectionName}")))
             &&
            value.Contains(ContextVariables.LoggerPathStr);
    }

    public string TransformValue(string value)
    {
        return value.Replace(ContextVariables.LoggerPathStr, _logPath);
    }
}