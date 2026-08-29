#nullable enable

namespace AIGuiders.Platform.InputNotation.Quarry;

public static class QuarryModifierLexer
{
    public static bool TryConsumePrefix(ref ReadOnlySpan<char> span, IReadOnlyList<string> prefixes, out string consumed)
    {
        foreach (var candidate in prefixes)
        {
            if (!span.StartsWith(candidate, StringComparison.Ordinal))
                continue;

            consumed = candidate;
            span = span[candidate.Length..];
            return true;
        }

        consumed = "";
        return false;
    }

    public static bool TryParsePrefixes(
        ref ReadOnlySpan<char> span,
        IReadOnlyList<string> prefixes,
        out List<string> modifiers,
        out string error)
    {
        modifiers = new List<string>(4);
        error = "";

        while (!span.IsEmpty)
        {
            if (!TryConsumePrefix(ref span, prefixes, out var mod))
                break;

            modifiers.Add(mod);
        }

        if (span.IsEmpty && modifiers.Count > 0)
        {
            error = "Empty key after modifiers.";
            return false;
        }

        return true;
    }
}
