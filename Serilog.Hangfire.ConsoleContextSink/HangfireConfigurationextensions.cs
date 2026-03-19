using Hangfire;

namespace Serilog.HangfireConsoleContextSink;

public static class HangfireConfigurationExtensions
{
    public static void AddConsoleContextFilter(this IGlobalConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        config.UseFilter(new ConsoleContextFilter());   
    }

    public static void AddAggregateConsoleContextFilter(this IGlobalConfiguration config,
        string idPropertyName = AggregateConsoleContextStoreFilter.DefaultIdPropertyName,
        string isAggregatePropertyName = AggregateConsoleContextStoreFilter.DefaultIsAggregatePropertyName,
        string parentJobIdPropertyName = AggregateConsoleContextStoreFilter.DefaultParentIdPropertyName)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        config.UseFilter(new AggregateConsoleContextStoreFilter(idPropertyName, isAggregatePropertyName, parentJobIdPropertyName));   
    }
}