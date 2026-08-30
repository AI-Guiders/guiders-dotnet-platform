using AIGuiders.Platform.Notations.Argument.Kv;

namespace AIGuiders.Platform.Notations.Command.Console;

public static class ConsoleCommandNotation
{
    /// <summary>
    /// Splits <paramref name="line"/> into path tokens (before first kv token) and kv tail.
    /// Example: <c>buffer open doc=README.md</c> → path <c>buffer open</c>, slot <c>doc=README.md</c>.
    /// </summary>
    public static bool TryParse(string line, out SlashWireBody pathWire, out NormalizedArgTail argTail)
    {
        pathWire = new SlashWireBody([], false);
        argTail = NormalizedArgTail.FromRaw("");

        if (string.IsNullOrWhiteSpace(line))
            return false;

        var endsWithSpace = line.EndsWith(' ');
        var tokens = line.TrimEnd().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        if (tokens.Count == 0)
            return false;

        var pathTokens = new List<string>();
        var kvStart = tokens.Count;
        for (var i = 0; i < tokens.Count; i++)
        {
            if (IsKvToken(tokens[i]))
            {
                kvStart = i;
                break;
            }

            pathTokens.Add(tokens[i]);
        }

        if (pathTokens.Count == 0)
            return false;

        pathWire = new SlashWireBody(pathTokens, endsWithSpace && kvStart >= tokens.Count);
        var kvTail = kvStart < tokens.Count
            ? string.Join(' ', tokens.Skip(kvStart))
            : "";
        argTail = KvArgumentNotation.Parse(kvTail);
        return true;
    }

    private static bool IsKvToken(string token)
    {
        var eq = token.IndexOf('=');
        return eq > 0 && eq < token.Length - 1;
    }
}
