#nullable enable

using Microsoft.Z3;

namespace AIGuiders.Platform.Tools.CombinationsProof;

/// <summary>Probe whether the native Z3 library matches the managed binding (CI often lacks libz3).</summary>
public static class Z3ProofRuntime
{
    static bool? _available;

    public static bool IsAvailable()
    {
        if (_available is { } cached)
            return cached;

        try
        {
            using var ctx = new Context();
            _available = true;
        }
        catch (DllNotFoundException)
        {
            _available = false;
        }
        catch (EntryPointNotFoundException)
        {
            _available = false;
        }

        return _available.Value;
    }
}
