#nullable enable

using AIGuiders.Platform.Modeling.Notations.Argument;

namespace AIGuiders.Platform.IntermediateRepresentation.Melody;

/// <summary>
/// Catalog projection for melody mechanic — keyboard line after chord root (GUIDERS-ADR-0015).
/// Palette <c>c:</c> discoverability reuses slug/Help; it is not this descriptor's execution surface.
/// </summary>
public sealed class MelodyDescriptor
{
    public required string CommandId { get; init; }

    public required string Slug { get; init; }

    public MelodyLineProfile Profile { get; init; } = MelodyLineProfile.PureByNote;

    public IReadOnlyList<MelodyStep> Steps { get; init; } = [];

    public ArgumentNotationProfile? ArgumentNotation { get; init; }

    public string? Help { get; init; }

    public MelodyLine ToLine() => new()
    {
        Slug = Slug,
        Profile = Profile,
        Steps = Steps,
        ArgumentNotation = ArgumentNotation,
        Help = Help,
    };

    public static MelodyDescriptor FromSlug(string commandId, string slug, string? help = null) => new()
    {
        CommandId = commandId,
        Slug = slug,
        Profile = MelodyLineProfile.PureByNote,
        Help = help,
    };
}
