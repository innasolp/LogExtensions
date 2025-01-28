using Microsoft.Extensions.Logging;

namespace Log.Interceptors.DependencyInjection;

internal class ServiceLoggerImplmentation(Type serviceType,Func<ILoggerFactory, ILogger> createLogger ) : IServiceLoggerImplmentation
{
    public Type ServiceType => serviceType;

    private readonly Func<ILoggerFactory, ILogger> _createLogger = createLogger;

    public ILogger CreateLogger(ILoggerFactory loggerFactory)
    {
        return _createLogger(loggerFactory);
    }
}

internal class ServiceLoggerImplmentation<TService>(Func<ILoggerFactory, ILogger> createLogger)
    : ServiceLoggerImplmentation(typeof(TService),createLogger)
{
}
