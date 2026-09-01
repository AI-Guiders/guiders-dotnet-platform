namespace AIGuiders.Platform.Authoring.Core;

public static class KvSurface
{
    public static void MergeInto(IDictionary<string, string> target, IEnumerable<AuthoringLine> body)
    {
        foreach (var line in body)
        {
            if (!TryParsePair(line.Text, out var key, out var value))
            {
                continue;
            }

            target[key] = value;
        }
    }

    public static IReadOnlyList<(string Name, string? Value)> ParseNameOrPair(IEnumerable<AuthoringLine> body)
    {
        var list = new List<(string, string?)>();
        foreach (var line in body)
        {
            if (TryParsePair(line.Text, out var key, out var value))
            {
                list.Add((key, value));
            }
            else if (!string.IsNullOrWhiteSpace(line.Text))
            {
                list.Add((line.Text.Trim(), null));
            }
        }

        return list;
    }

    public static bool TryParsePair(string text, out string key, out string value)
    {
        key = "";
        value = "";
        var eq = text.IndexOf('=');
        if (eq <= 0)
        {
            return false;
        }

        key = text[..eq].Trim();
        value = text[(eq + 1)..].Trim();
        return key.Length > 0;
    }

    public static IReadOnlyList<string> ParseList(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? []
            : raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}
