#nullable enable
using AIGuiders.Platform.IntermediateRepresentation.Keyboard;

namespace AIGuiders.Platform.IntermediateRepresentation.Binding;

public sealed record BindingEntry(
    BindingDescriptor Descriptor,
    NormalizedKeySequence? NormalizedGesture);
