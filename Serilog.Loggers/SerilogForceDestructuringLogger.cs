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
            if (originalFormat is null)
            {
                base.Log(logLevel, eventId, state, exception, formatter);
                return;
            }

            var paramsNeedDestructuring = values.Where(v => v.Value is not null && v.Value.ToString()?.Contains('@') == false
                     && IsTypeDestructuring(v.Value.GetType()));
            if (!paramsNeedDestructuring.Any())
            {
                base.Log(logLevel, eventId, state, exception, formatter);
                return;
            }

            var formattingValues = new List<KeyValuePair<string, object?>>(values);

            var destructuringFormat = originalFormat;
            foreach(var destructuringParameter in paramsNeedDestructuring)
            {
                var initialKey = $"{{{destructuringParameter.Key}}}";
                var destructuringKey = $"{{@{destructuringParameter.Key}}}";
                destructuringFormat = destructuringFormat.Replace(initialKey, destructuringKey);

                var formattingValue = formattingValues.First(f => f.Key == destructuringParameter.Key);
                var index = formattingValues.IndexOf(formattingValue,0);
                formattingValues[index] = new KeyValuePair<string, object?>($"@{destructuringParameter.Key}", formattingValue.Value);
            }

            var customLogValueFormatter = new CustomLogValuesFormatter(destructuringFormat);

            base.Log(logLevel, eventId, formattingValues, exception, (state, ex) =>
            {
                return customLogValueFormatter.Format([.. state.Where(v=>v.Key != OriginalFormatKey).Select(f=>f.Value)]);
            });
        }
        else 
            base.Log(logLevel, eventId, state, exception, formatter);
    }

    private static bool IsTypeDestructuring(Type type)
    {
        return !type.IsPrimitive && type != typeof(string) && !type.IsEnum
            && (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Nullable<>)
            || (!Nullable.GetUnderlyingType(type)?.IsPrimitive == false && Nullable.GetUnderlyingType(type) != typeof(string)));
    }
}