#nullable enable

namespace AIGuiders.Platform.Notations.Keyboard.Quarry;

public static class QuarryHyphenTokenParser
{
    public static bool TryParse(
        string token,
        IReadOnlyList<string> modifierPrefixes,
        bool stripAngleBrackets,
        out QuarryWireStep? step,
        out string error)
    {
        step = null;
        error = "";

        if (string.IsNullOrWhiteSpace(token))
        {
            error = "Empty token.";
            return false;
        }

        token = token.Trim();
        if (stripAngleBrackets && token.Length >= 2 && token[0] == '<' && token[^1] == '>')
            token = token[1..^1];

        var span = token.AsSpan();
        if (!QuarryModifierLexer.TryParsePrefixes(ref span, modifierPrefixes, out var mods, out error))
            return false;

        if (span.IsEmpty)
        {
            error = "Empty key after modifiers.";
            return false;
        }

        var key = span.ToString();
        step = mods.Count > 0
            ? new QuarryWireChordStep(mods, key)
            : new QuarryWirePlainStep(key);
        return true;
    }
}
