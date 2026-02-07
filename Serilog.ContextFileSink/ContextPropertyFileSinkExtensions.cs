using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.File;
using System.Text;

namespace Serilog.ContextFileSink;

public static class ContextPropertyFileSinkExtensions
{
    const int DefaultRetainedFileCountLimit = 31; // A long month of logs
    const long DefaultFileSizeLimitBytes = 1L * 1024 * 1024 * 1024; // 1GB
    const string DefaultOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
      
    public static LoggerConfiguration ContextPropertyFile(
        this LoggerSinkConfiguration sinkConfiguration,
        string propertyName,
        string pathFormat,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        string outputTemplate = DefaultOutputTemplate,
        IFormatProvider? formatProvider = null,
        long? fileSizeLimitBytes = DefaultFileSizeLimitBytes,
        LoggingLevelSwitch? levelSwitch = null,
        bool buffered = false,
        RollingInterval rollingInterval = RollingInterval.Infinite,
        bool rollOnFileSizeLimit = false,
        int? retainedFileCountLimit = DefaultRetainedFileCountLimit,
        Encoding? encoding = null,
        FileLifecycleHooks? hooks = null,
        TimeSpan? retainedFileTimeLimit = null)
    {
        ArgumentNullException.ThrowIfNull(sinkConfiguration);
        ArgumentNullException.ThrowIfNull(pathFormat);
        ArgumentNullException.ThrowIfNull(outputTemplate);

        var formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
        var sink = new ContextPropertyFileSink(sinkConfiguration, propertyName,pathFormat, formatter,  restrictedToMinimumLevel, fileSizeLimitBytes,
            levelSwitch, outputTemplate, encoding, buffered, hooks, rollingInterval,retainedFileCountLimit, rollOnFileSizeLimit, 
             retainedFileTimeLimit);
        return sinkConfiguration.Sink(sink);
    }
}