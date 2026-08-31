#nullable enable

namespace AIGuiders.Platform.CommandPlane;

static class SlashCompletionSort
{
    public static IReadOnlyList<ArgCompletionItem> Order(IEnumerable<ArgCompletionItem> items) =>
        items.OrderBy(i => SortKey(i.SlashPath), StringComparer.OrdinalIgnoreCase).ToList();

    public static string SortKey(string slashPath)
    {
        var path = slashPath.Trim();
        if (path.StartsWith('/'))
            path = path[1..];
        return path.ToLowerInvariant();
    }
}
