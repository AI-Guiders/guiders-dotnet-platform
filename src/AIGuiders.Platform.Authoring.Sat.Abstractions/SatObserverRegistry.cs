namespace AIGuiders.Platform.Authoring.Sat;

using AIGuiders.Platform.Authoring.Emit;

public static class SatObserverRegistry
{
    private static readonly object Gate = new();
    private static readonly List<ISatObserver> Observers = [];
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

    public static void Register(ISatObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        lock (Gate)
        {
            Observers.Add(observer);
        }
    }

    public static void MarkInitialized()
    {
        lock (Gate)
        {
            initialized = true;
        }
    }

    public static IReadOnlyList<ISatObserver> Snapshot()
    {
        lock (Gate)
        {
            return Observers
                .OrderByDescending(static o => o.Priority)
                .ThenBy(static o => o.ObserverId, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    public static bool TryObserve(SatContext context, out SatRunResult? result, out string? error)
    {
        result = null;
        error = null;

        var matches = Snapshot().Where(o => o.CanObserve(context)).ToArray();
        if (matches.Length == 0)
        {
            error = DescribeMissing(context);
            return false;
        }

        var diagnostics = new List<GdlDiagnostic>();
        var summaries = new List<string>();
        var anySupported = false;

        foreach (var observer in matches)
        {
            var run = observer.Observe(context);
            if (run.Supported)
            {
                anySupported = true;
            }

            if (!string.IsNullOrWhiteSpace(run.Summary))
            {
                summaries.Add(run.Summary);
            }

            if (!run.Success)
            {
                diagnostics.AddRange(run.Diagnostics);
            }
        }

        if (diagnostics.Count > 0)
        {
            result = SatRunResult.Failed(
                diagnostics,
                $"sat: failed — {string.Join("; ", summaries)}");
            return true;
        }

        if (!anySupported)
        {
            result = SatRunResult.Skipped(
                summaries.Count > 0
                    ? string.Join(Environment.NewLine, summaries)
                    : "sat: skipped — no supported observers");
            return true;
        }

        result = SatRunResult.Ok(
            summaries.Count > 0
                ? string.Join(Environment.NewLine, summaries)
                : "sat: ok");
        return true;
    }

    private static string DescribeMissing(SatContext context)
    {
        var hint = Snapshot()
            .Select(static o => o.ObserverId)
            .ToArray();

        var registered = hint.Length == 0
            ? "no sat observers registered"
            : $"registered: {string.Join(", ", hint)}";

        if (!string.IsNullOrWhiteSpace(context.AdrId))
        {
            return $"no sat observer for adr `{context.AdrId}` — {registered}";
        }

        if (!string.IsNullOrWhiteSpace(context.FactsPath))
        {
            return $"no sat observer for facts `{context.FactsPath}` — {registered}";
        }

        if (!string.IsNullOrWhiteSpace(context.Path))
        {
            return $"no sat observer for `{context.Path}` — {registered}";
        }

        return $"no sat observer for context — {registered}";
    }
}
