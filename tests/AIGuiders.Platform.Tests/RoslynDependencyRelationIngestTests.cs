#nullable enable

using AIGuiders.Platform.Execution.Language.CSharp.Relations;
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.Paths;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class RoslynDependencyRelationIngestTests
{
    [Fact]
    public void IngestUsesFromSource_emits_field_type_dependency()
    {
        const string source = """
            namespace Demo;

            public class Helper { }

            public class Consumer
            {
                Helper field;
            }
            """;

        var project = ProjectIdModule.create(@"D:\repo\App.csproj");
        var direct = CorrespondenceMaterialize.buildUsesFromTypeNames("src/App.cs", "Consumer", "Helper", project);
        Assert.True(RelationGraph.validateRelation(direct).IsOk);

        var relations = RoslynDependencyRelationIngest.IngestUsesFromSource("src/App.cs", source, project);

        Assert.Single(relations);
        Assert.Equal(RelationType.Uses, relations[0].Type);
    }
}
