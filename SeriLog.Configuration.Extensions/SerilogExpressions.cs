namespace Serilog.Configuration.Extensions;

public static class SerilogExpressions
{
    public static readonly ContextProperty SourceContext = new("SourceContext");

    public static readonly ContextProperty EventId = new("EventId.Id");
}

[AttributeUsage(AttributeTargets.Field)]
public class SerilogFunctionParametersAttribute(int requiredAttributesCount, int optionalAttributeCount = 0) : Attribute
{
    public SerilogFunctionParametersAttribute() : this(0, 0) { }

    public int RequiredParametersCount { get; } = requiredAttributesCount;

    public int OptionalParametersCount { get; } = optionalAttributeCount;
}

public enum SerilogFunc : short
{
    [SerilogFunctionParameters(2)]
    Contains = 0,

    [SerilogFunctionParameters(2)]
    Coalesce = 1,

    [SerilogFunctionParameters(2, 1)]
    Concat = 2,

    [SerilogFunctionParameters(2)]
    ElementAt = 3,

    [SerilogFunctionParameters(2)]
    EndsWith = 4,

    [SerilogFunctionParameters(2)]
    IndexOf = 5,

    [SerilogFunctionParameters(2)]
    IndexOfMatch = 6,

    [SerilogFunctionParameters(1, 1)]
    Inspect = 7,

    [SerilogFunctionParameters(2)]
    IsMatch = 8,

    [SerilogFunctionParameters(1)]
    IsDefined = 9,

    [SerilogFunctionParameters(2)]
    LastIndexOf = 10,

    [SerilogFunctionParameters(1)]
    Length = 11,

    [SerilogFunctionParameters(1)]
    Nest = 12,

    [SerilogFunctionParameters]
    Now = 13,

    [SerilogFunctionParameters(3)]
    Replace = 14,

    [SerilogFunctionParameters(0, 1)]
    Rest = 15,

    [SerilogFunctionParameters(2)]
    Round = 16,

    [SerilogFunctionParameters(2)]
    StartsWith = 17,

    [SerilogFunctionParameters(2, 1)]
    Substring = 18,

    [SerilogFunctionParameters(1)]
    TagOf = 19,

    [SerilogFunctionParameters(1, 1)]
    ToString = 20,

    [SerilogFunctionParameters(1)]
    TypeOf = 21,

    [SerilogFunctionParameters]
    Undefined = 22,

    [SerilogFunctionParameters(1)]
    UtcDateTime = 23
}
