using AIGuiders.Platform.Modeling.Notations.Keyboard;
#nullable enable
using Eto.Parse;

namespace AIGuiders.Platform.Notations.Keyboard;

public abstract record VimNotationStep;

public sealed record VimNotationChordStep(IReadOnlyList<string> ModifierPrefixes, string Key) : VimNotationStep;

public sealed record VimNotationPlainStep(string Token) : VimNotationStep;

public readonly record struct VimNotationParseResult(bool IsSuccess, IReadOnlyList<VimNotationStep> Steps, string Error)
{
    public static VimNotationParseResult Ok(IReadOnlyList<VimNotationStep> steps) =>
        new(true, steps, "");

    public static VimNotationParseResult Fail(string message) =>
        new(false, Array.Empty<VimNotationStep>(), message);
}

/// <summary>Vim-style chord sequence parser (quarry from CIDE <c>ChordNotationParser</c>).</summary>
public static class VimChordNotationParser
{
    public static VimNotationParseResult Parse(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return VimNotationParseResult.Ok(Array.Empty<VimNotationStep>());

        var trimmed = input.Trim();
        var gm = VimChordNotationGrammar.Instance.Match(trimmed);
        if (!gm.Success)
            return VimNotationParseResult.Fail(FormatGrammarError(gm, trimmed));

        var tokens = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var steps = new List<VimNotationStep>(tokens.Length);
        foreach (var t in tokens)
        {
            if (t.Length >= 2 && t[0] == '<' && t[^1] == '>')
            {
                steps.Add(ParseBracketInner(t.AsSpan(1, t.Length - 2)));
                continue;
            }

            steps.Add(new VimNotationPlainStep(t));
        }

        return VimNotationParseResult.Ok(steps);
    }

    public static bool TryParseToNormalized(string? input, out NormalizedKeySequence? sequence, out string error)
    {
        sequence = null;
        var r = Parse(input);
        if (!r.IsSuccess)
        {
            error = r.Error;
            return false;
        }

        sequence = VimNotationNormalizer.FromVimSteps(r.Steps);
        error = "";
        return true;
    }

    static VimNotationChordStep ParseBracketInner(ReadOnlySpan<char> inner)
    {
        var mods = new List<string>(4);
        while (!inner.IsEmpty)
        {
            if (inner.StartsWith("Alt-", StringComparison.Ordinal))
            {
                mods.Add("Alt-");
                inner = inner[4..];
                continue;
            }

            if (inner.StartsWith("C-", StringComparison.Ordinal))
            {
                mods.Add("C-");
                inner = inner[2..];
                continue;
            }

            if (inner.StartsWith("M-", StringComparison.Ordinal))
            {
                mods.Add("M-");
                inner = inner[2..];
                continue;
            }

            if (inner.StartsWith("A-", StringComparison.Ordinal))
            {
                mods.Add("A-");
                inner = inner[2..];
                continue;
            }

            if (inner.StartsWith("S-", StringComparison.Ordinal))
            {
                mods.Add("S-");
                inner = inner[2..];
                continue;
            }

            if (inner.StartsWith("D-", StringComparison.Ordinal))
            {
                mods.Add("D-");
                inner = inner[2..];
                continue;
            }

            break;
        }

        var key = inner.ToString();
        if (key.Length == 0)
            throw new InvalidOperationException("Chord notation: empty key inside <…>.");

        return new VimNotationChordStep(mods, key);
    }

    static string FormatGrammarError(GrammarMatch gm, string text)
    {
        var idx = gm.ErrorIndex >= 0 ? gm.ErrorIndex : 0;
        var tail = idx < text.Length ? text[idx..] : "";
        tail = tail.Length > 24 ? tail[..24] + "…" : tail;
        return $"Chord notation: parse stopped at position {idx} (remainder: \"{tail}\").";
    }
}
