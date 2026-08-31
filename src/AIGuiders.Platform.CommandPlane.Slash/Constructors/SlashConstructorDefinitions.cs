#nullable enable

namespace AIGuiders.Platform.CommandPlane;

public abstract record SlashConstructorDefinition(string Id, string? Label);

public sealed record SlashConstructorSegmentDefinition(
    string SegmentId,
    string Label);

public sealed record SlashLeafConstructorDefinition(
    string Id,
    string? Label,
    IReadOnlyList<SlashConstructorSegmentDefinition> Segments,
    string WirePattern,
    string DisplayPattern)
    : SlashConstructorDefinition(Id, Label);

public sealed record SlashConstructorSlotDefinition(
    string SlotId,
    string ConstructorId,
    string? Label,
    string? SeparatorBefore = null);

public sealed record SlashCompositeConstructorDefinition(
    string Id,
    string? Label,
    IReadOnlyList<SlashConstructorSlotDefinition> Slots,
    string WirePattern)
    : SlashConstructorDefinition(Id, Label);
