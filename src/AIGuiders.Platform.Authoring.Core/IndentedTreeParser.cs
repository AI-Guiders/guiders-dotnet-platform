namespace AIGuiders.Platform.Authoring.Core;

public sealed record IndentedNode(string Key, string? Value, IReadOnlyList<IndentedNode> Children);

public static class IndentedTreeParser
{
    public static IReadOnlyList<IndentedNode> Parse(IEnumerable<(int Line, string Text)> lines, int baseIndent = 2)
    {
        var items = lines
            .Select(l => (l.Line, l.Text, Indent: CountIndent(l.Text)))
            .Where(x => !string.IsNullOrWhiteSpace(x.Text))
            .ToList();

        return ParseLevel(items, 0, items.Count, baseIndent * 1, baseIndent);
    }

    static IReadOnlyList<IndentedNode> ParseLevel(
        List<(int Line, string Text, int Indent)> items,
        int start,
        int end,
        int minIndent,
        int step)
    {
        var nodes = new List<IndentedNode>();
        var i = start;
        while (i < end)
        {
            var (_, text, indent) = items[i];
            if (indent < minIndent)
            {
                i++;
                continue;
            }

            if (indent > minIndent)
            {
                i++;
                continue;
            }

            var (key, value) = SplitKv(text.Trim());
            var childStart = i + 1;
            var childEnd = childStart;
            while (childEnd < end && items[childEnd].Indent > indent)
            {
                childEnd++;
            }

            var children = childStart < childEnd
                ? ParseLevel(items, childStart, childEnd, indent + step, step)
                : [];

            nodes.Add(new IndentedNode(key, value, children));
            i = childEnd;
        }

        return nodes;
    }

    static (string Key, string? Value) SplitKv(string line)
    {
        var eq = line.IndexOf('=');
        if (eq <= 0)
        {
            return (line.Trim(), null);
        }

        return (line[..eq].Trim(), line[(eq + 1)..].Trim());
    }

    static int CountIndent(string line)
    {
        var n = 0;
        foreach (var ch in line)
        {
            if (ch == ' ')
            {
                n++;
            }
            else if (ch == '\t')
            {
                n += 2;
            }
            else
            {
                break;
            }
        }

        return n;
    }
}
