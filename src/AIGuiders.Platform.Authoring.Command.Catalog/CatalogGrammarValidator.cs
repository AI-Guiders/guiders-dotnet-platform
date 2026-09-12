using AIGuiders.Platform.Authoring.Core;
using AIGuiders.Platform.Notations.Keyboard;
using GdlParseCatalog = AIGuiders.Platform.Modeling.Gdl.Parse.Catalog;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Authoring.Command.Catalog;

/// <summary>
/// GUIDERS-FSHARP-ADR-0003 §4.5 cutover: wire grammar rules SSOT via Modeling.Gdl.Parse.Catalog;
/// keyboard wire parsing stays execution-side (Notations.Keyboard wrappers).
/// </summary>
public static class CatalogGrammarValidator
{
    public static void Validate(CatalogDocument document, List<AuthoringDiagnostic> diagnostics)
    {
        var keyboardParse =
            FSharpFunc<string, FSharpFunc<string, Tuple<bool, FSharpOption<string>>>>.FromConverter(
                grammarId => FSharpFunc<string, Tuple<bool, FSharpOption<string>>>.FromConverter(
                    wire =>
                    {
                        if (TryParseKeyboard(grammarId, wire, out var looksLike))
                        {
                            return Tuple.Create(true, FSharpInterop.OptString(looksLike));
                        }

                        return Tuple.Create(false, FSharpInterop.OptString(looksLike));
                    }));

        foreach (var diagnostic in GdlParseCatalog.CatalogGrammarValidator.validate(
                     FSharpOption<FSharpFunc<string, FSharpFunc<string, Tuple<bool, FSharpOption<string>>>>>.Some(
                         keyboardParse),
                     document.ToModel()))
        {
            diagnostics.Add(CatalogInterop.FromDiagnostic(diagnostic));
        }

        NotationGrammarRegistry.ValidateDocument(document, diagnostics);
    }

    static bool TryParseKeyboard(string grammarId, string wire, out string? looksLike)
    {
        looksLike = null;
        if (string.IsNullOrWhiteSpace(wire) || wire == "—" || wire == "-")
        {
            return true;
        }

        if (grammarId.Equals("keyboard-vim", StringComparison.OrdinalIgnoreCase)
            || grammarId.Equals("keyboard-neovim", StringComparison.OrdinalIgnoreCase))
        {
            if (VimChordNotationParser.TryParseToNormalized(wire, out _, out _))
            {
                return true;
            }

            if (KeyGestureChordSyntax.TryParseToNormalized(wire, out _, out _))
            {
                looksLike = "KeyGesture";
            }

            return false;
        }

        if (grammarId.Equals("keyboard-key-gesture", StringComparison.OrdinalIgnoreCase))
        {
            if (KeyGestureChordSyntax.TryParseToNormalized(wire, out _, out _))
            {
                return true;
            }

            if (wire.StartsWith('<') || wire.Contains("C-", StringComparison.Ordinal))
            {
                looksLike = "Vim";
            }

            return false;
        }

        return true;
    }
}
