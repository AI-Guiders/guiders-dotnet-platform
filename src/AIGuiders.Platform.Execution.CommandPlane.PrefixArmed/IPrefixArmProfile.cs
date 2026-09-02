#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane;

public enum PrefixArmDisposition
{
    NoMatch = 0,
    Ready = 1,
    ArmConstructor = 2,
}

/// <summary>Result of a prefix-arm profile match (GUIDERS-ADR-0038).</summary>
public sealed record PrefixArmMatch(
    PrefixArmDisposition Disposition,
    string? Wire = null,
    string? DisplayTail = null,
    string? RootConstructorId = null,
    IReadOnlyDictionary<string, string>? Segments = null)
{
    public static PrefixArmMatch NoMatch { get; } = new(PrefixArmDisposition.NoMatch);
}

/// <summary>Product-provided prefix lexer for PAC — not tied to dates, slash, or any single surface.</summary>
public interface IPrefixArmProfile
{
    string ProfileId { get; }

    bool TryMatch(string partial, PrefixArmSite site, out PrefixArmMatch match);
}
