#nullable enable

namespace AIGuiders.Platform.CommandPlane;

public sealed class ArgConstructorDraft
{
    public required string RootConstructorId { get; init; }
    public required string CanonicalPath { get; init; }
    public int SlotIndex { get; set; }
    public int SegmentIndex { get; set; }
    public List<CompletedSlot> CompletedSlots { get; } = [];
    public Dictionary<string, string> ActiveSegments { get; } = new(StringComparer.OrdinalIgnoreCase);

    public string DisplayBuffer { get; set; } = "";
    public string WireBuffer { get; set; } = "";

    public DateOnly? AnchorDate { get; set; }

    public sealed record CompletedSlot(string SlotId, IReadOnlyDictionary<string, string> Segments);
}
