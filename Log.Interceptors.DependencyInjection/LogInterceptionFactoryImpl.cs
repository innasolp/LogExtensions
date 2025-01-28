using Microsoft.Extensions.Logging;

namespace Log.Interceptors.DependencyInjection;

internal class LogInterceptionFactoryImpl(ILoggerFactory loggerFactory,
    IEnumerable<IServiceLoggerInterceptor>? logInterceptorImplementations)
    : LogInterceptionFactory(loggerFactory)
{
    private readonly IEnumerable<IServiceLoggerInterceptor>? _logInterceptorImplementations = logInterceptorImplementations;    

    public override ILogger CreateLogger(string categoryName)
    {
        var implementation = _logInterceptorImplementations?.FirstOrDefault(x => x.ServiceType.Name?.Contains(categoryName) == true)
            ?? _logInterceptorImplementations?.FirstOrDefault(x => x.ServiceType.FullName?.Contains(categoryName) == true);

        var logger = LoggerFactory.CreateLogger(categoryName);

        return implementation?.Intercept(logger) ?? logger;
    }
}
