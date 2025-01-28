using Microsoft.Extensions.Logging;

namespace Log.Interceptors;

public abstract class LogInterceptor(ILogger logger) : ILogger
{
    protected ILogger Logger { get; } = logger;

    public virtual IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return Logger.BeginScope(state);
    }

    public virtual bool IsEnabled(LogLevel logLevel)
    {
        return Logger.IsEnabled(logLevel);
    }

    public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Logger.Log(logLevel, eventId, state, exception, formatter);
    }
}

public abstract class LogInterceptor<T>(ILogger<T> logger) : LogInterceptor(logger), ILogger<T>
{ }
