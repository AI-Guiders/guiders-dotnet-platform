namespace AIGuiders.Platform.Authoring.Command.Catalog;

public static class CatalogProfileTable
{
    public static void MergeRows(IList<CatalogProfile> target, IReadOnlyList<Dictionary<string, string>> maps)
    {
        foreach (var group in maps.GroupBy(static m => m.GetValueOrDefault("profile") ?? "", StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(group.Key))
            {
                continue;
            }

            var bundleRow = group.FirstOrDefault(static m =>
                string.Equals(m.GetValueOrDefault("entry"), "bundle", StringComparison.OrdinalIgnoreCase));
            if (bundleRow is not null)
            {
                target.Add(new()
                {
                    Name = group.Key,
                    BundleSource = bundleRow.GetValueOrDefault("ref") ?? "",
                });
                continue;
            }

            var entries = group
                .Where(static m => !string.Equals(m.GetValueOrDefault("entry"), "bundle", StringComparison.OrdinalIgnoreCase))
                .Select(static m => new CatalogProfileEntry(
                    m.GetValueOrDefault("arg") ?? "",
                    m.GetValueOrDefault("entry") ?? "",
                    m.GetValueOrDefault("ref") ?? ""))
                .Where(static e => e.Entry.Length > 0)
                .ToList();

            if (entries.Count == 0)
            {
                continue;
            }

            target.Add(new() { Name = group.Key, Entries = entries });
        }
    }
}
