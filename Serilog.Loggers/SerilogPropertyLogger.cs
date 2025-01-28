using Log.Interceptors;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Serilog.Loggers;

public class SerilogPropertyLogger(Microsoft.Extensions.Logging.ILogger logger, string propertyName, object propertyValue) : LogInterceptor(logger)
{
    private readonly object _propertyValue = propertyValue;
    private readonly string _propertyName = propertyName;

    //public override ILogger<T> Intercept<T>(Microsoft.Extensions.Logging.ILogger logger)
    //{
    //    return new SerilogPropertyLogInterceptor<T>(logger as ILogger<T>, _propertyName, _propertyValue);
    //}

    public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        using (LogContext.PushProperty(_propertyName, _propertyValue))
        {
            base.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}

public class SerilogPropertyLogger<T>(ILogger<T> logger, string propertyName, object propertyValue)
    : SerilogPropertyLogger(logger, propertyName, propertyValue), ILogger<T>
{
}
