using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Log.Interceptors.DependencyInjection;

public static class LogServiceProviderExtensions
{
    public static ILogger<T> GetLogger<T>(this IServiceProvider serviceProvider, object? key)
    {
        var logger = serviceProvider.GetKeyedService<ILogger<T>>(key);
        if (logger != null) return logger;

        var keyedLoggerFactory = serviceProvider.GetKeyedService<ILoggerFactory>(key);
        if(keyedLoggerFactory  != null)        
            return keyedLoggerFactory.CreateLogger<T>();

        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        logger = loggerFactory.CreateLogger<T>();
        var keyedLogInterceptor = serviceProvider.GetKeyedService<IKeyedServiceLoggerInterceptor>(typeof(T)) ?? 
            serviceProvider.GetKeyedService<IKeyedServiceLoggerInterceptor>(key);

        if (keyedLogInterceptor == null) return logger;

        return keyedLogInterceptor.Intercept(logger, key);
    }
}
