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
    Dictionary<string, object>? allowedСontextProperties =  null) : ILogEventSink
{
    private static readonly Encoding DefaultEncoding;

    static HangfireConsoleContextSink()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        DefaultEncoding = Encoding.GetEncoding("windows-1251");
    }

    public void Emit(LogEvent logEvent)
    {
        if (HangfireConsoleContext.Current == null)
            return;

        if (restrictedToMininmumLevel.HasValue && logEvent.Level < restrictedToMininmumLevel.Value)
            return;

        if (allowedСontextProperties?.All(acp => logEvent.Properties.TryGetValue(acp.Key, out var propertyValue)
            && propertyValue is ScalarValue scalarValue && scalarValue != null
            && scalarValue.Value?.ToString() == acp.Value.ToString()) != true)
            return;

        HangfireConsoleContext.Current?.WriteLine(GetFormattedLogMessage(logEvent, encoding ?? DefaultEncoding));
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
}