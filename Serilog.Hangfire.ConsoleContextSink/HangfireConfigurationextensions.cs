using Hangfire;

namespace Serilog.HangfireConsoleContextSink;

public static class HangfireConfigurationextensions
{
    public static void AddConsoleContextFilter(this IGlobalConfiguration config)
    {
        config.UseFilter(new ConsoleContextFilter());   
    }
}