using System.Numerics;

namespace Serilog.Configuration.Extensions;

public class SerilogPropertyExpression
{
    public string? Func { get; }

    public string PropertyName { get; }

    public object? Value { get; } = false;

    public bool SetToOutput { get; }

    public SerilogPropertyExpression(string? func, string propertyName, object? value, bool setToOutput)
    {
        Func = func;
        PropertyName = propertyName;
        Value = value;
        SetToOutput = setToOutput;
    }

    public SerilogPropertyExpression(string propertyName, object? value) : this (null, propertyName, value, false)
    { }

    public SerilogPropertyExpression(string? func, string propertyName, object? value):this(func, propertyName, value, false)
    { }

    public SerilogPropertyExpression(string propertyName, object? value, bool setToOutput):this(null, propertyName, value, setToOutput) 
    { }

    private static string GetFormattedValue(object? value)
    {
        return value == null ? "null" :
            (value.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISignedNumber<>))
             ? value.ToString() : $"'{value}'");
    }

    public string GetExpression()
    {
        if (Func == null || Func == "=")
            return $"{PropertyName}={GetFormattedValue(Value)}";
        else
        {
            return $"{Func}({PropertyName},{GetFormattedValue(Value)})";
        }
    }
}
