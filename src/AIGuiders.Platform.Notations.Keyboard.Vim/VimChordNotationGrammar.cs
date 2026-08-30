#nullable enable
using Eto.Parse;

namespace AIGuiders.Platform.Notations.Keyboard;

internal static class VimChordNotationGrammar
{
    static readonly Lazy<Grammar> Lazy = new(Build);

    public static Grammar Instance => Lazy.Value;

    static Grammar Build()
    {
        Parser mod(string s) => s;
        var modifier =
            mod("Alt-") | mod("C-") | mod("M-") | mod("A-") | mod("S-") | mod("D-");

        var key = (+Terminals.LetterOrDigit).Named("key");
        var bracketInner = modifier.Repeat(0) & key;
        var bracket = ("<" & bracketInner & ">").Named("bracket");
        var plain = (+Terminals.LetterOrDigit).Named("plain");
        var step = (bracket | plain).Named("step");
        var sp = +Terminals.WhiteSpace;
        var sequence = Terminals.WhiteSpace.Repeat(0) & step & (sp & step).Repeat(0) & Terminals.WhiteSpace.Repeat(0) & Terminals.End;

        return new Grammar("chord_sequence", sequence);
    }
}
