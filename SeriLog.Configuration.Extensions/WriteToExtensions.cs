using Microsoft.Extensions.Configuration;

namespace Serilog.Configuration.Extensions;

internal static class WriteToExtensions
{
    internal const string WriteToSectionName = "WriteTo";

    internal const string ConfigureLoggerSectionName = "configureLogger";
    internal const string FilterSectionName = "Filter";
    internal const string ExpressionSectionName = "expression";

    internal const string OutputTemplateSectionName = "outputTemplate";
    internal const string PropertyNameSectionName = "propertyName";

    internal static List<IConfigurationSection> GetWriteToSections(this IConfiguration configuration)
    {
        var serilogSection = configuration.GetSection("Serilog");
        if (serilogSection == null) return [];

        return [.. serilogSection.GetSection(WriteToSectionName).GetChildren()];
    }

    internal static List<IConfigurationSection> GetWriteToExpressionSections(this IConfiguration configuration)
    {
        var writeToSections = configuration.GetWriteToSections();
        return [.. writeToSections.Select(writeTo => writeTo.GetSection($"Args:{ConfigureLoggerSectionName}:{FilterSectionName}"))
            .SelectMany(f => f.GetChildren())
            .Select(c => c.GetSection($"Args:{ExpressionSectionName}"))];
    }

    internal static List<IConfigurationSection> GetWriteToExpressionSections(this IConfigurationSection writeToSection)
    {
        return [.. writeToSection.GetSection($"Args:{ConfigureLoggerSectionName}:{FilterSectionName}")
            .GetChildren()
            .Select(c => c.GetSection($"Args:{ExpressionSectionName}"))];
    }

    internal static List<IConfigurationSection> GetWriteToOutputTemplateSections(this IConfiguration configuration)
    {
        var writeToSections = configuration.GetWriteToSections();
        return [.. writeToSections.SelectMany(writeTo => writeTo.GetSection($"Args:{ConfigureLoggerSectionName}:{WriteToSectionName}").GetChildren())
            .Select(c => c.GetSection($"Args:{OutputTemplateSectionName}"))];
    }

    internal static List<IConfigurationSection> GetWriteToOutputTemplateSections(this IConfigurationSection writeToSection)
    {
        return [.. writeToSection.GetSection($"Args:{ConfigureLoggerSectionName}:{WriteToSectionName}").
            GetChildren().
            Select(c => c.GetSection($"Args:{OutputTemplateSectionName}"))];
    }

    internal static List<IConfigurationSection> GetWriteToSectionsByName(this IConfigurationSection writeToSection, string sectionName)
    {
        return [.. writeToSection.GetSection($"Args:{ConfigureLoggerSectionName}:{WriteToSectionName}").
            GetChildren().
            Select(c => c.GetSection($"Args:{sectionName}"))];
    }

    internal static List<IConfigurationSection> GetWriteToPropertyNameSections(this IConfigurationSection writeToSection)
    {
        return writeToSection.GetWriteToSectionsByName("propertyName");
    }

    internal static List<IConfigurationSection> GetWriteToPathSections(this IConfiguration configuration, string[] pathSectionNames)
    {
        var writeToSections = configuration.GetWriteToSections();
        if (writeToSections.Count == 0) return [];

        var pathes = new List<IConfigurationSection>();
        foreach (var writeTo in writeToSections)
        {
            pathes.AddRange(pathSectionNames.SelectMany(s => writeTo.GetSection($"Args:{ConfigureLoggerSectionName}:{WriteToSectionName}")
            .GetChildren()
            .Select(c => c.GetSection($"Args:{s}"))));
        }

        return pathes;
    }

    internal static List<IConfigurationSection> GetWriteToPathSections(this IConfigurationSection writeTo, string[] pathSectionNames)
    {
        return [.. pathSectionNames.SelectMany(s => writeTo.GetSection($"Args:{ConfigureLoggerSectionName}:{WriteToSectionName}")
        .GetChildren()
        .Select(c => c.GetSection($"Args:{s}")))];
    }
}
