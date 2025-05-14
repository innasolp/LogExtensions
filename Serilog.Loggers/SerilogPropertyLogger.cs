using Log.Interceptors;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Collections.Immutable;

namespace Serilog.Loggers;

public class SerilogPropertyLogger(Microsoft.Extensions.Logging.ILogger logger, Dictionary<string, object> properties) : LogInterceptor(logger)
{
    private readonly Dictionary<string, object> _properties = properties;

    public SerilogPropertyLogger(Microsoft.Extensions.Logging.ILogger logger, string propertyName, object propertyValue)
        : this(logger, new Dictionary<string, object> { { propertyName, propertyValue } })
    {
    }

    public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var disposables = _properties.Select(p => LogContext.PushProperty(p.Key, p.Value)).ToImmutableArray();

        base.Log(logLevel, eventId, state, exception, formatter);

        foreach (var disposable in disposables)
            disposable.Dispose();
    }
}

public class SerilogPropertyLogger<T> : SerilogPropertyLogger, ILogger<T>
{
    public SerilogPropertyLogger(Microsoft.Extensions.Logging.ILogger logger, Dictionary<string, object> properties) : base(logger, properties)
    {
    }

    public SerilogPropertyLogger(Microsoft.Extensions.Logging.ILogger logger, string propertyName, object propertyValue) : base(logger, propertyName, propertyValue)
    {
    }
}
