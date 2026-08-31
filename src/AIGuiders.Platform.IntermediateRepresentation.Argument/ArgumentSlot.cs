namespace AIGuiders.Platform.IntermediateRepresentation.Argument;

public enum ArgumentSlotKind
{
    Flag,
    Value,
    Positional,
}

/// <summary>Per-commandId arg slot schema (catalog / capabilities).</summary>
public sealed record ArgumentSlot(
    string Name,
    ArgumentSlotKind Kind = ArgumentSlotKind.Value,
    string? LongOption = null,
    string? ShortOption = null);
