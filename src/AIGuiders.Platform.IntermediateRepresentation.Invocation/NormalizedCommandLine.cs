namespace AIGuiders.Platform.IntermediateRepresentation.Invocation;

/// <summary>Post-resolve command path (catalog longest-prefix or explicit console path).</summary>
public sealed record NormalizedCommandLine(
    string CanonicalPath,
    IReadOnlyList<string> PathSegments);
