using Hangfire;

namespace Serilog.HangfireConsoleContextSink;

public static class HangfireConfigurationextensions
{
    public static void AddConsoleContextFilter(this IGlobalConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        config.UseFilter(new ConsoleContextFilter());   
    }
}