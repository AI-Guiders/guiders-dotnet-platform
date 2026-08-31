using AIGuiders.Platform.IntermediateRepresentation.Keyboard;
#nullable enable

namespace AIGuiders.Platform.InputNotation;

/// <summary>Legacy alias for <see cref="Notations.Keyboard.IKeyboardNotationReader"/> (GUIDERS-ADR-0021 Wave 4).</summary>
[Obsolete("Use AIGuiders.Platform.Notations.Keyboard.IKeyboardNotationReader and package AIGuiders.Platform.Notations.Keyboard.")]
public interface IInputNotationReader : Notations.Keyboard.IKeyboardNotationReader;

[Obsolete("Use AIGuiders.Platform.IntermediateRepresentation.Keyboard.ChordModifierKeys.")]
public enum ChordModifierKeys
{
    None = IntermediateRepresentation.Keyboard.ChordModifierKeys.None,
    Control = IntermediateRepresentation.Keyboard.ChordModifierKeys.Control,
    Alt = IntermediateRepresentation.Keyboard.ChordModifierKeys.Alt,
    Shift = IntermediateRepresentation.Keyboard.ChordModifierKeys.Shift,
    Meta = IntermediateRepresentation.Keyboard.ChordModifierKeys.Meta,
}

[Obsolete("Use AIGuiders.Platform.IntermediateRepresentation.Keyboard.NormalizedKeySequence.")]
public sealed record NormalizedKeySequence(IReadOnlyList<NormalizedSequenceStep> Steps)
{
    public static NormalizedKeySequence Empty { get; } = new(Array.Empty<NormalizedSequenceStep>());

    public static implicit operator IntermediateRepresentation.Keyboard.NormalizedKeySequence(NormalizedKeySequence s) =>
        new(s.Steps.Select(ConvertStep).ToList());

    public static implicit operator NormalizedKeySequence(IntermediateRepresentation.Keyboard.NormalizedKeySequence s) =>
        new(s.Steps.Select(ConvertStepBack).ToList());

    static IntermediateRepresentation.Keyboard.NormalizedSequenceStep ConvertStep(NormalizedSequenceStep step) => step switch
    {
        NormalizedChordStep c => new IntermediateRepresentation.Keyboard.NormalizedChordStep((IntermediateRepresentation.Keyboard.ChordModifierKeys)c.Modifiers, c.KeySymbol),
        NormalizedPlainKeyStep p => new IntermediateRepresentation.Keyboard.NormalizedPlainKeyStep(p.KeySymbol),
        _ => throw new InvalidOperationException("Unknown sequence step."),
    };

    static NormalizedSequenceStep ConvertStepBack(IntermediateRepresentation.Keyboard.NormalizedSequenceStep step) => step switch
    {
        IntermediateRepresentation.Keyboard.NormalizedChordStep c => new NormalizedChordStep((ChordModifierKeys)c.Modifiers, c.KeySymbol),
        IntermediateRepresentation.Keyboard.NormalizedPlainKeyStep p => new NormalizedPlainKeyStep(p.KeySymbol),
        _ => throw new InvalidOperationException("Unknown sequence step."),
    };
}

[Obsolete("Use AIGuiders.Platform.IntermediateRepresentation.Keyboard.NormalizedSequenceStep.")]
public abstract record NormalizedSequenceStep;

[Obsolete("Use AIGuiders.Platform.IntermediateRepresentation.Keyboard.NormalizedChordStep.")]
public sealed record NormalizedChordStep(ChordModifierKeys Modifiers, string KeySymbol) : NormalizedSequenceStep;

[Obsolete("Use AIGuiders.Platform.IntermediateRepresentation.Keyboard.NormalizedPlainKeyStep.")]
public sealed record NormalizedPlainKeyStep(string KeySymbol) : NormalizedSequenceStep;

[Obsolete("Use AIGuiders.Platform.Notations.Keyboard.ChordSemanticNormalizer.")]
public static class ChordSemanticNormalizer
{
    public static string NormalizeKeySymbol(string key) =>
        Notations.Keyboard.ChordSemanticNormalizer.NormalizeKeySymbol(key);
}
