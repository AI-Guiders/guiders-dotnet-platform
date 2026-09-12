#nullable enable

using CommandNotation = AIGuiders.Platform.Modeling.Notations.Command;

namespace AIGuiders.Platform.IntermediateRepresentation.Invocation;

/// <summary>
/// GUIDERS-FSHARP-ADR-0003 §4.3 cutover: core fields SSOT via <see cref="ToModel"/> (Modeling.Notations.Command).
/// </summary>
public sealed record NormalizedCommandLine(
    string CanonicalPath,
    IReadOnlyList<string> PathSegments)
{
    public CommandNotation.NormalizedCommandLine ToModel() => new(
        CanonicalPath,
        PathSegments);

    public static NormalizedCommandLine FromModel(CommandNotation.NormalizedCommandLine model) => new(
        model.CanonicalPath,
        model.PathSegments);
}
