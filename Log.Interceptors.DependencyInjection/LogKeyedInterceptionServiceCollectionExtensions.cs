using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Log.Interceptors.DependencyInjection;

public static class LogKeyedInterceptionServiceCollectionExtensions
{
    public static IServiceCollection AddKeyedLogInterception(this IServiceCollection services, Type serviceType, Func<ILogger, object?, ILogger> keyedLoggerInterception)
    {
        return services.AddKeyedTransient<IKeyedServiceLoggerInterceptor>(serviceType, (provider, key) => new KeyedServiceLoggerInterceptor(keyedLoggerInterception));
    }

    public static IServiceCollection AddKeyedLogInterception(this IServiceCollection services, Func<ILogger, object?, ILogger> keyedLoggerInterception, object? key)
    {
        return services.AddKeyedTransient<IKeyedServiceLoggerInterceptor>(key, (provider, serviceKey) => new KeyedServiceLoggerInterceptor(keyedLoggerInterception));
    }

    public static IServiceCollection AddKeyedLogInterception(this IServiceCollection services, Type keyedLogInterceptionType, object? key)
    {
        return services.AddKeyedTransient(typeof(IKeyedServiceLoggerInterceptor), key, keyedLogInterceptionType);
    }

    public static IServiceCollection AddKeyedLogInterception<T>(this IServiceCollection services, object? key)
        where T : class, IKeyedServiceLoggerInterceptor
    {
        return services.AddKeyedTransient<IKeyedServiceLoggerInterceptor, T>(key);
    }
    public static IServiceCollection AddKeyedLogInterception(this IServiceCollection services, IKeyedServiceLoggerInterceptor keyedLogInterceptor, object? key)
    {
        return services.AddKeyedSingleton(typeof(IKeyedServiceLoggerInterceptor), key, keyedLogInterceptor);
    }
}
