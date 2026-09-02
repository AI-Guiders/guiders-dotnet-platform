#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane;

public abstract record ConstructorDefinition(string Id, string? Label);

public sealed record ConstructorSegmentDefinition(
    string SegmentId,
    string Label,
    int? WireMinWidth = null,
    int? DisplayMinWidth = null);

public sealed record LeafConstructorDefinition(
    string Id,
    string? Label,
    IReadOnlyList<ConstructorSegmentDefinition> Segments,
    string WirePattern,
    string DisplayPattern)
    : ConstructorDefinition(Id, Label);

public sealed record ConstructorSlotDefinition(
    string SlotId,
    string ConstructorId,
    string? Label,
    string? SeparatorBefore = null);

public sealed record CompositeConstructorDefinition(
    string Id,
    string? Label,
    IReadOnlyList<ConstructorSlotDefinition> Slots,
    string WirePattern)
    : ConstructorDefinition(Id, Label);
