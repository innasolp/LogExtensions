using Hangfire.Server;
using System.Collections.Concurrent;

namespace Serilog.HangfireConsoleContextSink;

internal static class AggregateConsoleContextStore
{
    private static readonly ConcurrentDictionary<string, PerformContext> _aggregatePerformContexts = [];

    private static readonly ConcurrentDictionary<string, PerformContext> _parentPerformContexts = [];

    public static bool TrySetAggregatePerformContext(string aggregateId, PerformContext context)
    {
        if (!_aggregatePerformContexts.TryAdd(aggregateId, context))
        {
            _aggregatePerformContexts[aggregateId] = context;
            return true;
        }
        
        return true;
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
            if(!_parentPerformContexts.TryAdd(id, parentPerformContext))
                _parentPerformContexts[id] = parentPerformContext;
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

    public static bool TryRemovePerformContext(string id)
    {
        if (_aggregatePerformContexts.ContainsKey(id))
            return _aggregatePerformContexts.TryRemove(id, out var context);

        if (_parentPerformContexts.ContainsKey(id))
            return _parentPerformContexts.TryRemove(id, out var context);

        return false;
    }
}