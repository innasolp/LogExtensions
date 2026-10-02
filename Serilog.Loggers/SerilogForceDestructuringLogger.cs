using Log.Interceptors;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Serilog.Loggers;

public class SerilogForceDestructuringLogger(Microsoft.Extensions.Logging.ILogger coreLogger) : LogInterceptor(coreLogger)
{
    private const string OriginalFormatKey = "OriginalFormat";

    private static readonly Regex ParameterRegex = new(@"\{([a-zA-Z0-9_]+)\}", RegexOptions.Compiled);

    public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (state is not IEnumerable<KeyValuePair<string, object?>> values)
        {
            base.Log(logLevel, eventId, state, exception, formatter);
            return;
        }

        var originalValues = FormattedLogValuesAccessor.ExtractValues(state);

        if (originalValues?.Any() != true)
        {
            base.Log(logLevel, eventId, state, exception, formatter);
            return;
        }

        var paramsNeedDestructuring = originalValues?.Where(v => v is not null && v.ToString()?.Contains('@') == false
                 && IsTypeDestructuring(v.GetType()));
        if (!(paramsNeedDestructuring?.Any() == true))
        {
            base.Log(logLevel, eventId, state, exception, formatter);
            return;
        }

        var originalFormat = FormattedLogValuesAccessor.ExtractOriginalFormat(state);
        if (string.IsNullOrEmpty(originalFormat))
        {
            base.Log(logLevel, eventId, state, exception, formatter);
            return;
        }

        var matches = ParameterRegex.Matches(originalFormat);
        if (matches.Count == 0)
        {
            base.Log(logLevel, eventId, state, exception, formatter);
            return;
        }


        var destructuringFormat = originalFormat;
        var modified = false;
        var formattingValues = new Dictionary<string,object?>();

        for (int i = 0; i < matches.Count && i < originalValues!.Length; i++)
        {
            var val = originalValues[i];
            if (val != null && IsTypeDestructuring(val.GetType()))
            {
                var paramName = matches[i].Groups[1].Value;
                var placeholder = $"{{{paramName}}}";
                var destructuringPlaceholder = $"{{@{paramName}}}";

                if (destructuringFormat.Contains(placeholder))
                {
                    destructuringFormat = destructuringFormat.Replace(placeholder, destructuringPlaceholder);
                    modified = true;
                }

                formattingValues.Add($"{destructuringPlaceholder}", val);
            }
        }

        if (!modified)
        {
            base.Log(logLevel, eventId, state, exception, formatter);
            return;
        }

        var customFormatter = new CustomLogValuesFormatter(destructuringFormat);

        base.Log(logLevel, eventId, formattingValues, exception, (state, ex) =>
        {
            return customFormatter.Format([.. state.Where(v => v.Key != OriginalFormatKey).Select(f => f.Value)]);
        });
    }

    private static bool IsTypeDestructuring(Type type)
    {
        if (type.IsPrimitive || type == typeof(string) || type.IsEnum || type == typeof(decimal))
            return false;

        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying != null)
        {
            return !underlying.IsPrimitive && underlying != typeof(string) && !underlying.IsEnum && underlying != typeof(decimal);
        }

        return true;
    }
}