using Hangfire.Server;

namespace Serilog.HangfireConsoleContextSink;

internal class ConsoleContextFilter : IServerFilter
{
    public virtual void OnPerforming(PerformingContext filterContext) => HangfireConsoleContext.Current = filterContext;
    public virtual void OnPerformed(PerformedContext filterContext) => HangfireConsoleContext.Current = null;
}