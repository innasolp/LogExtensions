using Hangfire.Server;

namespace Serilog.HangfireConsoleContextSink;

internal class HangfireConsoleContext
{
    private static readonly AsyncLocal<PerformContext?> _currentContext = new();

    public static PerformContext? Current
    {
        get => _currentContext.Value;
        set
        {
            if(_currentContext != null)
                _currentContext.Value = value;
        }
    }
}