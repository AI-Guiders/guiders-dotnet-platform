#nullable enable

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Relations;

/// <summary>RelationSpec.Nav projection for navigation land wires (plan §10).</summary>
public sealed record NavResolveAxes(
    string? File,
    int? Line = null,
    int? Column = null,
    string? Command = null,
    string? Go = null,
    string? Solution = null);
