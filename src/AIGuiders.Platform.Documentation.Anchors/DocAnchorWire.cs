#nullable enable

using AIGuiders.Platform.IntermediateRepresentation.Bracket;

namespace AIGuiders.Platform.Documentation.Anchors;

public static class DocAnchorWire
{
    public static bool LooksLikeDocEnvelope(string inner) =>
        inner.Contains("Family:", StringComparison.OrdinalIgnoreCase)
        && inner.Contains("doc", StringComparison.OrdinalIgnoreCase);

    public static bool HasDocFamily(NormalizedBracketWire wire)
    {
        foreach (var axis in wire.Axes)
        {
            if (axis.Key.Equals("Family", StringComparison.OrdinalIgnoreCase)
                && axis.Value.Equals("doc", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public static string Format(
        NormalizedBracketWire wire,
        BracketNotationProfile? profile = null)
    {
        profile ??= BracketProfiles.DocSymbol;
        var parts = new List<string>(wire.Axes.Count);
        foreach (var axis in wire.Axes)
            parts.Add($"{axis.Key}{profile.KvSign}{axis.Value.Trim()}");

        return $"{profile.StartTerminal}{string.Join($"{profile.ListSeparator} ", parts)}{profile.EndTerminal}";
    }
}
