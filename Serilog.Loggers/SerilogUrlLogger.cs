using Log.Interceptors;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Serilog.Loggers;

public class SerilogUrlLogger(Microsoft.Extensions.Logging.ILogger logger) : LogInterceptor(logger)
{
    public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (state is not IReadOnlyList<KeyValuePair<string, object?>> logValues)
        {
            base.Log(logLevel, eventId, state, exception, formatter);
            return;
        }

        var url = logValues.FirstOrDefault(lv => lv.Value is Uri ||
        lv.Key.Equals("uri", StringComparison.CurrentCultureIgnoreCase)
        || lv.Key.Equals("url", StringComparison.CurrentCultureIgnoreCase)
        || (lv.Value != null && Uri.TryCreate(lv.Value.ToString(), UriKind.Absolute, out _)));

        if (string.IsNullOrEmpty(url.Key) || url.Value is null)
        {
            base.Log(logLevel, eventId, state, exception, formatter);
            return;
        }

        if( !Uri.TryCreate(url.Value.ToString(), UriKind.Absolute, out Uri? uri) || uri is null || !uri.IsAbsoluteUri)
        {
            base.Log(logLevel, eventId, state, exception, formatter);
            return;
        }

        var baseUrl = $"{uri.Scheme}://{uri.Host}" + (!uri.IsDefaultPort ? $":{uri.Port}" : "");

        using (LogContext.PushProperty("Url", baseUrl))
        using (LogContext.PushProperty("Host", uri.Host))
        {
            base.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}

public class SerilogUrlLogger<T>(Microsoft.Extensions.Logging.ILogger logger) : SerilogUrlLogger(logger), ILogger<T>
{
}
