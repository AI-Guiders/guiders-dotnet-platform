#nullable enable

namespace AIGuiders.Platform.CommandPlane;

public enum SlashPrefixArmDisposition
{
    NoMatch = 0,
    Ready = 1,
    ArmConstructor = 2,
}

/// <summary>Result of a prefix-arm profile match (GUIDERS-ADR-0038).</summary>
public sealed record SlashPrefixArmMatch(
    SlashPrefixArmDisposition Disposition,
    string? Wire = null,
    string? DisplayTail = null,
    string? RootConstructorId = null,
    IReadOnlyDictionary<string, string>? Segments = null)
{
    public static SlashPrefixArmMatch NoMatch { get; } = new(SlashPrefixArmDisposition.NoMatch);
}

/// <summary>Product-provided prefix lexer for PAC — not tied to dates or any single domain.</summary>
public interface ISlashPrefixArmProfile
{
    string ProfileId { get; }

    bool TryMatch(string partial, SlashRouteEntry route, out SlashPrefixArmMatch match);
}
