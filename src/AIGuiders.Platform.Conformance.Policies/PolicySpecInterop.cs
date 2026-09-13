#nullable enable
using System.Text.Json;
using AIGuiders.Platform.Combinations;

namespace AIGuiders.Platform.Conformance.Policies;

/// <summary>
/// Bridges F# conformance SSOT wire shapes to platform combinators (W3 CombinationSemantics struct).
/// </summary>
internal static class PolicySpecInterop
{
    internal static CombinationSemantics ParseSemantics(string semantics) => semantics switch
    {
        "ShipFirst" => CombinationSemantics.ShipFirst,
        "OverlayWins" => CombinationSemantics.OverlayWins,
        "FieldOverlay" => CombinationSemantics.FieldOverlay,
        "SectionReplace" => CombinationSemantics.SectionReplace,
        _ => throw new ArgumentOutOfRangeException(nameof(semantics), semantics, "Unknown combination semantics."),
    };

    internal static JsonElement ParseJson(string json) =>
        JsonDocument.Parse(json).RootElement.Clone();
}
