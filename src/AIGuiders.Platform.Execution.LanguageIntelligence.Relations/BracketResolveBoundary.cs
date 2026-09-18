#nullable enable

using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Relations;

/// <summary>
/// Unified bracket wire resolve entry: Kind: RelationSpec first, legacy F/M/L fallback (plan §10).
/// </summary>
public static class BracketResolveBoundary
{
    public static bool TryParseToAxes(string bracketOrInner, out CodeEditResolveAxes axes, out string? parsePath)
    {
        axes = default!;
        parsePath = null;
        if (string.IsNullOrWhiteSpace(bracketOrInner))
            return false;

        var spec = RelationSpecWireBoundary.TryParseKindSpec(bracketOrInner);
        if (spec is RelationSpec.CodeEdit
            && CodeEditResolveProjection.TryFromRelationSpec(spec, out axes))
        {
            parsePath = "kind-spec";
            return true;
        }

        try
        {
            var legacy = RelationWireBoundary.Parse(bracketOrInner);
            if (CodeEditResolveProjection.TryFromLegacyWire(legacy, out axes))
            {
                parsePath = "legacy-wire";
                return true;
            }
        }
        catch (ArgumentException)
        {
            return false;
        }

        return false;
    }
}
