#nullable enable

namespace AIGuiders.Platform.Notations;

/// <summary>Split list segments respecting nested bracket depth (CDP BracketLocate parity).</summary>
public static class NotationListSplit
{
    public static List<string> SplitTopLevel(
        string text,
        char separator,
        char openBracket = '[',
        char closeBracket = ']')
    {
        var parts = new List<string>();
        var depth = 0;
        var start = 0;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c == openBracket)
                depth++;
            else if (c == closeBracket)
                depth = Math.Max(0, depth - 1);
            else if (c == separator && depth == 0)
            {
                parts.Add(text[start..i]);
                start = i + 1;
            }
        }

        parts.Add(text[start..]);
        return parts;
    }
}
