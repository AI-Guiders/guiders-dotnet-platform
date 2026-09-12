#nullable enable

using AIGuiders.Platform.Modeling.Notations.Keyboard;
using GdlBinding = AIGuiders.Platform.Modeling.Gdl.Command.Binding;

namespace AIGuiders.Platform.IntermediateRepresentation.Binding;

/// <summary>
/// GUIDERS-FSHARP-ADR-0003 §4.3 cutover: entry SSOT via <see cref="ToModel"/> (Modeling.Gdl.Command.Binding).
/// </summary>
public sealed record BindingEntry(
    BindingDescriptor Descriptor,
    NormalizedKeySequence? NormalizedGesture)
{
    public GdlBinding.BindingEntry ToModel() => new(
        Descriptor.ToModel(),
        FSharpInterop.OptGesture(NormalizedGesture));

    public static BindingEntry FromModel(GdlBinding.BindingEntry model) => new(
        BindingDescriptor.FromModel(model.Descriptor),
        FSharpInterop.OptGesture(model.NormalizedGesture));
}
