using AIGuiders.Platform.Authoring.Core;

namespace AIGuiders.Platform.Authoring.Deck;

public sealed class DeckParseResult
{
    public DeckDocument? Document { get; init; }
    public IReadOnlyList<AuthoringDiagnostic> Diagnostics { get; init; } = [];
}
