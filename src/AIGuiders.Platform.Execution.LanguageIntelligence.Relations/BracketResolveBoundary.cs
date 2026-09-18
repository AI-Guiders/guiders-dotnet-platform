#nullable enable

using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Relations;

/// <summary>
/// Unified bracket wire resolve entry: Kind: RelationSpec only (plan §10). Legacy F/M/L ingest via <see cref="RelationWireBoundary"/>.
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

        return false;
    }

    public static bool TryParseNav(string bracketOrInner, out NavResolveAxes axes, out string? parsePath)
    {
        axes = default!;
        parsePath = null;
        if (string.IsNullOrWhiteSpace(bracketOrInner))
            return false;

        var spec = RelationSpecWireBoundary.TryParseKindSpec(bracketOrInner);
        if (spec is not null && NavResolveProjection.TryFromRelationSpec(spec, out axes))
        {
            parsePath = "kind-nav";
            return true;
        }

        return false;
    }

    /// <summary>Emit Kind:CodeEdit wire from resolve axes (plan §10 consumer codemod).</summary>
    public static bool TryFormatCodeEdit(CodeEditResolveAxes axes, out string wire)
    {
        wire = "";
        if (string.IsNullOrWhiteSpace(axes.File))
            return false;

        var parts = new List<string> { "Kind:CodeEdit", $"File:{axes.File.Trim()}" };

        if (!string.IsNullOrWhiteSpace(axes.XmlPath))
        {
            parts.Add($"Element:{axes.XmlPath.Trim()}");
            if (!string.IsNullOrWhiteSpace(axes.Attr))
                parts.Add($"Attribute:{axes.Attr.Trim()}");
            if (!string.IsNullOrWhiteSpace(axes.Role))
                parts.Add($"Role:{axes.Role.Trim()}");
        }
        else if (!string.IsNullOrWhiteSpace(axes.MemberKey))
            parts.Add($"Member:{axes.MemberKey.Trim()}");

        if (string.IsNullOrWhiteSpace(axes.MemberKey) && axes.LineStart is int ls)
        {
            parts.Add(axes.LineEnd is int le && le != ls
                ? $"Line:{ls}-{le}"
                : $"Line:{ls}");
        }

        if (!string.IsNullOrWhiteSpace(axes.ScopeKind))
        {
            var kind = axes.ScopeKind.Trim().ToLowerInvariant();
            var idx = axes.ScopeIndex is > 0 ? axes.ScopeIndex.Value : 1;
            parts.Add(idx == 1 ? $"Scope:{kind}" : $"Scope:{kind}:{idx}");
        }

        if (!string.IsNullOrWhiteSpace(axes.TextNeedle))
            parts.Add($"Text:{RelationWireBoundary.SanitizeTextNeedle(axes.TextNeedle)}");

        if (!string.IsNullOrWhiteSpace(axes.TypeKey))
            parts.Add($"Type:{axes.TypeKey.Trim()}");

        wire = "[" + string.Join("; ", parts) + "]";
        return true;
    }

    public static bool TryFormatCodeEdit(LegacyWireSpan legacy, out string wire)
    {
        wire = "";
        return CodeEditResolveProjection.TryFromLegacyWire(legacy, out var axes)
               && TryFormatCodeEdit(axes, out wire);
    }

    public static bool TryFormatNav(NavResolveAxes axes, out string wire)
    {
        wire = "";
        if (string.IsNullOrWhiteSpace(axes.File)
            && string.IsNullOrWhiteSpace(axes.Command)
            && string.IsNullOrWhiteSpace(axes.Go))
            return false;

        var parts = new List<string> { "Kind:Nav" };
        if (!string.IsNullOrWhiteSpace(axes.File))
            parts.Add($"File:{axes.File.Trim()}");
        if (axes.Line is int line)
            parts.Add($"Line:{line}");
        if (axes.Column is int column)
            parts.Add($"Column:{column}");
        if (!string.IsNullOrWhiteSpace(axes.Command))
            parts.Add($"Command:{axes.Command.Trim()}");
        if (!string.IsNullOrWhiteSpace(axes.Go))
            parts.Add($"Go:{axes.Go.Trim()}");
        if (!string.IsNullOrWhiteSpace(axes.Solution))
            parts.Add($"Solution:{axes.Solution.Trim()}");

        wire = "[" + string.Join("; ", parts) + "]";
        return true;
    }

    public static bool TryFormatNav(LegacyWireSpan legacy, out string wire)
    {
        wire = "";
        return NavResolveProjection.TryFromLegacyNav(legacy, out var axes)
               && TryFormatNav(axes, out wire);
    }
}
