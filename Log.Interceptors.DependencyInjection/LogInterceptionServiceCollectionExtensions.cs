using DependencyInjection.Interception;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Log.Interceptors.DependencyInjection;

public static class LogInterceptionServiceCollectionExtensions
{    
    private static IServiceCollection SetLogInterceptionFactoryIfNeed(this IServiceCollection services)
    {
        var interceptionService = services.FirstOrDefault(s => s.ServiceType == typeof(ILoggerFactory)
                                                              && s.ImplementationType == typeof(LogInterceptionFactoryImpl));
        return interceptionService == null ? services.Intercept<ILoggerFactory, LogInterceptionFactoryImpl>() : services;
    }

    public static IServiceCollection AddLogInterception(this IServiceCollection services, Type serviceType, Func<ILogger, ILogger> loggerInterception)
    {
        services.SetLogInterceptionFactoryIfNeed();
        return services.AddTransient<IServiceLoggerInterceptor>((provider) => new ServiceLogInterceptorImpl(serviceType, loggerInterception));
    }

    public static IServiceCollection AddLogInterception<TService>(this IServiceCollection services, Func<ILogger, ILogger> loggerInterception)
    {
        services.SetLogInterceptionFactoryIfNeed();
        return services.AddLogInterception(typeof(TService), loggerInterception);
    }

    public static IServiceCollection AddLogInterception(this IServiceCollection services, Type logInterceptionType)
    {
        services.SetLogInterceptionFactoryIfNeed();
        return services.AddTransient(typeof(IServiceLoggerInterceptor), logInterceptionType);
    }
    
    public static IServiceCollection AddLogInterception<TLogInterception>(this IServiceCollection services, Type logInterceptionType)
        where TLogInterception:class, IServiceLoggerInterceptor
    {
        services.SetLogInterceptionFactoryIfNeed();
        return services.AddTransient<IServiceLoggerInterceptor,TLogInterception>();
    }

    public static IServiceCollection AddLogInterception(this IServiceCollection services, IServiceLoggerInterceptor logInterceptor)
    {
        services.SetLogInterceptionFactoryIfNeed();
        return services.AddSingleton(logInterceptor);
    }

}
