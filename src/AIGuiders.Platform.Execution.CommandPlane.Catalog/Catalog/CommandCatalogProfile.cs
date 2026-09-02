#nullable enable

using AIGuiders.Platform.Modeling.Catalog;

namespace AIGuiders.Platform.Execution.CommandPlane;

/// <summary>Command catalog profile: path keys, ship-first merge (GUIDERS-ADR-0041).</summary>
public sealed class CommandCatalogProfile : ICatalogProfile<CommandDescriptor, string, CatalogRouteEntry>
{
    public static CommandCatalogProfile Instance { get; } = new();

    public IEqualityComparer<string> KeyComparer => StringComparer.OrdinalIgnoreCase;

    public CatalogIndexCollisionPolicy LayerCollisionPolicy => CatalogIndexCollisionPolicy.ShipFirst;

    public CatalogIndexCollisionPolicy MergeCollisionPolicy => CatalogIndexCollisionPolicy.ShipFirst;

    public IEnumerable<(string, CatalogRouteEntry)> Project(CommandDescriptor descriptor)
    {
        foreach (var path in descriptor.AllPaths())
        {
            var normalized = NormalizePath(path);
            if (normalized.Length == 0)
            {
                continue;
            }

            yield return (normalized, CatalogRouteEntry.FromDescriptor(descriptor, normalized));
        }
    }

    public string NormalizeKey(string key) => NormalizePath(key);

    internal static string NormalizePath(string path)
    {
        var p = path.Trim();
        if (p.StartsWith('/'))
            p = p[1..];
        return p.Trim();
    }
}
