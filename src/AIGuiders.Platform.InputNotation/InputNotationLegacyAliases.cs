#nullable enable

namespace AIGuiders.Platform.InputNotation;

/// <summary>Legacy alias for <see cref="Notations.Keyboard.IKeyboardNotationReader"/> (GUIDERS-ADR-0021 Wave 4).</summary>
[Obsolete("Use AIGuiders.Platform.Notations.Keyboard.IKeyboardNotationReader and package AIGuiders.Platform.Notations.Keyboard.")]
public interface IInputNotationReader : Notations.Keyboard.IKeyboardNotationReader;

[Obsolete("Use AIGuiders.Platform.Notations.Keyboard.ChordModifierKeys.")]
public enum ChordModifierKeys
{
    None = Notations.Keyboard.ChordModifierKeys.None,
    Control = Notations.Keyboard.ChordModifierKeys.Control,
    Alt = Notations.Keyboard.ChordModifierKeys.Alt,
    Shift = Notations.Keyboard.ChordModifierKeys.Shift,
    Meta = Notations.Keyboard.ChordModifierKeys.Meta,
}

[Obsolete("Use AIGuiders.Platform.Notations.Keyboard.NormalizedKeySequence.")]
public sealed record NormalizedKeySequence(IReadOnlyList<NormalizedSequenceStep> Steps)
{
    public static NormalizedKeySequence Empty { get; } = new(Array.Empty<NormalizedSequenceStep>());

    public static implicit operator Notations.Keyboard.NormalizedKeySequence(NormalizedKeySequence s) =>
        new(s.Steps.Select(ConvertStep).ToList());

    public static implicit operator NormalizedKeySequence(Notations.Keyboard.NormalizedKeySequence s) =>
        new(s.Steps.Select(ConvertStepBack).ToList());

    static Notations.Keyboard.NormalizedSequenceStep ConvertStep(NormalizedSequenceStep step) => step switch
    {
        NormalizedChordStep c => new Notations.Keyboard.NormalizedChordStep((Notations.Keyboard.ChordModifierKeys)c.Modifiers, c.KeySymbol),
        NormalizedPlainKeyStep p => new Notations.Keyboard.NormalizedPlainKeyStep(p.KeySymbol),
        _ => throw new InvalidOperationException("Unknown sequence step."),
    };

    static NormalizedSequenceStep ConvertStepBack(Notations.Keyboard.NormalizedSequenceStep step) => step switch
    {
        Notations.Keyboard.NormalizedChordStep c => new NormalizedChordStep((ChordModifierKeys)c.Modifiers, c.KeySymbol),
        Notations.Keyboard.NormalizedPlainKeyStep p => new NormalizedPlainKeyStep(p.KeySymbol),
        _ => throw new InvalidOperationException("Unknown sequence step."),
    };
}

[Obsolete("Use AIGuiders.Platform.Notations.Keyboard.NormalizedSequenceStep.")]
public abstract record NormalizedSequenceStep;

[Obsolete("Use AIGuiders.Platform.Notations.Keyboard.NormalizedChordStep.")]
public sealed record NormalizedChordStep(ChordModifierKeys Modifiers, string KeySymbol) : NormalizedSequenceStep;

[Obsolete("Use AIGuiders.Platform.Notations.Keyboard.NormalizedPlainKeyStep.")]
public sealed record NormalizedPlainKeyStep(string KeySymbol) : NormalizedSequenceStep;

[Obsolete("Use AIGuiders.Platform.Notations.Keyboard.ChordSemanticNormalizer.")]
public static class ChordSemanticNormalizer
{
    public static string NormalizeKeySymbol(string key) =>
        Notations.Keyboard.ChordSemanticNormalizer.NormalizeKeySymbol(key);
}
