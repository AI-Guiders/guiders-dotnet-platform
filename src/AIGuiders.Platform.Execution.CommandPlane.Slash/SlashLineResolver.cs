using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

using AIGuiders.Platform.Notations.Command.Slash;

namespace AIGuiders.Platform.Execution.CommandPlane;

/// <summary>Canonical path + arg tail from slash body (ADR-0150 quarry).</summary>
public static class SlashLineResolver
{
    public readonly record struct SlashLineResolution(
        string CanonicalPath,
        string ArgTail,
        CommandArgTailKind ArgTailKind,
        bool IsCatalogMatch,
        bool IsExactPathMatch,
        bool EndsWithSpaceAfterPath,
        bool HasArgTailContent)
    {
        public bool ShouldHideSegmentSuggestions =>
            IsCatalogMatch && (
                (ArgTailKind == CommandArgTailKind.None && IsExactPathMatch)
                || ArgTailKind == CommandArgTailKind.Optional && (IsExactPathMatch || EndsWithSpaceAfterPath || HasArgTailContent)
                || (ArgTailKind == CommandArgTailKind.Required && HasArgTailContent)
                || (ArgTailKind == CommandArgTailKind.Picker && (EndsWithSpaceAfterPath || HasArgTailContent)));

        public bool InsertsTrailingSpaceOnCommit => CommandArgTailPolicy.InsertsTrailingSpaceOnCommit(ArgTailKind);

        public bool IsRunnable =>
            IsCatalogMatch
            && (ArgTailKind != CommandArgTailKind.Required || !string.IsNullOrWhiteSpace(ArgTail));
    }

    public static bool TryResolveSlashLine(string slashLine, CommandCatalogIndex catalog, out SlashLineResolution resolution)
    {
        resolution = default;
        if (string.IsNullOrWhiteSpace(slashLine) || slashLine[0] != '/')
            return false;

        var body = slashLine[1..].TrimEnd();
        return TryResolveBody(body, catalog, out resolution);
    }

    public static bool TryResolveBody(string body, CommandCatalogIndex catalog, out SlashLineResolution resolution)
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
        var wire = SlashCommandNotation.ParseBody(body);
        endsWithSpace = wire.EndsWithSpaceAfterTokens;
        tokens = wire.Tokens.ToList();
    }
}
