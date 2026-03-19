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
        Encoding? encoding = null)
    {
        ArgumentNullException.ThrowIfNull(sinkConfiguration);
        ArgumentNullException.ThrowIfNull(outputTemplate);        

        var formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
        var sink = new HangfireConsoleContextSink(formatter, restrictedToMinimumLevel, encoding);
        return sinkConfiguration.Sink(sink);
    }

    public static LoggerConfiguration HangfireAggregateConsoleContext(
        this LoggerSinkConfiguration sinkConfiguration,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        string outputTemplate = DefaultOutputTemplate,
        IFormatProvider? formatProvider = null,
        Encoding? encoding = null)
    {
        ArgumentNullException.ThrowIfNull(sinkConfiguration);
        ArgumentNullException.ThrowIfNull(outputTemplate);        

        var formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
        var sink = new HangfireConsoleAggregateContextSink(formatter, restrictedToMinimumLevel, encoding);
        return sinkConfiguration.Sink(sink);
    }
}