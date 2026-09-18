#nullable enable

using AIGuiders.Platform.Execution.Documentation.Correspondence;
using AIGuiders.Platform.Execution.Ide.Session;
using AIGuiders.Platform.Modeling.Core.Identity;
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;
using AIGuiders.Platform.Modeling.Paths;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class CorrespondenceRelationIngestTests
{
    static SessionRuntime CreateRuntime(string filePath, string contents)
    {
        var anchor = LogicalPath.Create(Path.GetFullPath(@"D:\repo\App.slnx"));
        var graph = SolutionGraphModule.create(anchor, ListModule.Empty<ProjectNode>(), ListModule.Empty<Relation>());
        var session = SolutionSessionModule.withPhase(LifecyclePhase.Unloaded, SolutionSessionModule.create(anchor, graph));

        var owner = AIGuiders.Platform.Modeling.Ide.Session.ProjectIdModule.create(Path.GetFullPath(@"D:\repo\App.csproj"));
        var fullPath = Path.GetFullPath(filePath);
        FSharpMap<string, ProjectId> ownership = MapModule.OfSeq([Tuple.Create(fullPath, owner)]);

        return SessionOrchestrator.create(
            session,
            [Tuple.Create(fullPath, contents)],
            ownership);
    }

    [Fact]
    public void IngestDocToCodeWitnesses_merges_normates_edge_into_graph()
    {
        var runtime = CreateRuntime(@"D:\repo\src\Foo.cs", "class Bar {}");
        Assert.Empty(runtime.Session.Graph.Relations);

        var result = CorrespondenceRelationIngest.IngestDocToCodeWitnesses(
            [
                new DocToCodeWitness(
                    "docs/adr/0063.md",
                    "ADR-0063",
                    CorrespondenceProvenance.Bracket,
                    CorrespondenceKind.Normates,
                    "src/Foo.cs",
                    null,
                    null,
                    "Bar",
                    "[F:src/Foo.cs; M:Bar]"),
            ],
            runtime);

        Assert.Equal(1, result.Materialized);
        Assert.Equal(0, result.Skipped);
        Assert.Single(result.Runtime.Session.Graph.Relations);
        Assert.Equal(RelationType.Normates, result.Runtime.Session.Graph.Relations[0].Type);
    }
}
