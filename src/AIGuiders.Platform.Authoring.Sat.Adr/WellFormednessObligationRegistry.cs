namespace AIGuiders.Platform.Authoring.Sat;

/// <summary>
/// Registry for well-formedness obligation ids declared in ADR facts blocks.
/// Built-in catalog entries delegate to F# SSOT; runtime entries may be added explicitly.
/// </summary>
public static class WellFormednessObligationRegistry
{
    private static readonly object Gate = new();
    private static readonly HashSet<string> Registered = new(StringComparer.OrdinalIgnoreCase);

    static WellFormednessObligationRegistry()
    {
        RegisterRange(HoareCatalogBridge.RegisteredWellFormednessIds());
    }

    public static void Register(string wfId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(wfId);
        lock (Gate)
        {
            Registered.Add(wfId.Trim());
        }
    }

    public static void RegisterRange(IEnumerable<string> wfIds)
    {
        ArgumentNullException.ThrowIfNull(wfIds);
        lock (Gate)
        {
            foreach (var id in wfIds)
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    Registered.Add(id.Trim());
                }
            }
        }
    }

    public static bool IsRegistered(string wfId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(wfId);
        var trimmed = wfId.Trim();
        if (HoareCatalogBridge.IsRegisteredWellFormedness(trimmed))
        {
            return true;
        }

        lock (Gate)
        {
            return Registered.Contains(trimmed);
        }
    }

    public static IReadOnlyList<string> Snapshot()
    {
        lock (Gate)
        {
            var merged = new HashSet<string>(Registered, StringComparer.OrdinalIgnoreCase);
            foreach (var id in HoareCatalogBridge.RegisteredWellFormednessIds())
            {
                merged.Add(id);
            }

            return merged.OrderBy(static id => id, StringComparer.OrdinalIgnoreCase).ToArray();
        }
    }

    public static void Clear()
    {
        lock (Gate)
        {
            Registered.Clear();
        }
    }
}
