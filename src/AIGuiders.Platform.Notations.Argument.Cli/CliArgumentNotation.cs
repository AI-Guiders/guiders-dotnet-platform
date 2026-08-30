using AIGuiders.Platform.Notations.Argument;

namespace AIGuiders.Platform.Notations.Argument.Cli;

/// <summary>
/// Modern CLI tail quarry: <c>-h</c>, <c>--verbose</c>, <c>--out=file</c>, clustered <c>-abc</c>.
/// Aligned with System.CommandLine / POSIX short-flag subset (GUIDERS-ADR-0021 §11).
/// </summary>
public static class CliArgumentNotation
{
    public const string WireClassCli = "cli";

    public static NormalizedArguments Parse(string tail)
    {
        if (string.IsNullOrWhiteSpace(tail))
            return NormalizedArguments.FromRaw("", WireClassCli);

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
            ? NormalizedArguments.FromSlots(slots, WireClassCli)
            : NormalizedArguments.FromRaw(tail.Trim(), WireClassCli);
    }

    /// <summary>Schema-aware CLI parse: <c>--config release</c>, value flags, positional slots by name.</summary>
    public static NormalizedArguments ParseWithSchema(string tail, IReadOnlyList<ArgumentSlot> schema)
    {
        if (string.IsNullOrWhiteSpace(tail))
            return NormalizedArguments.FromRaw("", WireClassCli);

        var byLong = new Dictionary<string, ArgumentSlot>(StringComparer.Ordinal);
        var byShort = new Dictionary<string, ArgumentSlot>(StringComparer.Ordinal);
        var positionals = new List<ArgumentSlot>();
        foreach (var parameter in schema)
        {
            if (!string.IsNullOrWhiteSpace(parameter.LongOption))
                byLong[parameter.LongOption] = parameter;
            if (!string.IsNullOrWhiteSpace(parameter.ShortOption))
                byShort[parameter.ShortOption] = parameter;
            if (parameter.Kind == ArgumentSlotKind.Positional)
                positionals.Add(parameter);
        }

        var slots = new Dictionary<string, string>(StringComparer.Ordinal);
        var tokens = Tokenize(tail);
        var positionalIndex = 0;

        for (var i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token.StartsWith("--", StringComparison.Ordinal))
            {
                var eq = token.IndexOf('=');
                if (eq > 2 && eq < token.Length - 1)
                {
                    AssignLongOption(slots, token[..eq], token[(eq + 1)..], byLong);
                    continue;
                }

                if (byLong.TryGetValue(token, out var longParam))
                {
                    if (longParam.Kind == ArgumentSlotKind.Flag)
                    {
                        slots[longParam.Name] = "true";
                        continue;
                    }

                    if (i + 1 < tokens.Count && !IsOptionToken(tokens[i + 1]))
                    {
                        slots[longParam.Name] = tokens[++i];
                        continue;
                    }
                }

                slots[token] = "true";
                continue;
            }

            if (token.Length > 1 && token[0] == '-')
            {
                if (token.Length == 2)
                {
                    if (byShort.TryGetValue(token, out var shortParam))
                    {
                        if (shortParam.Kind == ArgumentSlotKind.Flag)
                            slots[shortParam.Name] = "true";
                        else if (i + 1 < tokens.Count && !IsOptionToken(tokens[i + 1]))
                            slots[shortParam.Name] = tokens[++i];
                    }
                    else
                    {
                        slots[token] = "true";
                    }

                    continue;
                }

                for (var c = 1; c < token.Length; c++)
                {
                    var shortToken = "-" + token[c];
                    if (byShort.TryGetValue(shortToken, out var clustered))
                        slots[clustered.Name] = "true";
                    else
                        slots[shortToken] = "true";
                }

                continue;
            }

            if (positionalIndex < positionals.Count)
            {
                slots[positionals[positionalIndex++].Name] = token;
                continue;
            }

            slots[$"@{slots.Count}"] = token;
        }

        return slots.Count > 0
            ? NormalizedArguments.FromSlots(slots, WireClassCli)
            : NormalizedArguments.FromRaw(tail.Trim(), WireClassCli);
    }

    static void AssignLongOption(
        Dictionary<string, string> slots,
        string option,
        string value,
        IReadOnlyDictionary<string, ArgumentSlot> byLong)
    {
        if (byLong.TryGetValue(option, out var parameter))
        {
            slots[parameter.Name] = parameter.Kind == ArgumentSlotKind.Flag ? "true" : value;
            return;
        }

        slots[option] = value;
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
