using Microsoft.Extensions.Logging;

namespace Log.Interceptors;

public  interface IKeyedServiceLoggerInterceptor
{
    ILogger Intercept(ILogger logger, object? key);

    ILogger<T> Intercept<T>(ILogger<T> logger, object? key);
}
