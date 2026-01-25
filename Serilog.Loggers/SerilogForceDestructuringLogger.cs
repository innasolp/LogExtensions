using Log.Interceptors;
using Microsoft.Extensions.Logging;

namespace Serilog.Loggers;

public class SerilogForceDestructuringLogger(Microsoft.Extensions.Logging.ILogger coreLogger) : LogInterceptor(coreLogger)
{
    private const string OriginalFormatKey = "{OriginalFormat}";

    public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if(state is IEnumerable<KeyValuePair<string, object?>> values)
        {
            var originalFormat = values.FirstOrDefault(v => v.Key == OriginalFormatKey && v.Value != null).Value?.ToString();
            if(originalFormat is null)
                base.Log(logLevel, eventId, state, exception, formatter);

            var destructuringParameters = values.Where(v => v.Value is not string && v.Value?.GetType().IsPrimitive == false);

            var destructuringFormat = originalFormat;
            foreach(var destructuringParameter in destructuringParameters)
            {
                destructuringFormat = destructuringFormat.Replace($"{{{destructuringParameter.Key}}}",
                    $"{{@{destructuringParameter.Key}}}");
            }

            var customLogValueFormatter = new CustomLogValuesFormatter(destructuringFormat);

            base.Log(logLevel, eventId, values, exception, (state, ex) =>
            {
                return customLogValueFormatter.Format([.. values.Where(v=>v.Key != OriginalFormatKey).Select(f=>f.Value)]);
            });
        }
        else 
            base.Log(logLevel, eventId, state, exception, formatter);
    }
}