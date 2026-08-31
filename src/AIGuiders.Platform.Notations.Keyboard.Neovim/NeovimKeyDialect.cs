using AIGuiders.Platform.IntermediateRepresentation.Keyboard;
#nullable enable
using AIGuiders.Platform.Notations.Keyboard.Quarry;

namespace AIGuiders.Platform.Notations.Keyboard;

/// <summary>Neovim <c>:help key-notation</c> quarry dialect (v1 subset).</summary>
public sealed class NeovimKeyDialect : QuarryNotationDialectBase
{
    public static NeovimKeyDialect Instance { get; } = new();

    static readonly string[] Modifiers = ["Alt-", "C-", "M-", "A-", "S-", "D-"];

    public override string SurfaceId => "neovim-kbd";

    protected override IReadOnlyList<string> ModifierPrefixes => Modifiers;

    protected override bool TryParseToken(string token, out QuarryWireStep? step, out string error) =>
        QuarryBracketTokenParser.TryParse(token, ModifierPrefixes, out step, out error);

    protected override ChordModifierKeys MapModifier(string prefix) =>
        prefix switch
        {
            "C-" => ChordModifierKeys.Control,
            "M-" or "A-" or "Alt-" => ChordModifierKeys.Alt,
            "S-" => ChordModifierKeys.Shift,
            "D-" => ChordModifierKeys.Meta,
            _ => 0,
        };

    protected override string NormalizeKey(string key)
    {
        if (key.Length == 1)
            return ChordSemanticNormalizer.NormalizeKeySymbol(key);

        return key switch
        {
            "Space" or "SPC" or "spc" => "Space",
            "CR" or "Return" or "Enter" => "Return",
            "Tab" or "TAB" => "Tab",
            "Esc" or "Escape" => "Esc",
            _ => key,
        };
    }
}
