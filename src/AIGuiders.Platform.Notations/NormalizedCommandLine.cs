namespace AIGuiders.Platform.Notations;

/// <summary>Post-resolve command path (catalog longest-prefix or explicit console path).</summary>
public sealed record NormalizedCommandLine(
    string CanonicalPath,
    IReadOnlyList<string> PathSegments);
