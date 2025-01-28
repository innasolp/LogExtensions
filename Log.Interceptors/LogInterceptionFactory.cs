using Microsoft.Extensions.Logging;

namespace Log.Interceptors;

public abstract class LogInterceptionFactory(ILoggerFactory loggerFactory) : ILoggerFactory
{
    protected ILoggerFactory LoggerFactory { get; } = loggerFactory;
    
    public void AddProvider(ILoggerProvider provider)
    {
        LoggerFactory.AddProvider(provider);
    }

    public abstract ILogger CreateLogger(string categoryName);

    public virtual void Dispose()
    {
        LoggerFactory.Dispose();
    }
}
