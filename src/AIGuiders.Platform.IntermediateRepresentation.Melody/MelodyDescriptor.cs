#nullable enable

using GdlMelody = AIGuiders.Platform.Modeling.Gdl.Command.Melody;

namespace AIGuiders.Platform.IntermediateRepresentation.Melody;

/// <summary>
/// GUIDERS-FSHARP-ADR-0003 §4.3 cutover: SSOT via <see cref="ToModel"/> (Modeling.Gdl.Command.Melody).
/// </summary>
public sealed class MelodyDescriptor
{
    public required string CommandId { get; init; }
    public required string Slug { get; init; }
    public MelodyLineProfile Profile { get; init; } = MelodyLineProfile.PureByNote;
    public IReadOnlyList<MelodyStep> Steps { get; init; } = [];
    public string? Help { get; init; }

    public MelodyLine ToLine() => MelodyLine.FromModel(GdlMelody.MelodyDescriptorModule.toLine(ToModel()));

    public static MelodyDescriptor FromSlug(string commandId, string slug, string? help = null) =>
        FromModel(GdlMelody.MelodyDescriptorModule.fromSlug(commandId, slug, help));

    public GdlMelody.MelodyDescriptor ToModel() => new(
        CommandId,
        Slug,
        Profile,
        Steps,
        Help is null ? Microsoft.FSharp.Core.FSharpOption<string>.None : Microsoft.FSharp.Core.FSharpOption<string>.Some(Help));

    public static MelodyDescriptor FromModel(GdlMelody.MelodyDescriptor model) => new()
    {
        CommandId = model.CommandId,
        Slug = model.Slug,
        Profile = model.Profile,
        Steps = model.Steps,
        Help = Microsoft.FSharp.Core.FSharpOption<string>.get_IsSome(model.Help) ? model.Help.Value : null,
    };
}
