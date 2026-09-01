#nullable enable

namespace AIGuiders.Platform.IntermediateRepresentation.Command;

/// <summary>Structured arg-menu from <c>.catalog</c> profiles (GUIDERS-ADR-0047 §8).</summary>
public sealed class ArgTailProfile
{
    public required string Name { get; init; }
    public IReadOnlyList<ArgTailMenuEntry> Menu { get; init; } = [];
}

public sealed record ArgTailMenuEntry(string Arg, ArgTailEntryKind Kind, string Ref);

public enum ArgTailEntryKind
{
    Preset,
    Constructor,
    FreeText,
    PickerForSlot,
}
