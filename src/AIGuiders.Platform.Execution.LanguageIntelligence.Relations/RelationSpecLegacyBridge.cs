#nullable enable

using System.Linq;
using AIGuiders.Platform.Execution.LanguageIntelligence;
using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;
using AIGuiders.Platform.Modeling.Notations.Bracket;
using AIGuiders.Platform.Notations.Bracket;
using Microsoft.FSharp.Collections;
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
        if (spec is not RelationSpec.CodeEdit codeEdit)
            return false;

        if (codeEdit.target is not CodeTarget.Symbol symbolTarget)
            return false;

        if (symbolTarget.doc is not DocumentRef.File fileRef || fileRef.Item.IsEmpty)
            return false;

        var symbol = symbolTarget.symbol;
        var container = ListModule.ToArray(symbol.Container);

        if (container.Length >= 3 && container[0] == XmlWireEncoding.Marker)
        {
            span = new BracketAnchorSpan(
                File: fileRef.Item.Value,
                MemberKey: null,
                LineStart: null,
                LineEnd: null,
                XmlPath: symbol.Name,
                Attr: OptNonEmpty(container[1]),
                Role: OptNonEmpty(container[2]));
            return true;
        }

        span = new BracketAnchorSpan(
            File: fileRef.Item.Value,
            MemberKey: symbol.Name,
            LineStart: null,
            LineEnd: null,
            ScopeKind: container.Length == 0 ? null : string.Join(".", container));
        return true;
    }

    static string? OptNonEmpty(string? raw) =>
        string.IsNullOrWhiteSpace(raw) ? null : raw.Trim();
}
