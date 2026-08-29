#nullable enable

namespace AIGuiders.Platform.CommandPlane.Binding;

public sealed record BindingDescriptor
{
    public required string BindingKey { get; init; }

    public required string GestureWire { get; init; }

    public BindingTargetKind TargetKind { get; init; } = BindingTargetKind.Command;

    public static BindingDescriptor FromFlatEntry(string bindingKey, string gestureWire)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bindingKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(gestureWire);

        var kind = string.Equals(bindingKey, BindingWellKnownKeys.CascadeChord, StringComparison.OrdinalIgnoreCase)
            ? BindingTargetKind.ChordRoot
            : BindingTargetKind.Command;

        return new BindingDescriptor
        {
            BindingKey = bindingKey.Trim(),
            GestureWire = gestureWire.Trim(),
            TargetKind = kind,
        };
    }

    public string? CommandId =>
        TargetKind == BindingTargetKind.Command ? BindingKey : null;
}
