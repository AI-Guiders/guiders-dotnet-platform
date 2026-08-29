#nullable enable

namespace AIGuiders.Platform.CommandPlane.Editor;

public static class EditorLineRangeParser
{
    public static bool TryParse(string? tail, out EditorLineRange range)
    {
        range = default;
        if (string.IsNullOrWhiteSpace(tail))
            return false;

        var parts = tail.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            var token = parts[0];
            var sep = token.IndexOfAny([':', '-']);
            if (sep > 0)
            {
                if (!int.TryParse(token[..sep], out var a) || !int.TryParse(token[(sep + 1)..], out var b))
                    return false;
                range = new EditorLineRange(Math.Min(a, b), Math.Max(a, b));
                return true;
            }

            if (!int.TryParse(token, out var single))
                return false;
            range = new EditorLineRange(single, single);
            return true;
        }

        if (parts.Length >= 2
            && int.TryParse(parts[0], out var start)
            && int.TryParse(parts[1], out var end))
        {
            range = new EditorLineRange(Math.Min(start, end), Math.Max(start, end));
            return true;
        }

        return false;
    }
}
