#nullable enable

using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;
using AIGuiders.Platform.Modeling.Paths;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.LanguageIntelligence.Relations;

/// <summary>Project RelationSpec.Nav into resolver axes (plan §10).</summary>
public static class NavResolveProjection
{
    public static bool TryFromRelationSpec(RelationSpec spec, out NavResolveAxes axes)
    {
        axes = default!;
        if (spec is not RelationSpec.Nav nav)
            return false;

        var seed = nav.source;
        var file = seed.Path.IsEmpty ? null : seed.Path.Value;
        axes = new NavResolveAxes(
            File: file,
            Line: seed.Line is not null && FSharpOption<int>.get_IsSome(seed.Line) ? seed.Line.Value : null,
            Column: seed.Column is not null && FSharpOption<int>.get_IsSome(seed.Column) ? seed.Column.Value : null,
            Command: seed.Command is not null && FSharpOption<string>.get_IsSome(seed.Command) ? seed.Command.Value : null,
            Go: seed.Go is not null && FSharpOption<string>.get_IsSome(seed.Go) ? seed.Go.Value : null,
            Solution: seed.Solution is not null && FSharpOption<LogicalPath>.get_IsSome(seed.Solution)
                ? seed.Solution.Value.Value
                : null,
            Member: seed.Member is not null && FSharpOption<string>.get_IsSome(seed.Member) ? seed.Member.Value : null);
        return true;
    }
}
