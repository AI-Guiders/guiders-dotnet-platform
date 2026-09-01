using AIGuiders.Platform.Authoring.Core;
using AIGuiders.Platform.Notations.Keyboard;

namespace AIGuiders.Platform.Authoring.Command.Catalog;

public static class CatalogNotationValidator
{
    public static void Validate(CatalogDocument document, List<AuthoringDiagnostic> diagnostics)
    {
        if (document.Bindings.Count > 0 && string.IsNullOrWhiteSpace(document.Defaults.NotationKeyboardBinding))
        {
            diagnostics.Add(new(
                AuthoringDiagnosticCode.MissingNotationDeclaration,
                "Section `bindings` requires `notation.keyboard.binding` in defaults.",
                1,
                Section: "defaults"));
        }

        if (document.Melodies.Count > 0 && string.IsNullOrWhiteSpace(document.Defaults.NotationKeyboardMelody))
        {
            diagnostics.Add(new(
                AuthoringDiagnosticCode.MissingNotationDeclaration,
                "Section `melodies` requires `notation.keyboard.melody` in defaults.",
                1,
                Section: "defaults"));
        }

        var bindingNotation = document.Defaults.NotationKeyboardBinding;
        if (!string.IsNullOrWhiteSpace(bindingNotation))
        {
            if (!string.IsNullOrWhiteSpace(document.Defaults.BindingChordRoot)
                && !TryParseKeyboard(bindingNotation, document.Defaults.BindingChordRoot, out _))
            {
                diagnostics.Add(Mismatch(1, "binding.chord-root", bindingNotation, document.Defaults.BindingChordRoot));
            }

            for (var i = 0; i < document.Bindings.Count; i++)
            {
                var row = document.Bindings[i];
                if (!TryParseKeyboard(bindingNotation, row.Gesture, out var looksLike))
                {
                    diagnostics.Add(Mismatch(i + 1, $"bindings row {i + 1}", bindingNotation, row.Gesture, looksLike));
                }
            }
        }

        var melodyNotation = document.Defaults.NotationKeyboardMelody;
        if (!string.IsNullOrWhiteSpace(melodyNotation))
        {
            for (var i = 0; i < document.Melodies.Count; i++)
            {
                var row = document.Melodies[i];
                if (!TryParseMelodySlug(melodyNotation, row.Slug, out var looksLike))
                {
                    diagnostics.Add(Mismatch(i + 1, $"melodies row {i + 1}", melodyNotation, row.Slug, looksLike));
                }
            }
        }
    }

    static bool TryParseMelodySlug(string notationId, string wire, out string? looksLike)
    {
        looksLike = null;
        if (string.IsNullOrWhiteSpace(wire))
        {
            return false;
        }

        if (notationId.Equals("keyboard-key-gesture", StringComparison.OrdinalIgnoreCase)
            && wire.All(static ch => char.IsLetterOrDigit(ch) || ch is '_' or '-'))
        {
            return true;
        }

        return TryParseKeyboard(notationId, wire, out looksLike);
    }

    static bool TryParseKeyboard(string notationId, string wire, out string? looksLike)
    {
        looksLike = null;
        if (string.IsNullOrWhiteSpace(wire) || wire == "—" || wire == "-")
        {
            return true;
        }

        if (notationId.Equals("keyboard-vim", StringComparison.OrdinalIgnoreCase)
            || notationId.Equals("keyboard-neovim", StringComparison.OrdinalIgnoreCase))
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

        if (notationId.Equals("keyboard-key-gesture", StringComparison.OrdinalIgnoreCase))
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
            AuthoringDiagnosticCode.NotationWireMismatch,
            looksLike is null
                ? $"notation-wire-mismatch: {where} — declared {declared}, unparsable cell '{cell}'."
                : $"notation-wire-mismatch: {where} — declared {declared}, cell looks like {looksLike} ('{cell}').",
            line,
            Section: where);
}
