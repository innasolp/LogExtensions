using Hangfire.Console;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using System.Text;

namespace Serilog.HangfireConsoleContextSink;

public class HangfireConsoleContextSink(
    ITextFormatter textFormatter,
    LogEventLevel? restrictedToMininmumLevel,
    Encoding? encoding) : ILogEventSink
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

        HangfireConsoleContext.Current?.WriteLine(Helper.GetFormattedLogMessage(textFormatter, logEvent, encoding ?? DefaultEncoding));
    }
}