#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane;

public abstract record PrefixArmResult;

public sealed record PrefixArmNoMatchResult : PrefixArmResult
{
    public static PrefixArmNoMatchResult Instance { get; } = new();
    private PrefixArmNoMatchResult() { }
}

public sealed record PrefixArmReadyResult(
    string CanonicalPath,
    string Wire,
    string DisplayTail,
    string Help,
    string ArgTailKind) : PrefixArmResult;

public sealed record PrefixArmContinuedResult : PrefixArmResult
{
    public static PrefixArmContinuedResult Instance { get; } = new();
    private PrefixArmContinuedResult() { }
}
