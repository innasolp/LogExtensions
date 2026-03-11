using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Display;
using System.Text;

namespace Serilog.HangfireConsoleContextSink;

public static class HangfireConsoleContextSinkExtensions
{
    const string DefaultOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

    public static LoggerConfiguration HangfireConsoleContext(
        this LoggerSinkConfiguration sinkConfiguration,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        string outputTemplate = DefaultOutputTemplate,
        IFormatProvider? formatProvider = null,
        Encoding? encoding = null,
        string[]? contextProperties = null,
        string[]? jobParameters = null)
    {
        ArgumentNullException.ThrowIfNull(sinkConfiguration);
        ArgumentNullException.ThrowIfNull(outputTemplate);

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
        var sink = new HangfireConsoleContextSink(formatter, restrictedToMinimumLevel, encoding, contextProperties, jobParameters);
        return sinkConfiguration.Sink(sink);
    }
}