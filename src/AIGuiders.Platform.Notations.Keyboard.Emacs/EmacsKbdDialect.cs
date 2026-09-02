using AIGuiders.Platform.Modeling.Notations.Keyboard;
#nullable enable
using AIGuiders.Platform.Notations.Keyboard.Quarry;

namespace AIGuiders.Platform.Notations.Keyboard;

/// <summary>Emacs <c>kbd</c> quarry dialect (GNU key-parse v1 subset).</summary>
public sealed class EmacsKbdDialect : QuarryNotationDialectBase
{
    public static EmacsKbdDialect Instance { get; } = new();

    static readonly string[] Modifiers = ["C-", "M-", "S-", "A-", "H-", "s-"];

    public override string SurfaceId => "emacs-kbd";

    protected override IReadOnlyList<string> ModifierPrefixes => Modifiers;

    protected override bool TryParseToken(string token, out QuarryWireStep? step, out string error) =>
        QuarryHyphenTokenParser.TryParse(token, ModifierPrefixes, stripAngleBrackets: true, out step, out error);

    protected override ChordModifierKeys MapModifier(string prefix) =>
        prefix switch
        {
            "C-" => ChordModifierKeys.Control,
            "M-" or "A-" => ChordModifierKeys.Alt,
            "S-" => ChordModifierKeys.Shift,
            "s-" or "H-" => ChordModifierKeys.Meta,
            _ => 0,
        };

    protected override string NormalizeKey(string key)
    {
        if (key.Length == 1)
            return ChordSemanticNormalizer.NormalizeKeySymbol(key);

        return key.ToUpperInvariant() switch
        {
            "SPC" or "SPACE" => "SPC",
            "RET" or "RETURN" => "RET",
            "TAB" => "TAB",
            "ESC" or "ESCAPE" => "ESC",
            "DEL" or "DELETE" => "DEL",
            _ => key,
        };
    }
}
