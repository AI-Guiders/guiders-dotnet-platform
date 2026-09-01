namespace AIGuiders.Platform.Authoring.Core;

public static class TableRowParser
{
    public static bool TryParseRow(string line, out IReadOnlyList<string> cells)
    {
        var trimmed = line.Trim();
        if (!trimmed.StartsWith('|') || !trimmed.EndsWith('|'))
        {
            cells = [];
            return false;
        }

        cells = trimmed.Trim('|')
            .Split('|', StringSplitOptions.TrimEntries)
            .ToArray();
        return cells.Count > 0;
    }

    public static bool IsSeparatorRow(IReadOnlyList<string> cells) =>
        cells.Count > 0 && cells.All(static c => c.All(static ch => ch is '-' or ':' or ' '));
}
