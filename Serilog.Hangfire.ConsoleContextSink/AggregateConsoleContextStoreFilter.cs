using Hangfire.Server;

namespace Serilog.HangfireConsoleContextSink;

internal class AggregateConsoleContextStoreFilter(string idPropertyName = AggregateConsoleContextStoreFilter.DefaultIdPropertyName,
    string isAggregatePropertyName = AggregateConsoleContextStoreFilter.DefaultIsAggregatePropertyName,
    string parentIdPropertyName = AggregateConsoleContextStoreFilter.DefaultParentIdPropertyName) : ConsoleContextFilter
{
    internal const string DefaultIdPropertyName = "id";
    internal const string DefaultIsAggregatePropertyName = "isAggregate";
    internal const string DefaultParentIdPropertyName = "parentId";

    private const string IdJobParameter = "id";

    //public void OnPerforming(PerformingContext filterContext)
    //{
    //    var id = filterContext.GetJobParameter<string>(idPropertyName);
    //    if (string.IsNullOrEmpty(id)) return;

    //    var isAggregate = filterContext.GetJobParameter<bool>(isAggregatePropertyName);

    //    if (isAggregate)
    //    {
    //        AggregateConsoleContextStore.TryAddAggregatePerformContext(id, filterContext);

    //        return;
    //    }

    //    var parentId = filterContext.GetJobParameter<string>(parentIdPropertyName);

    //    if(!string.IsNullOrEmpty(parentId))        
    //        AggregateConsoleContextStore.SetParentPerformContextIfNeed(id, parentId);        
    //}

    public override void OnPerforming(PerformingContext filterContext)
    {
        base.OnPerforming(filterContext);

        var job = filterContext.BackgroundJob.Job;

        var parameters = job.Method.GetParameters()?
           .Where(p => !string.IsNullOrEmpty(p.Name))
            .Select((p, index) => new { Name = p.Name!, Value = job.Args[index] })
            .ToDictionary(x => x.Name, x => x.Value);

        var id = parameters?.TryGetValue(idPropertyName, out var idValue) == true && idValue != null ? idValue.ToString() : null;
        if (string.IsNullOrEmpty(id)) return;

        if (parameters?.TryGetValue(isAggregatePropertyName, out var isAggregateValue) == true && isAggregateValue is bool isAggregate && isAggregate)
        {
            AggregateConsoleContextStore.TryAddAggregatePerformContext(id, filterContext);
            filterContext.SetJobParameter(IdJobParameter, id);
            return;
        }

        var parentId = parameters?.TryGetValue(parentIdPropertyName, out var parentIdValue) == true && parentIdValue != null
            ? parentIdValue.ToString()
            : null;

        if (!string.IsNullOrEmpty(parentId))
        {
            AggregateConsoleContextStore.SetParentPerformContextIfNeed(id, parentId);
            filterContext.SetJobParameter(IdJobParameter, id);
        }
    }
}