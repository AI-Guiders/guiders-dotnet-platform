namespace AIGuiders.Platform.Authoring.Emit;

public static class GdlQuarryRegistry
{
    private static readonly object Gate = new();
    private static readonly List<IGdlQuarryPlugin> Plugins = [];
    private static bool initialized;

    public static bool IsInitialized
    {
        get
        {
            lock (Gate)
            {
                return initialized;
            }
        }
    }

    public static void Register(IGdlQuarryPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        lock (Gate)
        {
            Plugins.Add(plugin);
        }
    }

    public static void MarkInitialized()
    {
        lock (Gate)
        {
            initialized = true;
        }
    }

    public static IReadOnlyList<IGdlQuarryPlugin> Snapshot()
    {
        lock (Gate)
        {
            return Plugins.ToArray();
        }
    }

    public static bool TryResolve(
        string quarryId,
        string lang,
        string? surface,
        out IGdlQuarryPlugin? plugin,
        out string? error)
    {
        plugin = null;
        error = null;

        var normalizedSurface = NormalizeSurface(quarryId, surface);
        var matches = Snapshot()
            .Where(candidate => candidate.CanHandle(quarryId, lang, normalizedSurface))
            .ToArray();

        if (matches.Length == 1)
        {
            plugin = matches[0];
            return true;
        }

        if (matches.Length == 0)
        {
            error = DescribeMissing(quarryId, lang, normalizedSurface);
            return false;
        }

        error = $"ambiguous quarry plugin for `{quarryId}` lang `{lang}` surface `{normalizedSurface ?? "(none)"}`";
        return false;
    }

    public static string? NormalizeSurface(string quarryId, string? surface)
    {
        if (string.Equals(quarryId, GdlQuarryIds.Deck, StringComparison.OrdinalIgnoreCase))
        {
            return string.IsNullOrWhiteSpace(surface) ? GdlSurfaceIds.Wpf : surface.Trim();
        }

        return string.IsNullOrWhiteSpace(surface) ? null : surface.Trim();
    }

    private static string DescribeMissing(string quarryId, string lang, string? surface)
    {
        var registered = Snapshot()
            .Select(static plugin => $"{plugin.QuarryId}/{string.Join(',', plugin.SupportedLanguages)}/{plugin.SurfaceId ?? "(any)"}")
            .ToArray();

        var registryHint = registered.Length == 0
            ? "no quarry plugins registered"
            : $"registered: {string.Join("; ", registered)}";

        return $"no plugin for quarry `{quarryId}` lang `{lang}` surface `{surface ?? "(none)"}` — {registryHint}";
    }
}
