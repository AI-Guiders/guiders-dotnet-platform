#nullable enable

using AIGuiders.Platform.IntermediateRepresentation.Argument;

namespace AIGuiders.Platform.IntermediateRepresentation.Melody;

/// <summary>Sequential play line for one melody alias (slug + steps + profile).</summary>
public sealed class MelodyLine
{
    public required string Slug { get; init; }

    public MelodyLineProfile Profile { get; init; } = MelodyLineProfile.PureByNote;

    public IReadOnlyList<MelodyStep> Steps { get; init; } = [];

    /// <summary>Parametric argument notation after slug resolves.</summary>
    public ArgumentNotationProfile? ArgumentNotation { get; init; }

    public string? Help { get; init; }
}
