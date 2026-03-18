using Hangfire.Server;
using System.Collections.Concurrent;

namespace Serilog.HangfireConsoleContextSink;

internal static class AggregateConsoleContextStore
{
    private static readonly ConcurrentDictionary<string, PerformContext> _aggregatePerformContexts = [];

    private static readonly ConcurrentDictionary<string, PerformContext> _parentPerformContexts = [];

    public static bool TryAddAggregatePerformContext(string aggregateId, PerformContext context)
    {
        if (!_aggregatePerformContexts.ContainsKey(aggregateId))
        {
            _aggregatePerformContexts[aggregateId] = context;
            return true;
        }

        return _aggregatePerformContexts.TryAdd(aggregateId, context);
    }

    public static bool TryGetAggregatePerformContext(string aggregateId, out PerformContext? context)
    {
        return _aggregatePerformContexts.TryGetValue(aggregateId, out context) && context != null;
    }

    public static void SetParentPerformContextIfNeed(string id, string parentId)
    {
        if (!string.IsNullOrEmpty(parentId) &&
            _aggregatePerformContexts.TryGetValue(parentId, out PerformContext? parentPerformContext) && parentPerformContext != null)
        {
            if(!_parentPerformContexts.ContainsKey(id))
                _aggregatePerformContexts[id] = parentPerformContext;
            else  
                _aggregatePerformContexts.TryAdd(id, parentPerformContext);
        }
    }

    public static bool TryGetPerformContext(string id, out PerformContext? context)
    {
        context = null;

        if (_aggregatePerformContexts.ContainsKey(id))
            return _aggregatePerformContexts.TryGetValue(id, out context);

        if (_parentPerformContexts.ContainsKey(id))
            return _parentPerformContexts.TryGetValue(id, out context);

        return false;
    }
}