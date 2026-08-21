#nullable enable

namespace AIGuiders.Platform.Cockpit.DataBus;

/// <summary>
/// Async route policy for <see cref="InMemoryDataBus"/>:
/// burst → bounded(1) + DropOldest; otherwise unbounded reliable queue.
/// </summary>
public readonly struct DataBusEventPolicy
{
    readonly IReadOnlyDictionary<string, bool>? _burstByTypeName;

    /// <summary>All events reliable (no burst DropOldest). Default when policy omitted.</summary>
    public static DataBusEventPolicy AllReliable { get; } = new(new Dictionary<string, bool>());

    /// <summary>Embedded default (CIDE ADR 0099 quarry).</summary>
    public static DataBusEventPolicy Default { get; } = new(new Dictionary<string, bool>(StringComparer.Ordinal)
    {
        ["DebugStateChanged"] = true,
        ["GitStateChanged"] = true,
        ["IdeHostStateChanged"] = true,
        ["BuildStateChanged"] = false,
        ["TestsStateChanged"] = false,
        ["StartupProjectPathChanged"] = false,
    });

    public DataBusEventPolicy(IReadOnlyDictionary<string, bool> burstByTypeName)
    {
        ArgumentNullException.ThrowIfNull(burstByTypeName);
        _burstByTypeName = burstByTypeName;
    }

    public bool IsBurst(Type eventType) =>
        _burstByTypeName?.GetValueOrDefault(eventType.Name, defaultValue: false) == true;
}
