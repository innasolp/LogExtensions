using Microsoft.Extensions.Logging;

namespace Log.Interceptors;

public interface IServiceLoggerInterceptor
{
    Type ServiceType { get; }

    ILogger Intercept(ILogger logger);
}
