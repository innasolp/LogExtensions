using Microsoft.Extensions.Logging;

namespace Log.Interceptors;

public class LogFuncInterceptionFactory<T>(ILoggerFactory factory, Func<ILogger, T>? getLogger) : LogInterceptionFactory(factory)
    where T : ILogger
{
    private readonly Func<ILogger, T?>? _getLogger = getLogger;

    public override ILogger CreateLogger(string categoryName)
    {
        var logger = CoreLoggerFactory.CreateLogger(categoryName);
        return _getLogger != null ? (_getLogger(logger) ?? logger) : logger;
    }
}