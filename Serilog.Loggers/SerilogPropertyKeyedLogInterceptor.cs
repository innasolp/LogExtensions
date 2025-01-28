using Log.Interceptors;
using Microsoft.Extensions.Logging;

namespace Serilog.Loggers;

public class SerilogPropertyKeyedLogInterceptor(string propertyName, object propertyValue) : IKeyedServiceLoggerInterceptor
{
    private readonly object _propertyValue = propertyValue;
    private readonly string _propertyName = propertyName;

    public Microsoft.Extensions.Logging.ILogger Intercept(Microsoft.Extensions.Logging.ILogger logger, object? key)
    {
        return new SerilogPropertyLogger(logger, _propertyName, _propertyValue);
    }

    public ILogger<T> Intercept<T>(ILogger<T> logger, object? key)
    {
        return new SerilogPropertyLogger<T>(logger, _propertyName, _propertyValue);
    }
}
