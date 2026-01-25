using Microsoft.Extensions.Logging;

namespace Log.Interceptors;

public abstract class LogInterceptionFactory(ILoggerFactory coreLoggerFactory) : ILoggerFactory
{
    protected ILoggerFactory CoreLoggerFactory { get; } = coreLoggerFactory;
    
    public void AddProvider(ILoggerProvider provider)
    {
        CoreLoggerFactory.AddProvider(provider);
    }

    public abstract ILogger CreateLogger(string categoryName);

    public virtual void Dispose()
    {
        CoreLoggerFactory.Dispose();
    }
}
