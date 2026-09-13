using AIGuiders.Platform.IntermediateRepresentation.Invocation;
using NotationsCommand = global::AIGuiders.Platform.Notations.Command;
#nullable enable

namespace AIGuiders.Platform.Notations;

/// <summary>Legacy alias for <see cref="IntermediateRepresentation.Invocation.NormalizedCommandLine"/> (GUIDERS-ADR-0021 Wave 4b).</summary>
[Obsolete("Use AIGuiders.Platform.IntermediateRepresentation.Invocation.NormalizedCommandLine and package AIGuiders.Platform.IntermediateRepresentation.Invocation.")]
public sealed record NormalizedCommandLine(
    string CanonicalPath,
    IReadOnlyList<string> PathSegments)
{
    public static implicit operator IntermediateRepresentation.Invocation.NormalizedCommandLine(NormalizedCommandLine line) =>
        new(line.CanonicalPath, line.PathSegments);

    public static implicit operator NormalizedCommandLine(IntermediateRepresentation.Invocation.NormalizedCommandLine line) =>
        new(line.CanonicalPath, line.PathSegments);
}

/// <summary>Legacy alias for <see cref="Command.SlashWireBody"/> (GUIDERS-ADR-0021 Wave 4b).</summary>
[Obsolete("Use AIGuiders.Platform.Notations.Command.SlashWireBody and package AIGuiders.Platform.Notations.Command.")]
public sealed record SlashWireBody(
    IReadOnlyList<string> Tokens,
    bool EndsWithSpaceAfterTokens)
{
    public string JoinedTokens => string.Join(' ', Tokens);

    public static implicit operator NotationsCommand.SlashWireBody(SlashWireBody body) =>
        new(body.Tokens, body.EndsWithSpaceAfterTokens);

    public static implicit operator SlashWireBody(NotationsCommand.SlashWireBody body) =>
        new(body.Tokens, body.EndsWithSpaceAfterTokens);
}

/// <summary>Legacy alias for <see cref="Command.InvocationNotation"/> (GUIDERS-ADR-0021 Wave 4b).</summary>
[Obsolete("Use AIGuiders.Platform.Notations.Command.InvocationNotation and package AIGuiders.Platform.Notations.Command.")]
public static class InvocationNotation
{
    public static NormalizedCommandLine FromPathSegments(IReadOnlyList<string> segments)
    {
        var line = NotationsCommand.InvocationNotation.FromPathSegments(segments);
        return new NormalizedCommandLine(line.CanonicalPath, line.PathSegments);
    }

    public static bool PathsEqual(NormalizedCommandLine a, NormalizedCommandLine b) =>
        NotationsCommand.InvocationNotation.PathsEqual(
            new IntermediateRepresentation.Invocation.NormalizedCommandLine(a.CanonicalPath, a.PathSegments),
            new IntermediateRepresentation.Invocation.NormalizedCommandLine(b.CanonicalPath, b.PathSegments));
}
