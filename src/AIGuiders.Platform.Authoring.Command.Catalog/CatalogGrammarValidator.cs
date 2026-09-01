using AIGuiders.Platform.Authoring.Core;
using AIGuiders.Platform.Notations.Keyboard;

namespace AIGuiders.Platform.Authoring.Command.Catalog;

public static class CatalogGrammarValidator
{
    public static void Validate(CatalogDocument document, List<AuthoringDiagnostic> diagnostics)
    {
        if (document.Bindings.Count > 0 && string.IsNullOrWhiteSpace(document.Defaults.GrammarKeyboardBinding))
        {
            diagnostics.Add(new(
                AuthoringDiagnosticCode.MissingGrammarDeclaration,
                "Section `bindings` requires `grammar.keyboard.binding` in defaults.",
                1,
                Section: "defaults"));
        }

        if (document.Melodies.Count > 0 && string.IsNullOrWhiteSpace(document.Defaults.GrammarKeyboardMelody))
        {
            diagnostics.Add(new(
                AuthoringDiagnosticCode.MissingGrammarDeclaration,
                "Section `melodies` requires `grammar.keyboard.melody` in defaults.",
                1,
                Section: "defaults"));
        }

        var bindingGrammar = document.Defaults.GrammarKeyboardBinding;
        if (!string.IsNullOrWhiteSpace(bindingGrammar))
        {
            if (!string.IsNullOrWhiteSpace(document.Defaults.BindingChordRoot)
                && !TryParseKeyboard(bindingGrammar, document.Defaults.BindingChordRoot, out _))
            {
                diagnostics.Add(Mismatch(1, "binding.chord-root", bindingGrammar, document.Defaults.BindingChordRoot));
            }

            for (var i = 0; i < document.Bindings.Count; i++)
            {
                var row = document.Bindings[i];
                if (!TryParseKeyboard(bindingGrammar, row.Gesture, out var looksLike))
                {
                    diagnostics.Add(Mismatch(i + 1, $"bindings row {i + 1}", bindingGrammar, row.Gesture, looksLike));
                }
            }
        }

        var melodyGrammar = document.Defaults.GrammarKeyboardMelody;
        if (!string.IsNullOrWhiteSpace(melodyGrammar))
        {
            for (var i = 0; i < document.Melodies.Count; i++)
            {
                var row = document.Melodies[i];
                if (!TryParseMelodySlug(melodyGrammar, row.Slug, out var looksLike))
                {
                    diagnostics.Add(Mismatch(i + 1, $"melodies row {i + 1}", melodyGrammar, row.Slug, looksLike));
                }
            }
        }

        NotationGrammarRegistry.ValidateDocument(document, diagnostics);
    }

    static bool TryParseMelodySlug(string grammarId, string wire, out string? looksLike)
    {
        looksLike = null;
        if (string.IsNullOrWhiteSpace(wire))
        {
            return false;
        }

        if (grammarId.Equals("keyboard-key-gesture", StringComparison.OrdinalIgnoreCase)
            && wire.All(static ch => char.IsLetterOrDigit(ch) || ch is '_' or '-'))
        {
            return true;
        }

        return TryParseKeyboard(grammarId, wire, out looksLike);
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

    static AuthoringDiagnostic Mismatch(int line, string where, string declared, string cell, string? looksLike = null) =>
        new(
            AuthoringDiagnosticCode.GrammarWireMismatch,
            looksLike is null
                ? $"grammar-wire-mismatch: {where} — declared {declared}, unparsable cell '{cell}'."
                : $"grammar-wire-mismatch: {where} — declared {declared}, cell looks like {looksLike} ('{cell}').",
            line,
            Section: where);
}
