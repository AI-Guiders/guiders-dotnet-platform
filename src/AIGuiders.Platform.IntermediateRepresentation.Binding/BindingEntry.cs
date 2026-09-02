#nullable enable
using AIGuiders.Platform.Modeling.Notations.Keyboard;

namespace AIGuiders.Platform.IntermediateRepresentation.Binding;

public sealed record BindingEntry(
    BindingDescriptor Descriptor,
    NormalizedKeySequence? NormalizedGesture);
