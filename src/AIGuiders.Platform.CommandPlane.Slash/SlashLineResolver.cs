#nullable enable

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Canonical path + arg tail from slash body (ADR-0150 quarry).</summary>
public static class SlashLineResolver
{
    public readonly record struct SlashLineResolution(
        string CanonicalPath,
        string ArgTail,
        SlashArgTailKind ArgTailKind,
        bool IsCatalogMatch,
        bool IsExactPathMatch,
        bool EndsWithSpaceAfterPath,
        bool HasArgTailContent)
    {
        public bool ShouldHideSegmentSuggestions =>
            IsCatalogMatch && (
                (ArgTailKind == SlashArgTailKind.None && IsExactPathMatch)
                || ArgTailKind == SlashArgTailKind.Optional && (IsExactPathMatch || EndsWithSpaceAfterPath || HasArgTailContent)
                || (ArgTailKind == SlashArgTailKind.Required && HasArgTailContent)
                || (ArgTailKind == SlashArgTailKind.Picker && (EndsWithSpaceAfterPath || HasArgTailContent)));

        public bool InsertsTrailingSpaceOnCommit => SlashArgTailPolicy.InsertsTrailingSpaceOnCommit(ArgTailKind);

        public bool IsRunnable =>
            IsCatalogMatch
            && (ArgTailKind != SlashArgTailKind.Required || !string.IsNullOrWhiteSpace(ArgTail));
    }

    public static bool TryResolveSlashLine(string slashLine, SlashCatalogIndex catalog, out SlashLineResolution resolution)
    {
        resolution = default;
        if (string.IsNullOrWhiteSpace(slashLine) || slashLine[0] != '/')
            return false;

        var body = slashLine[1..].TrimEnd();
        return TryResolveBody(body, catalog, out resolution);
    }

    public static bool TryResolveBody(string body, SlashCatalogIndex catalog, out SlashLineResolution resolution)
    {
        resolution = default;
        ParseTypedBody(body, out var tokens, out var endsWithSpace);
        if (tokens.Count == 0)
            return false;

        if (!catalog.TryResolveLongestPrefix(
                tokens, endsWithSpace,
                out var path, out var argTail, out var isExact, out var endsSpaceAfter, out var entry))
            return false;

        resolution = new SlashLineResolution(
            path,
            argTail,
            entry.ArgTailKind,
            true,
            isExact,
            endsSpaceAfter,
            !string.IsNullOrWhiteSpace(argTail));
        return true;
    }

    internal static void ParseTypedBody(string body, out List<string> tokens, out bool endsWithSpace)
    {
        endsWithSpace = body.EndsWith(' ');
        tokens = body.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}
