#nullable enable

namespace AIGuiders.Platform.Notations;

/// <summary>Legacy alias for <see cref="Command.NormalizedCommandLine"/> (GUIDERS-ADR-0021 Wave 4b).</summary>
[Obsolete("Use AIGuiders.Platform.Notations.Command.NormalizedCommandLine and package AIGuiders.Platform.Notations.Command.")]
public sealed record NormalizedCommandLine(
    string CanonicalPath,
    IReadOnlyList<string> PathSegments)
{
    public static implicit operator Command.NormalizedCommandLine(NormalizedCommandLine line) =>
        new(line.CanonicalPath, line.PathSegments);

    public static implicit operator NormalizedCommandLine(Command.NormalizedCommandLine line) =>
        new(line.CanonicalPath, line.PathSegments);
}

/// <summary>Legacy alias for <see cref="Command.SlashWireBody"/> (GUIDERS-ADR-0021 Wave 4b).</summary>
[Obsolete("Use AIGuiders.Platform.Notations.Command.SlashWireBody and package AIGuiders.Platform.Notations.Command.")]
public sealed record SlashWireBody(
    IReadOnlyList<string> Tokens,
    bool EndsWithSpaceAfterTokens)
{
    public string JoinedTokens => string.Join(' ', Tokens);

    public static implicit operator Command.SlashWireBody(SlashWireBody body) =>
        new(body.Tokens, body.EndsWithSpaceAfterTokens);

    public static implicit operator SlashWireBody(Command.SlashWireBody body) =>
        new(body.Tokens, body.EndsWithSpaceAfterTokens);
}

/// <summary>Legacy alias for <see cref="Command.InvocationNotation"/> (GUIDERS-ADR-0021 Wave 4b).</summary>
[Obsolete("Use AIGuiders.Platform.Notations.Command.InvocationNotation and package AIGuiders.Platform.Notations.Command.")]
public static class InvocationNotation
{
    public static NormalizedCommandLine FromPathSegments(IReadOnlyList<string> segments) =>
        Command.InvocationNotation.FromPathSegments(segments);

    public static bool PathsEqual(NormalizedCommandLine a, NormalizedCommandLine b) =>
        Command.InvocationNotation.PathsEqual(a, b);
}
