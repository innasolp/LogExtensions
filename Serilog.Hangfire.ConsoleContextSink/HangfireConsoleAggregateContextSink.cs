using Hangfire.Console;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using System.Text;

namespace Serilog.HangfireConsoleContextSink;

public class HangfireConsoleAggregateContextSink(
    ITextFormatter textFormatter,
    LogEventLevel? restrictedToMininmumLevel,
    Encoding? encoding) : ILogEventSink
{
    private static readonly Encoding DefaultEncoding;

    private const string IdJobParameter = "id";

    static HangfireConsoleAggregateContextSink()
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

        var id = HangfireConsoleContext.Current.GetJobParameter<string>(IdJobParameter);        
        var outputPerformContext = !string.IsNullOrEmpty(id) && AggregateConsoleContextStore.TryGetPerformContext(id, out var performContext) && performContext != null
            ? performContext
            : HangfireConsoleContext.Current;

         var message = Helper.GetFormattedLogMessage(textFormatter, logEvent, encoding ?? DefaultEncoding);
         outputPerformContext.WriteLine(message);
    }
}