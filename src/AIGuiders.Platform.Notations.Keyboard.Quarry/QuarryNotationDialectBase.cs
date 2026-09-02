using AIGuiders.Platform.Modeling.Notations.Keyboard;
#nullable enable

namespace AIGuiders.Platform.Notations.Keyboard.Quarry;

/// <summary>Shared quarry dialect: whitespace wire → <see cref="QuarryWireStep"/> → <see cref="NormalizedKeySequence"/>.</summary>
public abstract class QuarryNotationDialectBase : IKeyboardNotationReader
{
    public abstract string SurfaceId { get; }

    protected abstract IReadOnlyList<string> ModifierPrefixes { get; }

    protected abstract bool TryParseToken(string token, out QuarryWireStep? step, out string error);

    protected abstract ChordModifierKeys MapModifier(string prefix);

    protected abstract string NormalizeKey(string key);

    public QuarryParseResult Parse(string? wire) =>
        QuarrySequenceParser.Parse(wire, TryParseToken);

    public bool TryParseToNormalized(string? wire, out NormalizedKeySequence? sequence, out string error)
    {
        sequence = null;
        var r = Parse(wire);
        if (!r.IsSuccess)
        {
            error = r.Error;
            return false;
        }

        sequence = QuarryWireNormalizer.ToNormalized(r.Steps, MapModifier, NormalizeKey);
        error = "";
        return true;
    }
}
