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

    private readonly Dictionary<string, ResourceManager> _resourceManagers = [];

    private readonly Dictionary<string, string> _messageFormatsResourceKeys = [];

    private readonly Dictionary<string, Dictionary<string, object>> _resourceProperties;

    public SerilogAssemblyResourcePropertyLogger(Microsoft.Extensions.Logging.ILogger coreLogger,
        Dictionary<string, Assembly> assemblyResources, Dictionary<string, Dictionary<string, object>> resourceProperties) : base(coreLogger)
    {
        foreach (var assemblyResource in assemblyResources)
        {
            var resourceManager = new ResourceManager(assemblyResource.Key,assemblyResource.Value);
            _resourceManagers.TryAdd(assemblyResource.Key, resourceManager);
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
            resourceKey = _resourceManagers
                .SelectMany(m => m.Value.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, false, false)?.OfType<DictionaryEntry>() ?? [])
                .FirstOrDefault(r => r.Value is string messageFormat && messageFormat == originalFormat).Value as string;

            if (!string.IsNullOrEmpty(resourceKey))
                _messageFormatsResourceKeys.TryAdd(originalFormat, resourceKey);
            else
            {
                base.Log(logLevel, eventId, state, exception, formatter);
                return;
            }
        }

        if (!_resourceProperties.TryGetValue(resourceKey, out var properties) || properties?.Any() != true)
        {
            base.Log(logLevel, eventId, state, exception, formatter);
            return;
        }

        var disposables = properties.Select(p => LogContext.PushProperty(p.Key, p.Value)).ToImmutableArray();

        base.Log(logLevel, eventId, state, exception, formatter);

        foreach (var disposable in disposables)
            disposable.Dispose();
    }
}

public class SerilogAssemblyResourcePropertyLogger<T>(Microsoft.Extensions.Logging.ILogger coreLogger, 
    Dictionary<string, Assembly> assemblyResources,
    Dictionary<string, Dictionary<string, object>> resourceProperties) 
    : SerilogAssemblyResourcePropertyLogger(coreLogger, assemblyResources, resourceProperties), ILogger<T>
{
}