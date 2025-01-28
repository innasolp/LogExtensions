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
        lv.Key.Equals("uri", StringComparison.CurrentCultureIgnoreCase) || lv.Key.Equals("url", StringComparison.CurrentCultureIgnoreCase));

        if (string.IsNullOrEmpty(url.Key))
            url = logValues.FirstOrDefault(lv => lv.Value != null && Uri.TryCreate(lv.Value.ToString(), UriKind.Absolute, out Uri? uri));

        if (string.IsNullOrEmpty(url.Key))
            base.Log(logLevel, eventId, state, exception, formatter);

        var uri = new Uri(url.Value.ToString());
        var baseUrl = $"{uri.Scheme}://{uri.Host}" + (!uri.IsDefaultPort ? $":{uri.Port}" : "");

        using (LogContext.PushProperty("Url", baseUrl))
        {
            base.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}

public class SerilogUrlLogger<T>(ILogger<T> logger) : SerilogUrlLogger(logger), ILogger<T>
{
}
