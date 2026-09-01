using AIGuiders.Platform.IntermediateRepresentation.Argument;
using AIGuiders.Platform.Notations.Argument;
using AIGuiders.Platform.Notations.Argument.Kv;

using AIGuiders.Platform.Notations.Command;



namespace AIGuiders.Platform.Notations.Command.Console;



public static class ConsoleCommandNotation

{

    /// <summary>

    /// Splits <paramref name="line"/> into path tokens (before first kv token) and kv tail.

    /// Example: <c>buffer open doc=README.md</c> → path <c>buffer open</c>, slot <c>doc=README.md</c>.

    /// </summary>

    public static bool TryParse(string line, out SlashWireBody pathWire, out NormalizedArguments args)

    {

        pathWire = new SlashWireBody([], false);

        args = NormalizedArguments.FromRaw("");



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

        args = KvArgumentNotation.Parse(kvTail);

        return true;

    }



    /// <summary>Path/tail split + profile-driven argument parse (kv default path split).</summary>

    public static bool TryParse(string line, ArgumentNotationProfile? profile, out SlashWireBody pathWire, out NormalizedArguments args)

    {

        pathWire = new SlashWireBody([], false);

        args = NormalizedArguments.FromRaw("");



        if (string.IsNullOrWhiteSpace(line))

            return false;



        var readerId = profile?.ReaderId ?? ArgumentReaders.Kv;

        if (readerId == ArgumentReaders.Kv)

            return TryParse(line, out pathWire, out args);



        var endsWithSpace = line.EndsWith(' ');

        var tokens = line.TrimEnd().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

        if (tokens.Count == 0)

            return false;



        var tailStart = FindTailStart(tokens, readerId);

        if (tailStart < 0)

            tailStart = tokens.Count;



        var pathTokens = tokens.Take(tailStart).ToList();

        if (pathTokens.Count == 0)

            return false;



        pathWire = new SlashWireBody(pathTokens, endsWithSpace && tailStart >= tokens.Count);

        var tail = tailStart < tokens.Count ? string.Join(' ', tokens.Skip(tailStart)) : "";

        args = ArgumentNotation.Parse(tail, profile);

        return true;

    }



    static int FindTailStart(IReadOnlyList<string> tokens, string readerId) =>

        readerId switch

        {

            ArgumentReaders.Cli => IndexOfFirst(tokens, IsCliOptionToken),

            ArgumentReaders.Kv => IndexOfFirst(tokens, IsKvToken),

            _ => tokens.Count,

        };



    static int IndexOfFirst(IReadOnlyList<string> tokens, Func<string, bool> predicate)

    {

        for (var i = 0; i < tokens.Count; i++)

        {

            if (predicate(tokens[i]))

                return i;

        }



        return -1;

    }



    static bool IsCliOptionToken(string token) =>

        token.Length > 0 && token[0] == '-';



    private static bool IsKvToken(string token)

    {

        var eq = token.IndexOf('=');

        return eq > 0 && eq < token.Length - 1;

    }

}


