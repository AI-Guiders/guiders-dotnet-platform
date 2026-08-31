#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Virtual arg-entry row that opens a value constructor tree (GUIDERS-ADR-0035).</summary>
public sealed record SlashConstructorBinding(
    string ConstructorId,
    string Label,
    string? Hint = null);
