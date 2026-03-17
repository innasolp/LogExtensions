using Log.Interceptors;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using System.Resources;

namespace Serilog.Loggers;

public class SerilogAssemblyResourcePropertyLogger : LogInterceptor
{
    private const string OriginalFormatKey = "{OriginalFormat}";

    private readonly Dictionary<string, string> _messageFormatsResourceKeys = [];

    private readonly IReadOnlyDictionary<string, Dictionary<string, object>> _resourceProperties;

    public SerilogAssemblyResourcePropertyLogger(Microsoft.Extensions.Logging.ILogger coreLogger,
        IReadOnlyDictionary<string, Assembly> assemblyResources,
        IReadOnlyDictionary<string, Dictionary<string, object>> resourceProperties) : base(coreLogger)
    {
        foreach (var assemblyResource in assemblyResources)
        {
            var resourceManager = new ResourceManager(assemblyResource.Key, assemblyResource.Value);
            var stringEntries = resourceManager.GetResourceSet(System.Globalization.CultureInfo.InvariantCulture, true, false)?
                          .OfType<DictionaryEntry>().Where(r => r.Value is not null && r.Value is string) ?? [];

            foreach (var entry in stringEntries)
                _messageFormatsResourceKeys.TryAdd(entry.Value!.ToString()!, assemblyResource.Key);
        }

        _resourceProperties = resourceProperties;
    }

    public override void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (state is not IEnumerable<KeyValuePair<string, object?>> values)
        {
            base.Log(logLevel, eventId, state, exception, formatter);
            return;
        }

        var originalFormat = values.FirstOrDefault(v => v.Key == OriginalFormatKey && v.Value != null).Value?.ToString();
        if (string.IsNullOrEmpty(originalFormat))
        {
            base.Log(logLevel, eventId, state, exception, formatter);
            return;
        }

        if (!_messageFormatsResourceKeys.TryGetValue(originalFormat, out var resourceKey) || string.IsNullOrEmpty(resourceKey))
        {
            base.Log(logLevel, eventId, state, exception, formatter);
            return;
        }

        if (!_resourceProperties.TryGetValue(resourceKey, out var properties) || properties?.Any() != true)
        {
            base.Log(logLevel, eventId, state, exception, formatter);
            return;
        }

        var disposables = properties.Select(p => LogContext.PushProperty(p.Key, p.Value)).ToImmutableArray();

        try
        {
            base.Log(logLevel, eventId, state, exception, formatter);
        }
        finally
        {
            foreach (var disposable in disposables)
                disposable.Dispose();
        }
    }
}

public class SerilogAssemblyResourcePropertyLogger<T>(Microsoft.Extensions.Logging.ILogger coreLogger,
    Dictionary<string, Assembly> assemblyResources,
    Dictionary<string, Dictionary<string, object>> resourceProperties)
    : SerilogAssemblyResourcePropertyLogger(coreLogger, assemblyResources, resourceProperties), ILogger<T>
{
}