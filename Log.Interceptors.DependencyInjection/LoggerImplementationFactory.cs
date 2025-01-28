using Microsoft.Extensions.Logging;

namespace Log.Interceptors.DependencyInjection;

internal class LoggerImplementationFactory(ILoggerFactory loggerFactory,
    IEnumerable<IServiceLoggerImplmentation>? loggerImplementations)
    : LogInterceptionFactory(loggerFactory)
{
    private readonly IEnumerable<IServiceLoggerImplmentation>? _loggerImplementations = loggerImplementations;    

    public override ILogger CreateLogger(string categoryName)
    {
        var implementation = _loggerImplementations?.FirstOrDefault(x => x.ServiceType.Name?.Contains(categoryName) == true)
            ?? _loggerImplementations?.FirstOrDefault(x => x.ServiceType.FullName?.Contains(categoryName) == true);

        return implementation?.CreateLogger(LoggerFactory) ?? LoggerFactory.CreateLogger(categoryName);
    }
}
