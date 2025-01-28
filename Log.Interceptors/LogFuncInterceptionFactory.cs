using Microsoft.Extensions.Logging;

namespace Log.Interceptors;

public class LogFuncInterceptionFactory<T>(ILoggerFactory factory, Func<ILogger, T>? getLogger, Func<ILogger, object?, T?>? getKeyedLogger)
    : LogInterceptionFactory(factory)
    where T : LogInterceptor
{
    public LogFuncInterceptionFactory(ILoggerFactory loggerFactory, Func<ILogger, T> getLogger)
        :this(loggerFactory, getLogger, null) { }
    
    public LogFuncInterceptionFactory(ILoggerFactory loggerFactory, Func<ILogger, object?, T?>? getKeyedLogger)
        :this(loggerFactory, null, getKeyedLogger) { }

    private readonly Func<ILogger, T?>? _getLogger = getLogger;

    public override ILogger CreateLogger(string categoryName)
    {
        var logger = LoggerFactory.CreateLogger(categoryName);
        return _getLogger != null ? (_getLogger(logger) ?? logger) : logger;
    }
}
