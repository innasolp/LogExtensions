using DependencyInjection.Interception;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Log.Interceptors.DependencyInjection;

public static class LogImplementationServiceCollectionExtensions
{
    public static IServiceCollection InterceptLoggerFactory<T>(this IServiceCollection services)
        where T: class, ILoggerFactory
    {
        return services.Intercept<ILoggerFactory, T>();
    }
    
    private static IServiceCollection SetLogImplementationFactoryIfNeed(this IServiceCollection services)
    {
        var implementationService = services.FirstOrDefault(s => s.ServiceType == typeof(ILoggerFactory) 
                                                              && (s.ImplementationType == typeof(LoggerImplementationFactory) ||
                                                               s.ImplementationFactory?.Target?.GetType().DeclaringType == typeof(InterceptorExtensions)));
        return implementationService == null ? services.Intercept<ILoggerFactory, LoggerImplementationFactory>() : services;
    }

    public static IServiceCollection AddLogImplementation(this IServiceCollection services, Type serviceType, Func<ILoggerFactory, ILogger> loggerImplementation)
    {
        services.SetLogImplementationFactoryIfNeed();
        return services.AddTransient<IServiceLoggerImplmentation>((provider) => new ServiceLoggerImplmentation(serviceType, loggerImplementation));
    }

    public static IServiceCollection AddLogImplementation<TService>(this IServiceCollection services, Func<ILoggerFactory, ILogger> loggerImplementation)
    {
        services.SetLogImplementationFactoryIfNeed();
        return services.AddLogImplementation(typeof(TService), loggerImplementation);
    }

    

    
}
