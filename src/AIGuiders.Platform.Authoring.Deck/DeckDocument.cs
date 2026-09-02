using AIGuiders.Platform.IntermediateRepresentation.Presentation;

namespace AIGuiders.Platform.Authoring.Deck;

public sealed class DeckDocument
{
    public required string Planet { get; init; }
    public IReadOnlyList<AttentionPreset> Presets { get; init; } = [];
    public IReadOnlyDictionary<string, string> ZoneBindings { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}

public sealed class AttentionPreset
{
    public required string Name { get; init; }
    public PresentationTopology? Topology { get; init; }
    public string? ForwardZoneId { get; init; }
    public IReadOnlyList<string> MfdZoneIds { get; init; } = [];
    public string? EicasPolicy { get; init; }
}
