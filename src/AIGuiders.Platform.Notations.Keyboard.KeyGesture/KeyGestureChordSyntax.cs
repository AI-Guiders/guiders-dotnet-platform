using AIGuiders.Platform.Modeling.Notations.Keyboard;
#nullable enable
using System.Diagnostics.CodeAnalysis;

namespace AIGuiders.Platform.Notations.Keyboard;

/// <summary>KeyGesture / hotkeys.toml wire syntax (quarry from CIDE).</summary>
public static class KeyGestureChordSyntax
{
    const char CommandKeySymbol = '\u2318';

    public static bool TryParseToNormalized(string? input, out NormalizedKeySequence? sequence, out string error)
    {
        sequence = null;
        error = "";

        if (string.IsNullOrEmpty(input))
        {
            sequence = NormalizedKeySequence.Empty;
            return true;
        }

        var trimmed = CollapseSpacesAroundPlus(input.Trim());
        var tokens = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var steps = new List<NormalizedSequenceStep>(tokens.Length);

        foreach (var raw in tokens)
        {
            if (!TryParseOneToken(raw, out var step, out var err))
            {
                error = err;
                return false;
            }

            steps.Add(step);
        }

        sequence = new NormalizedKeySequence(steps);
        return true;
    }

    static string CollapseSpacesAroundPlus(string input)
    {
        if (!input.Contains('+', StringComparison.Ordinal))
            return input;

        var parts = input.Split('+');
        for (var i = 0; i < parts.Length; i++)
            parts[i] = parts[i].Trim();

        return string.Join("+", parts);
    }

    static bool TryParseOneToken(string token, [NotNullWhen(true)] out NormalizedSequenceStep? step, out string error)
    {
        error = "";
        step = null;
        token = token.Trim();
        if (token.Length == 0)
        {
            error = "Empty token.";
            return false;
        }

        if (token.Length >= 2 && token[0] == CommandKeySymbol)
        {
            var rest = token[1..];
            step = new NormalizedChordStep(ChordModifierKeys.Meta, ChordSemanticNormalizer.NormalizeKeySymbol(rest));
            return true;
        }

        if (!token.Contains('+', StringComparison.Ordinal))
        {
            step = new NormalizedPlainKeyStep(ChordSemanticNormalizer.NormalizeKeySymbol(token));
            return true;
        }

        var parts = token.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            error = $"Expected Mod+Key, got: \"{token}\".";
            return false;
        }

        ChordModifierKeys mods = 0;
        for (var i = 0; i < parts.Length - 1; i++)
        {
            var mk = MapModifierWord(parts[i]);
            if (mk == null)
            {
                error = $"Unknown modifier: \"{parts[i]}\".";
                return false;
            }

            mods |= mk.Value;
        }

        var key = ChordSemanticNormalizer.NormalizeKeySymbol(parts[^1]);
        step = new NormalizedChordStep(mods, key);
        return true;
    }

    static ChordModifierKeys? MapModifierWord(string word)
    {
        if (word.Length == 1 && word[0] == CommandKeySymbol)
            return ChordModifierKeys.Meta;

        var w = word.Trim();
        if (w.Length == 0)
            return null;

        return w.ToUpperInvariant() switch
        {
            "CTRL" or "CONTROL" => ChordModifierKeys.Control,
            "ALT" or "OPTION" => ChordModifierKeys.Alt,
            "SHIFT" => ChordModifierKeys.Shift,
            "META" or "CMD" or "COMMAND" or "WIN" or "SUPER" => ChordModifierKeys.Meta,
            _ => null,
        };
    }
}
