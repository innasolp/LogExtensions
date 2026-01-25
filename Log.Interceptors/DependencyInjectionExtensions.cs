using DependencyInjection.InterceptionExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Log.Interceptors;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection InterceptLoggerFactory(this IServiceCollection services, Func<ILogger, ILogger>? getLogger)
    {
        return services.Intercept(typeof(ILoggerFactory), (f) =>
        {
            if(f is ILoggerFactory loggerFactory)
              return new LogFuncInterceptionFactory<ILogger>(loggerFactory, getLogger);
            else
                throw new ArgumentException($"Expected {nameof(ILoggerFactory)} but received {f.GetType().Name}");
        });
    }
}