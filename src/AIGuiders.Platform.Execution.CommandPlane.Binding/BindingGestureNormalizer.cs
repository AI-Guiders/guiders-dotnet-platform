using AIGuiders.Platform.Modeling.Notations.Keyboard;
#nullable enable
using AIGuiders.Platform.Notations.Keyboard;

namespace AIGuiders.Platform.Execution.CommandPlane.Binding;

public static class BindingGestureNormalizer
{
    public static bool TryNormalizeWire(string? gestureWire, out NormalizedKeySequence? sequence, out string error) =>
        KeyGestureChordSyntax.TryParseToNormalized(gestureWire, out sequence, out error);
}
