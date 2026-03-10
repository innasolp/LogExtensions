using Hangfire.Server;

namespace Serilog.HangfireConsoleContextSink;

internal class ConsoleContextFilter : IServerFilter
{
    public void OnPerforming(PerformingContext filterContext) => HangfireConsoleContext.Current = filterContext;
    public void OnPerformed(PerformedContext filterContext) => HangfireConsoleContext.Current = null;
}