#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>One step-completion row for slash surfaces (ADR-0011 / ADR-0012).</summary>
public sealed record SlashCompletionItem(
    string InsertText,
    string SlashPath,
    string Help,
    string? Group = null,
    string? StepSegment = null,
    SlashCompletionItemKind Kind = SlashCompletionItemKind.Segment,
    string? PickValue = null);

/// <summary>Hierarchy header for slash popup (domain → object → intent).</summary>
public sealed record SlashCompletionHierarchy(
    string PathPrefix,
    string NextStepLabel,
    string Breadcrumb);
