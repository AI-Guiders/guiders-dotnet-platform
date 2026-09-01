namespace AIGuiders.Platform.Authoring.Core;

public static class TableSurface
{
    public static IReadOnlyList<IReadOnlyList<string>> ParseRows(IEnumerable<AuthoringLine> body)
    {
        var rows = new List<IReadOnlyList<string>>();
        foreach (var line in body)
        {
            if (!TableRowParser.TryParseRow(line.Text, out var cells))
            {
                continue;
            }

            if (TableRowParser.IsSeparatorRow(cells))
            {
                continue;
            }

            rows.Add(cells);
        }

        return rows;
    }

    public static IReadOnlyList<Dictionary<string, string>> ParseMaps(IEnumerable<AuthoringLine> body)
    {
        var rows = ParseRows(body);
        if (rows.Count == 0)
        {
            return [];
        }

        var header = rows[0];
        var maps = new List<Dictionary<string, string>>();
        foreach (var row in rows.Skip(1))
        {
            maps.Add(ToMap(header, row));
        }

        return maps;
    }

    public static Dictionary<string, string> ToMap(IReadOnlyList<string> header, IReadOnlyList<string> row)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < header.Count && i < row.Count; i++)
        {
            map[header[i]] = row[i];
        }

        return map;
    }

    public static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
