#nullable enable

namespace AIGuiders.Platform.CommandPlane.Melody;

/// <summary>Sequential play line for one melody alias (slug + steps + profile).</summary>
public sealed class MelodyLine
{
    public required string Slug { get; init; }

    public MelodyLineProfile Profile { get; init; } = MelodyLineProfile.PureByNote;

    public IReadOnlyList<MelodyStep> Steps { get; init; } = [];

    /// <summary>Parametric tail wire class after slug resolves (planet SSOT).</summary>
    public string? TailWireClass { get; init; }

    public string? Help { get; init; }
}
