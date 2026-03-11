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
    string[]? jobParameters = null
    ) : ILogEventSink
{
    private static readonly Encoding DefaultEncoding = Encoding.GetEncoding("windows-1251");

    public void Emit(LogEvent logEvent)
    {
        if (HangfireConsoleContext.Current == null)
            return;

        if (logEvent.Level < restrictedToMininmumLevel)
            return;

        if (contextProperties != null && !logEvent.Properties.All(p => contextProperties.Contains(p.Key)))
            return;

        var context = HangfireConsoleContext.Current;
        if (jobParameters != null && !jobParameters.All(p => context.GetJobParameter<string>(p) == logEvent.Properties[p].ToString()))
            return;

        var currentEncoding = encoding ?? DefaultEncoding;
        
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, currentEncoding);
        textFormatter.Format(logEvent, writer);
        writer.Flush();

        byte[] encodedBytes = stream.ToArray();
        context?.WriteLine(currentEncoding.GetString(encodedBytes));
    }
}