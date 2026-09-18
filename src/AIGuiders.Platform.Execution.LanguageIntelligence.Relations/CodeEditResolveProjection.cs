#nullable enable

using System.Linq;
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

/// <summary>Project RelationSpec.CodeEdit and legacy wire spans into resolver axes (plan §10).</summary>
public static class CodeEditResolveProjection
{
    public static bool TryFromRelationSpec(RelationSpec spec, out CodeEditResolveAxes axes)
    {
        axes = default!;
        if (spec is not RelationSpec.CodeEdit codeEdit)
            return false;

        if (codeEdit.target is not CodeTarget.Symbol symbolTarget)
            return false;

        if (symbolTarget.doc is not DocumentRef.File fileRef || fileRef.Item.IsEmpty)
            return false;

        var symbol = symbolTarget.symbol;
        var container = symbol.Container;
        var lineHint = CodeEditWireEncoding.tryDecodeLine(container);
        var scopeHint = CodeEditWireEncoding.tryDecodeScope(container);
        var stripped = ListModule.ToArray(CodeEditWireEncoding.stripHints(container));

        int? lineStart = null;
        int? lineEnd = null;
        if (OptionModule.IsSome(lineHint))
        {
            var decoded = OptionModule.GetValue(lineHint);
            lineStart = decoded.Item1;
            lineEnd = OptionModule.IsSome(decoded.Item2)
                ? OptionModule.GetValue(decoded.Item2)
                : decoded.Item1;
        }

        string? scopeKind = null;
        int? scopeIndex = null;
        if (OptionModule.IsSome(scopeHint))
        {
            var decoded = OptionModule.GetValue(scopeHint);
            scopeKind = decoded.Item1;
            scopeIndex = decoded.Item2;
        }

        if (stripped.Length >= 3 && stripped[0] == XmlWireEncoding.Marker)
        {
            axes = new CodeEditResolveAxes(
                File: fileRef.Item.Value,
                MemberKey: null,
                LineStart: lineStart,
                LineEnd: lineEnd,
                XmlPath: symbol.Name,
                Attr: OptNonEmpty(stripped[1]),
                Role: OptNonEmpty(stripped[2]));
            return true;
        }

        axes = new CodeEditResolveAxes(
            File: fileRef.Item.Value,
            MemberKey: string.IsNullOrWhiteSpace(symbol.Name) ? null : symbol.Name,
            LineStart: lineStart,
            LineEnd: lineEnd,
            ScopeKind: scopeKind,
            ScopeIndex: scopeIndex);
        return true;
    }

    static string? OptNonEmpty(string? raw) =>
        string.IsNullOrWhiteSpace(raw) ? null : raw.Trim();
}
