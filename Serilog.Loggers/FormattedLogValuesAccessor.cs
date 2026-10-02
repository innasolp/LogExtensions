using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Serilog.Loggers;

internal static class FormattedLogValuesAccessor
{
    private static readonly ConcurrentDictionary<Type, Func<object, string?>?> _getterOriginalFormatCache = new();

    private static readonly ConcurrentDictionary<Type, Func<object, object?[]?>?> _getterValuesCache = new();

    private static readonly string[] OriginalFormatKeys = { "_originalMessage", "OriginalFormat" };

    private const string OriginalFormatPropertyKey = "OriginalFormat";

    private const string ValuesFieldKey = "_values";

    private const string StateKey = "state";

    public static string? ExtractOriginalFormat(object state)
    {
        var type = state.GetType();

        var getter = _getterOriginalFormatCache.GetOrAdd(type, t =>
        {
            var field = OriginalFormatKeys.FirstOrDefault(originalFormatKey =>
                t.GetField(originalFormatKey, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) != null);

            if (field != null)
                return BuildStateExpression(t, field, (cast, member) => Expression.Field(cast, member));

            var prop = t.GetProperty(OriginalFormatPropertyKey, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (prop != null && prop.CanRead)
                return BuildStateExpression(t, prop, (cast, member) => Expression.Property(cast, member));

            return null;
        });

        return getter != null ? getter(state) : null;
    }

    private static Func<object, string?> BuildStateExpression<TMemberInfo>(Type t, 
        TMemberInfo memberInfo, 
        Func<Expression, TMemberInfo, MemberExpression> getMemberExpression)
        where TMemberInfo: class
    {
        var param = Expression.Parameter(typeof(object), StateKey);
        var cast = Expression.Convert(param, t);
        var memberAccess = getMemberExpression(cast, memberInfo);
        return Expression.Lambda<Func<object, string?>>(memberAccess, param).Compile();        
    }

    public static object?[]? ExtractValues(object state)
    {
        var type = state.GetType();

        var getter = _getterValuesCache.GetOrAdd(type, t =>
        {
            var valuesField = t.GetField("_values", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            
            var param = Expression.Parameter(typeof(object), "state");
            var cast = Expression.Convert(param, t);

            var valuesLambda = Expression.Lambda<Func<object, object?[]>>(
                Expression.Field(cast, valuesField), param).Compile();

            return valuesLambda;
        });

        return getter != null ? getter(state) : null;
    }
}