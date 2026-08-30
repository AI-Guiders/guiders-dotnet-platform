namespace AIGuiders.Platform.Notations.Argument.Cli;

/// <summary>
/// Modern CLI tail quarry: <c>-h</c>, <c>--verbose</c>, <c>--out=file</c>, clustered <c>-abc</c>.
/// Aligned with System.CommandLine / POSIX short-flag subset (GUIDERS-ADR-0021 §11).
/// </summary>
public static class CliArgumentNotation
{
    public const string WireClassCli = "cli";

    public static NormalizedArgTail Parse(string tail)
    {
        if (string.IsNullOrWhiteSpace(tail))
            return NormalizedArgTail.FromRaw("", WireClassCli);

        var slots = new Dictionary<string, string>(StringComparer.Ordinal);
        var tokens = Tokenize(tail);
        for (var i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token.StartsWith("--", StringComparison.Ordinal))
            {
                var eq = token.IndexOf('=');
                if (eq > 2 && eq < token.Length - 1)
                {
                    slots[token[..eq]] = token[(eq + 1)..];
                    continue;
                }

                slots[token] = "true";
                continue;
            }

            if (token.Length > 1 && token[0] == '-')
            {
                if (token.Length == 2)
                {
                    slots[token] = "true";
                    continue;
                }

                for (var c = 1; c < token.Length; c++)
                    slots["-" + token[c]] = "true";
                continue;
            }

            slots[$"@{slots.Count}"] = token;
        }

        return slots.Count > 0
            ? NormalizedArgTail.FromSlots(slots, WireClassCli)
            : NormalizedArgTail.FromRaw(tail.Trim(), WireClassCli);
    }

    static bool IsOptionToken(string token) =>
        token.Length > 0 && token[0] == '-';

    static List<string> Tokenize(string tail)
    {
        var tokens = new List<string>();
        var i = 0;
        while (i < tail.Length)
        {
            while (i < tail.Length && char.IsWhiteSpace(tail[i]))
                i++;

            if (i >= tail.Length)
                break;

            if (tail[i] == '"')
            {
                i++;
                var start = i;
                while (i < tail.Length && tail[i] != '"')
                    i++;
                tokens.Add(tail[start..i]);
                if (i < tail.Length)
                    i++;
                continue;
            }

            var wordStart = i;
            while (i < tail.Length && !char.IsWhiteSpace(tail[i]))
                i++;
            tokens.Add(tail[wordStart..i]);
        }

        return tokens;
    }
}
