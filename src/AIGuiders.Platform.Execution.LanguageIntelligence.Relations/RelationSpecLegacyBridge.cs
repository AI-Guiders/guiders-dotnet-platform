#nullable enable

using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;
using AIGuiders.Platform.Modeling.Notations.Bracket;
using AIGuiders.Platform.Notations.Bracket;
using BracketModel = AIGuiders.Platform.Modeling.Notations.Bracket;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Relations;

/// <summary>Kind: bracket wire → <see cref="RelationSpec"/> at parse boundary (plan §3).</summary>
public static class RelationSpecWireBoundary
{
    public static RelationSpec? TryParseKindSpec(string bracketOrInner)
    {
        if (string.IsNullOrWhiteSpace(bracketOrInner))
            return null;

        if (!BracketReader.Default.TryRead(
                bracketOrInner,
                BracketProfiles.CdpSquareKeyValue,
                out var wire,
                out _))
            return null;

        var parsed = BracketRelationWire.tryParseRelationSpec(wire!);
        if (!FSharpOption<RelationSpec>.get_IsSome(parsed))
            return null;

        return parsed!.Value;
    }
}

/// <summary>
/// Transitional bridge: RelationSpec witness → legacy <see cref="BracketAnchorSpan"/> for language resolvers.
/// </summary>
public static class RelationSpecLegacyBridge
{
    public static bool TryToLegacySpan(RelationSpec spec, out BracketAnchorSpan span)
    {
        span = default!;
        var modelOpt = BracketModel.RelationSpecLegacyBridge.tryToLegacySpan(spec);
        if (!FSharpOption<BracketModel.BracketAnchorSpan>.get_IsSome(modelOpt))
            return false;

        span = global::AIGuiders.Platform.Execution.LanguageIntelligence.BracketAnchorSpan.FromModel(modelOpt!.Value);
        return true;
    }
}
