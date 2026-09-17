#nullable enable

using AIGuiders.Platform.Execution.Ide.Session;
using AIGuiders.Platform.Modeling.Core.Identity;
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.Paths;
using Microsoft.FSharp.Collections;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class DependencyRelationIngestTests
{
    static SessionRuntime CreateRuntime(string filePath, string contents)
    {
        var anchor = LogicalPath.Create(Path.GetFullPath(@"D:\repo\App.slnx"));
        var graph = SolutionGraphModule.create(anchor, ListModule.Empty<ProjectNode>(), ListModule.Empty<Relation>());
        var session = SolutionSessionModule.withPhase(LifecyclePhase.Unloaded, SolutionSessionModule.create(anchor, graph));

        var owner = AIGuiders.Platform.Modeling.Ide.Session.ProjectIdModule.create(Path.GetFullPath(@"D:\repo\App.csproj"));
        var fullPath = Path.GetFullPath(filePath);
        FSharpMap<string, ProjectId> ownership =
            MapModule.OfSeq([Tuple.Create(fullPath, owner)]);

        return SessionOrchestrator.create(
            session,
            [Tuple.Create(fullPath, contents)],
            ownership);
    }

    [Fact]
    public void IngestFromContents_merges_roslyn_uses_into_graph()
    {
        const string source = """
            namespace Demo;

            public class Helper { }

            public class Consumer
            {
                Helper field;
            }
            """;

        var runtime = CreateRuntime(@"D:\repo\src\App.cs", source);
        Assert.Empty(runtime.Session.Graph.Relations);

        var result = DependencyRelationIngest.IngestFromContents(runtime);

        Assert.Equal(1, result.Ingested);
        Assert.Equal(0, result.SkippedNonCs);
        Assert.Single(result.Runtime.Session.Graph.Relations);
        Assert.Equal(RelationType.Uses, result.Runtime.Session.Graph.Relations[0].Type);
    }

    [Fact]
    public void IngestFromContents_skips_non_cs_contents()
    {
        var runtime = CreateRuntime(@"D:\repo\src\App.fs", "let x = 1");
        var result = DependencyRelationIngest.IngestFromContents(runtime);

        Assert.Equal(0, result.Ingested);
        Assert.Equal(1, result.SkippedNonCs);
        Assert.Empty(result.Runtime.Session.Graph.Relations);
    }
}
