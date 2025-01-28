using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Log.DependencyInjection.Extensions;

public static class LogExtensions
{
    public static ILogger<T> GetLogger<T>(this IServiceProvider serviceProvider, object? key)
    {
        var logger = serviceProvider.GetKeyedService<ILogger<T>>(key);
        if (logger != null) return logger;

        var loggerFactory = serviceProvider.GetKeyedService<ILoggerFactory>(key);

        return loggerFactory == null ? serviceProvider.GetRequiredService<ILogger<T>>()
             : loggerFactory.CreateLogger<T>();
    }
}
