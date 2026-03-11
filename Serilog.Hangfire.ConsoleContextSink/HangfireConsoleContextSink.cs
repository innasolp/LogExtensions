using Hangfire.Common;
using Hangfire.Console;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using System.Text;

namespace Serilog.HangfireConsoleContextSink;

public class HangfireConsoleContextSink(
    ITextFormatter textFormatter,
    LogEventLevel? restrictedToMininmumLevel,
    Encoding? encoding,
    string[]? contextProperties = null,
    string[]? jobParameters = null) : ILogEventSink
{
    private static readonly Encoding DefaultEncoding = Encoding.GetEncoding("windows-1251");

    private Encoding CurrentEncoding =>  encoding ?? DefaultEncoding;

    public void Emit(LogEvent logEvent)
    {
        if (HangfireConsoleContext.Current == null)
            return;

        if (restrictedToMininmumLevel.HasValue && logEvent.Level < restrictedToMininmumLevel.Value)
            return;

        if (contextProperties == null != (jobParameters == null))
            return;

        if (contextProperties != null && !contextProperties.All(logEvent.Properties.ContainsKey))
            return;

        var context = HangfireConsoleContext.Current;

        if (contextProperties != null && jobParameters != null && !CompareContextWithJobParameters(logEvent, context, CurrentEncoding))
            return;           

        context?.WriteLine(GetFormattedLogMessage(logEvent, CurrentEncoding));
    }

    private bool CompareContextWithJobParameters(LogEvent logEvent, Hangfire.Server.PerformContext context, Encoding currentEncoding)
    {
        var contextPropertyValues = new string?[contextProperties!.Length];
        for (int i = 0; i < contextProperties.Length; i++)
        {
            var propertyValue = logEvent.Properties[contextProperties[i]] is ScalarValue scalarValue
                ? scalarValue.Value?.ToString()
                : GetRenderedLogPropertyValue(logEvent.Properties[contextProperties[i]], currentEncoding);

            contextPropertyValues[i] = propertyValue;
        }

        var jobParameterValues = GetJobParameterValues(context.BackgroundJob.Job, jobParameters!);

        if (!contextPropertyValues.All(c =>
                    jobParameterValues.Any(j => j?.ToString()?.Equals(c, StringComparison.InvariantCultureIgnoreCase) == true)))
            return false;
        return true;
    }

    private string? GetFormattedLogMessage(LogEvent logEvent, Encoding currentEncoding)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, currentEncoding);
        textFormatter.Format(logEvent, writer);
        writer.Flush();

        byte[] encodedBytes = stream.ToArray();
        return currentEncoding.GetString(encodedBytes);
    }

    private static object[] GetJobParameterValues(Job job, string[] parameterNames )
    {
        var parameters = job.Method.GetParameters();

        var result = new object[parameterNames.Length];       

        var j = 0;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameterNames.Contains(parameters[i].Name))
            {
                result[j] = job.Args[i];
                j++;
            }  
        }

        return result;
    }

    private static string? GetRenderedLogPropertyValue(LogEventPropertyValue logEventPropertyValue, Encoding encoding)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, encoding);
        logEventPropertyValue.Render(writer);
        writer.Flush();

        byte[] encodedBytes = stream.ToArray();
        return encoding.GetString(encodedBytes);
    }
}