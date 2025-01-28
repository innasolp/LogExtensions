using Microsoft.Extensions.Logging;

namespace Log.Interceptors.DependencyInjection;

internal class ServiceLogInterceptorImpl(Type serviceType, Func<ILogger, ILogger> loggerIntercept) : IServiceLoggerInterceptor
{
    public Type ServiceType => serviceType;

    private readonly Func<ILogger, ILogger> _loggerIntercept = loggerIntercept;

    public ILogger Intercept(ILogger logger)
    {
        return _loggerIntercept(logger);
    }
}
