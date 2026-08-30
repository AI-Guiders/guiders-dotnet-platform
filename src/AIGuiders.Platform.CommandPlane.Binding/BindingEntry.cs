#nullable enable
using AIGuiders.Platform.Notations.Keyboard;

namespace AIGuiders.Platform.CommandPlane.Binding;

public sealed record BindingEntry(
    BindingDescriptor Descriptor,
    NormalizedKeySequence? NormalizedGesture);
