using Microsoft.Extensions.Logging;

namespace Log.Interceptors.DependencyInjection;

internal class KeyedServiceLoggerInterceptor(Func<ILogger, object?, ILogger> loggerIntercept) : IKeyedServiceLoggerInterceptor
{
    private readonly Func<ILogger, object?, ILogger> _loggerIntercept = loggerIntercept;

    public ILogger Intercept(ILogger logger, object? key)
    {
        return _loggerIntercept(logger, key);
    }

    public ILogger<T> Intercept<T>(ILogger<T> logger, object? key)
    {
        return _loggerIntercept(logger, key) as ILogger<T>;
    }
}
