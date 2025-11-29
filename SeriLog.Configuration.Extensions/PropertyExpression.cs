using System.Numerics;
using System.Reflection;

namespace Serilog.Configuration.Extensions;

public record ContextProperty (string Name);

public class PropertyExpression
{
    public string? Func { get; }

    public SerilogFunc? SerilogFunc { get; }    

    public object[]? Parameters { get; }

    public PropertyExpression(SerilogFunc serilogFunc, object[]? parameters = null)
    {
        SerilogFunc = serilogFunc;
        Func = Enum.GetName(serilogFunc);
        Parameters = parameters;
    }

    public PropertyExpression(string func, object[]? parameters = null)        
    {
        Func = func;

        if (Enum.TryParse<SerilogFunc>(func, out var serilogFunc))
            SerilogFunc = serilogFunc;

        Parameters = parameters;
    }

    public PropertyExpression(string func, object value)
        :this(func,  [value])
    {}

    public PropertyExpression(SerilogFunc serilogFunc, object value)
        : this(serilogFunc, [value])
    { }


    private static string GetFormattedValue(object? value)
    {
        if (value is ContextProperty contextProperty) return contextProperty.Name;

        return value != null ? (IsNumeric(value.GetType()) ? value.ToString() : $"'{value}'") : "null";
    }

    private static bool IsNumeric(Type type)
    {
        return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISignedNumber<>));
    }

    public string GetExpression()
    {
        SerilogFunc? serilogFunc = SerilogFunc != null 
            ? SerilogFunc.Value 
            :(Enum.TryParse<SerilogFunc>(Func, out var f) ? f : null);

        if (serilogFunc != null)
        {
            var serilogFuncParameters = typeof(SerilogFunc).GetField(Func)?.GetCustomAttribute<SerilogFunctionParametersAttribute>();

            if (serilogFuncParameters?.RequiredParametersCount > (Parameters?.Length ?? 0))
                throw new InvalidOperationException($@"Invalid serilog expression: parameters for {Func} must be greater then {serilogFuncParameters?.RequiredParametersCount}");

            var takeValues = Parameters?.Take((serilogFuncParameters?.RequiredParametersCount + serilogFuncParameters?.OptionalParametersCount) ??
                serilogFuncParameters?.RequiredParametersCount
                ?? serilogFuncParameters?.OptionalParametersCount ?? 0);

            return $"{Func}({(takeValues?.Count() > 0 ? string.Join(",", takeValues.Select(GetFormattedValue)) : string.Empty)})";
        }
        else if (Parameters?.Length == 2)
            return $"{GetFormattedValue(Parameters?[0])}{Func}{GetFormattedValue(Parameters?[1])}";

        throw new InvalidOperationException($@"Invalid serilog expression: {Func}   
                    [ {(Parameters?.Length > 0 ? string.Join(",", Parameters) : "empty parameters")}]");
    }
}