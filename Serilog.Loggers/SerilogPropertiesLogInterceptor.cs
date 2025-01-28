using Log.Interceptors;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Interceptors;

public class SerilogPropertiesLogInterceptor(Microsoft.Extensions.Logging.ILogger logger, Dictionary<string,object> properties) 
    : LogInterceptor(logger)
{
    private readonly Dictionary<string, object> _properties = properties;
    public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        using (LogContext.PushProperty(_propertyName, _propertyValue))
        {
            base.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    private void LogWithPushProperty<TState>(string propertyName, object value, 
        LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        using (LogContext.PushProperty(propertyName, value))
        {
            base.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}
