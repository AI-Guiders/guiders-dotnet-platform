#nullable enable

using AIGuiders.Platform.Modeling.Notations.Argument;
using GdlMelody = AIGuiders.Platform.Modeling.Gdl.Command.Melody;

namespace AIGuiders.Platform.IntermediateRepresentation.Melody;

/// <summary>
/// GUIDERS-FSHARP-ADR-0003 §4.3 cutover: core line SSOT via <see cref="ToModel"/>;
/// <see cref="ArgumentNotation"/> stays execution-side.
/// </summary>
public sealed class MelodyLine
{
    public required string Slug { get; init; }
    public MelodyLineProfile Profile { get; init; } = MelodyLineProfile.PureByNote;
    public IReadOnlyList<MelodyStep> Steps { get; init; } = [];
    public ArgumentNotationProfile? ArgumentNotation { get; init; }
    public string? Help { get; init; }

    public GdlMelody.MelodyLine ToModel() => new(
        Slug,
        Profile,
        Steps,
        Help is null ? Microsoft.FSharp.Core.FSharpOption<string>.None : Microsoft.FSharp.Core.FSharpOption<string>.Some(Help));

    public static MelodyLine FromModel(GdlMelody.MelodyLine model, ArgumentNotationProfile? notation = null) => new()
    {
        Slug = model.Slug,
        Profile = model.Profile,
        Steps = model.Steps,
        Help = Microsoft.FSharp.Core.FSharpOption<string>.get_IsSome(model.Help) ? model.Help.Value : null,
        ArgumentNotation = notation,
    };
}
