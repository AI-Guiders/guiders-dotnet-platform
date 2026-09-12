#nullable enable

using GdlBinding = AIGuiders.Platform.Modeling.Gdl.Command.Binding;

namespace AIGuiders.Platform.IntermediateRepresentation.Binding;

/// <summary>
/// GUIDERS-FSHARP-ADR-0003 §4.3 cutover: core fields SSOT via <see cref="ToModel"/> (Modeling.Gdl.Command.Binding).
/// </summary>
public sealed class BindingDescriptor
{
    public required string BindingKey { get; init; }

    public required string GestureWire { get; init; }

    public BindingTargetKind TargetKind { get; init; } = BindingTargetKind.Command;

    public static BindingDescriptor FromFlatEntry(string bindingKey, string gestureWire) =>
        FromModel(GdlBinding.BindingDescriptorModule.fromFlatEntry(bindingKey, gestureWire));

    public string? CommandId =>
        FSharpInterop.OptString(GdlBinding.BindingDescriptorModule.commandId(ToModel()));

    public GdlBinding.BindingDescriptor ToModel() => new(
        BindingKey,
        GestureWire,
        TargetKind);

    public static BindingDescriptor FromModel(GdlBinding.BindingDescriptor model) => new()
    {
        BindingKey = model.BindingKey,
        GestureWire = model.GestureWire,
        TargetKind = model.TargetKind,
    };
}
