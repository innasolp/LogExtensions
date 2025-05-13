using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.File;
using System.Text;


namespace Serilog.ContextFileSink;

internal class ContextPropertyFileSink(
    LoggerSinkConfiguration loggerSinkConfiguration,
    string propertyName,
    string pathFormat,
    ITextFormatter textFormatter,
    LogEventLevel? restrictedToMininmumLevel,
    long? fileSizeLimitBytes,
    LoggingLevelSwitch? loggingLevelSwitch,
    string? outputTemplate,
    Encoding? encoding,
    bool buffered = false,
    FileLifecycleHooks? hooks = null,
    RollingInterval? rollingInterval = null,
    int? retainedFileCountLimit = null,
     bool? rollOnFileSizeLimit = null,
     TimeSpan? retainedFileTimeLimit = null) : ILogEventSink
{
    private readonly Dictionary<string, ILogEventSink> _fileSinks = [];   

    private readonly LoggerSinkConfiguration _loggerSinkConfiguration = loggerSinkConfiguration;

    private readonly string _propertyName = propertyName;
    private readonly string _pathFormat = pathFormat;
    private readonly ITextFormatter _textFormatter = textFormatter;
    private readonly LogEventLevel? _restrictedToMininmumLevel = restrictedToMininmumLevel;
    private readonly long? _fileSizeLimitBytes = fileSizeLimitBytes;
    private readonly LoggingLevelSwitch? _loggingLevelSwitch = loggingLevelSwitch;
    private  readonly string? _outputTemplate = outputTemplate;
    private readonly bool _buffered = buffered;
    private readonly Encoding _encoding = encoding ?? new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private readonly FileLifecycleHooks? _hooks = hooks;

    private readonly RollingInterval? _rollingInterval = rollingInterval;
    private readonly int? _retainedFileCountLimit = retainedFileCountLimit;
    private readonly bool? _rollOnFileSizeLimit = rollOnFileSizeLimit;
    private readonly TimeSpan? _retainedFileTimeLimit = retainedFileTimeLimit;

    public void Emit(LogEvent logEvent)
    {
        if (!logEvent.Properties.TryGetValue(_propertyName, out var propertyValue) || propertyValue == null) return;

        var value = propertyValue.ToString().Replace("\"", "");
        var path = _pathFormat.Replace("{" + _propertyName + "}", value);

        if (!_fileSinks.TryGetValue(path, out var sink) || sink == null)
        {
            if (string.IsNullOrEmpty(_outputTemplate))
                sink = LoggerSinkConfiguration.CreateSink( l=> l.File(_textFormatter, path, _restrictedToMininmumLevel ?? LogEventLevel.Verbose, _fileSizeLimitBytes, _loggingLevelSwitch, _buffered, false, null, _rollingInterval ?? RollingInterval.Infinite, _rollOnFileSizeLimit ?? false, _retainedFileCountLimit, _encoding, _hooks, _retainedFileTimeLimit));
            else
                sink = LoggerSinkConfiguration.CreateSink(l=>l.File(path, _restrictedToMininmumLevel ?? LogEventLevel.Verbose, _outputTemplate, null,  _fileSizeLimitBytes, _loggingLevelSwitch, _buffered, false, null, _rollingInterval ?? RollingInterval.Infinite, _rollOnFileSizeLimit ?? false, _retainedFileCountLimit, _encoding, _hooks, _retainedFileTimeLimit));

            _fileSinks.Add(path, sink);
            _loggerSinkConfiguration.Sink(sink, _restrictedToMininmumLevel ?? LogEventLevel.Verbose, _loggingLevelSwitch);
        }

        sink.Emit(logEvent);
    }
}
