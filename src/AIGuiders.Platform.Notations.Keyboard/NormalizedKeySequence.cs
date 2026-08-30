#nullable enable

namespace AIGuiders.Platform.Notations.Keyboard;

[Flags]
public enum ChordModifierKeys
{
    None = 0,
    Control = 1,
    Alt = 2,
    Shift = 4,
    Meta = 8,
}

public sealed record NormalizedKeySequence(IReadOnlyList<NormalizedSequenceStep> Steps)
{
    public static NormalizedKeySequence Empty { get; } = new(Array.Empty<NormalizedSequenceStep>());
}

public abstract record NormalizedSequenceStep;

public sealed record NormalizedChordStep(ChordModifierKeys Modifiers, string KeySymbol) : NormalizedSequenceStep;

public sealed record NormalizedPlainKeyStep(string KeySymbol) : NormalizedSequenceStep;
