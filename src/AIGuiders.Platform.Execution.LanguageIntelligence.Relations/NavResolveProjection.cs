#nullable enable

using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;
using AIGuiders.Platform.Modeling.Paths;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Relations;

/// <summary>Project RelationSpec.Nav and legacy navigation spans into resolver axes (plan §10).</summary>
public static class NavResolveProjection
{
    public static bool TryFromRelationSpec(RelationSpec spec, out NavResolveAxes axes)
    {
        axes = default!;
        if (spec is not RelationSpec.Nav nav)
            return false;

        var seed = nav.source;
        axes = new NavResolveAxes(
            File: seed.Path.Value,
            Line: seed.Line is not null && FSharpOption<int>.get_IsSome(seed.Line) ? seed.Line.Value : null,
            Column: seed.Column is not null && FSharpOption<int>.get_IsSome(seed.Column) ? seed.Column.Value : null,
            Command: seed.Command is not null && FSharpOption<string>.get_IsSome(seed.Command) ? seed.Command.Value : null,
            Go: seed.Go is not null && FSharpOption<string>.get_IsSome(seed.Go) ? seed.Go.Value : null,
            Solution: seed.Solution is not null && FSharpOption<LogicalPath>.get_IsSome(seed.Solution)
                ? seed.Solution.Value.Value
                : null);
        return true;
    }

    public static bool TryFromLegacyNav(LegacyWireSpan legacy, out NavResolveAxes axes)
    {
        axes = default!;
        if (RelationWireBoundary.ClassifyFamily(legacy, out _) != BracketAxisFamily.Navigation)
            return false;

        var file = legacy.File;
        int? line = legacy.LineStart;
        if (legacy.NestedAnchor is { } nested)
        {
            file ??= nested.File;
            line ??= nested.LineStart;
        }

        axes = new NavResolveAxes(
            File: file,
            Line: line,
            Command: legacy.Command,
            Go: legacy.Go);
        return !string.IsNullOrWhiteSpace(file)
               || !string.IsNullOrWhiteSpace(legacy.Command)
               || !string.IsNullOrWhiteSpace(legacy.Go);
    }
}
