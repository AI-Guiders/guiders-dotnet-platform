#nullable enable

namespace AIGuiders.Platform.Notations.Bracket;

/// <summary>Depth-aware bracket envelope discovery in prose/markdown (GUIDERS-ADR-0027).</summary>
public static class BracketEnvelopeScan
{
    public sealed record Envelope(int Start, int End, string Wire, string Inner);

    /// <summary>Find top-level <c>[ … ]</c> regions; nested brackets stay inside <see cref="Envelope.Inner"/>.</summary>
    public static IReadOnlyList<Envelope> LocateInText(
        string text,
        char open = '[',
        char close = ']')
    {
        var results = new List<Envelope>();
        var depth = 0;
        var start = -1;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == open)
            {
                if (depth == 0)
                    start = i;
                depth++;
            }
            else if (text[i] == close && depth > 0)
            {
                depth--;
                if (depth == 0 && start >= 0)
                {
                    var end = i + 1;
                    results.Add(new Envelope(start, end, text[start..end], text[(start + 1)..i]));
                    start = -1;
                }
            }
        }

        return results;
    }
}
