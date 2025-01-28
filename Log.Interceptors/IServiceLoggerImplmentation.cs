using Microsoft.Extensions.Logging;

namespace Log.Interceptors;

public interface IServiceLoggerImplmentation
{
    Type ServiceType { get; }

    ILogger CreateLogger(ILoggerFactory loggerFactory);
}
