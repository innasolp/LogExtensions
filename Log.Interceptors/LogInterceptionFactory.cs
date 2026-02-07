using Microsoft.Extensions.Logging;

namespace Log.Interceptors;

public abstract class LogInterceptionFactory(ILoggerFactory coreLoggerFactory) : ILoggerFactory
{
    private bool disposedValue;

    protected ILoggerFactory CoreLoggerFactory { get; } = coreLoggerFactory;
    
    public void AddProvider(ILoggerProvider provider)
    {
        CoreLoggerFactory.AddProvider(provider);
    }

    public abstract ILogger CreateLogger(string categoryName);

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                CoreLoggerFactory.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
    }
}