#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>One step-completion row for command surfaces (ADR-0011 / ADR-0012).</summary>
public sealed record ArgCompletionItem(
    string InsertText,
    string CommandPath,
    string Help,
    string? Group = null,
    string? StepSegment = null,
    ArgCompletionItemKind Kind = ArgCompletionItemKind.Segment,
    string? PickValue = null);

/// <summary>Hierarchy header for command popup (domain → object → intent).</summary>
public sealed record ArgCompletionHierarchy(
    string PathPrefix,
    string NextStepLabel,
    string Breadcrumb);
