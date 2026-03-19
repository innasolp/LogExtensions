using Serilog.Events;
using Serilog.Formatting;
using System.Text;

namespace Serilog.HangfireConsoleContextSink;

internal static class Helper
{
    public static string? GetFormattedLogMessage(ITextFormatter textFormatter, LogEvent logEvent, Encoding currentEncoding)
    {
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, currentEncoding);
        textFormatter.Format(logEvent, writer);
        writer.Flush();

        byte[] encodedBytes = stream.ToArray();
        return currentEncoding.GetString(encodedBytes);
    }
}