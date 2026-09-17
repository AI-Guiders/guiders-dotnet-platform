#nullable enable

namespace AIGuiders.Platform.Execution.LanguageIntelligence;

/// <summary>Execution-only language backend registration — Modeling never owns runtime slots (plan §2.3).</summary>
public sealed record AdapterSlot(string ProfileId, string BackendId);

/// <summary>Registry placeholder until Roslyn/FCS backends register emitters.</summary>
public static class AdapterSlotRegistry
{
    static readonly List<AdapterSlot> Slots = [];

    public static IReadOnlyList<AdapterSlot> All => Slots;

    public static void Register(AdapterSlot slot)
    {
        if (Slots.Any(existing =>
                string.Equals(existing.ProfileId, slot.ProfileId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(existing.BackendId, slot.BackendId, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        Slots.Add(slot);
    }

    public static void ResetForTests() => Slots.Clear();
}
