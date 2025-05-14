using Log.Interceptors;
using Microsoft.Extensions.Logging;

namespace Serilog.Loggers;

public class SerilogPropertyLogInterceptor(Dictionary<string, object> properties) : IKeyedServiceLoggerInterceptor
{    
    private readonly Dictionary<string, object> _properties = properties;

    public SerilogPropertyLogInterceptor(string propertyName, object propertyValue) 
        : this(new Dictionary<string, object> { { propertyName, propertyValue } })
    {
    }

    public Microsoft.Extensions.Logging.ILogger Intercept(Microsoft.Extensions.Logging.ILogger logger, object? key)
    {
        return new SerilogPropertyLogger(logger, _properties);
    }

    public ILogger<T> Intercept<T>(ILogger<T> logger, object? key)
    {
        return new SerilogPropertyLogger<T>(logger, _properties);
    }
}
