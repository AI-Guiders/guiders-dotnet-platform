#nullable enable

namespace AIGuiders.Platform.Notations.Keyboard.Quarry;

public static class QuarryBracketTokenParser
{
    public static bool TryParse(
        string token,
        IReadOnlyList<string> modifierPrefixes,
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
        if (token.Length >= 2 && token[0] == '<' && token[^1] == '>')
        {
            var inner = token.AsSpan(1, token.Length - 2);
            var mods = new List<string>(4);
            while (!inner.IsEmpty)
            {
                if (!QuarryModifierLexer.TryConsumePrefix(ref inner, modifierPrefixes, out var mod))
                    break;

                mods.Add(mod);
            }

            if (inner.IsEmpty)
            {
                error = "Empty key inside <…>.";
                return false;
            }

            step = new QuarryWireChordStep(mods, inner.ToString());
            return true;
        }

        step = new QuarryWirePlainStep(token);
        return true;
    }
}
