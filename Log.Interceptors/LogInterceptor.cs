using Microsoft.Extensions.Logging;

namespace Log.Interceptors;

public abstract class LogInterceptor(ILogger coreLogger) : ILogger
{
    protected ILogger CoreLogger { get; } = coreLogger;

    public virtual IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return CoreLogger.BeginScope(state);
    }

    public virtual bool IsEnabled(LogLevel logLevel)
    {
        return CoreLogger.IsEnabled(logLevel);
    }

    public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        CoreLogger.Log(logLevel, eventId, state, exception, formatter);
    }
}

public abstract class LogInterceptor<T>(ILogger<T> logger) : LogInterceptor(logger), ILogger<T>
{ }
