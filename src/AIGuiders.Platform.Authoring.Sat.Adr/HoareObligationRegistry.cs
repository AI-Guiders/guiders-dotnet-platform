namespace AIGuiders.Platform.Authoring.Sat;

/// <summary>
/// Pilot registry for Hoare obligation ids declared in ADR facts blocks.
/// Production wiring can populate this from ide-session gates / HoareChecker.
/// </summary>
public static class HoareObligationRegistry
{
    private static readonly object Gate = new();
    private static readonly HashSet<string> Registered = new(StringComparer.OrdinalIgnoreCase);

    public static void Register(string obligationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(obligationId);
        lock (Gate)
        {
            Registered.Add(obligationId.Trim());
        }
    }

    public static void RegisterRange(IEnumerable<string> obligationIds)
    {
        ArgumentNullException.ThrowIfNull(obligationIds);
        lock (Gate)
        {
            foreach (var id in obligationIds)
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    Registered.Add(id.Trim());
                }
            }
        }
    }

    public static bool IsRegistered(string obligationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(obligationId);
        lock (Gate)
        {
            return Registered.Contains(obligationId.Trim());
        }
    }

    public static IReadOnlyList<string> Snapshot()
    {
        lock (Gate)
        {
            return Registered.OrderBy(static id => id, StringComparer.OrdinalIgnoreCase).ToArray();
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
