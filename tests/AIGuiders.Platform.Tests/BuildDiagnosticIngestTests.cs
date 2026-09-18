#nullable enable

using AIGuiders.Platform.Execution.Ide.Session;
using AIGuiders.Platform.Modeling.Build;
using AIGuiders.Platform.Modeling.Core.Identity;
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;
using AIGuiders.Platform.Modeling.Paths;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class BuildDiagnosticIngestTests
{
    static SessionRuntime CreateRuntime(string filePath, string contents)
    {
        var anchor = LogicalPath.Create(Path.GetFullPath(@"D:\repo\App.slnx"));
        var graph = SolutionGraphModule.create(anchor, ListModule.Empty<ProjectNode>(), ListModule.Empty<Relation>());
        var session = SolutionSessionModule.withPhase(LifecyclePhase.Unloaded, SolutionSessionModule.create(anchor, graph));

        var owner = AIGuiders.Platform.Modeling.Ide.Session.ProjectIdModule.create(Path.GetFullPath(@"D:\repo\App.fsproj"));
        var fullPath = Path.GetFullPath(filePath);
        FSharpMap<string, ProjectId> ownership = MapModule.OfSeq([Tuple.Create(fullPath, owner)]);

        return SessionOrchestrator.create(
            session,
            [Tuple.Create(fullPath, contents)],
            ownership);
    }

    [Fact]
    public void Ingest_mints_DiagnosticRef_in_session_index_not_array_index_stub()
    {
        const string relativePath = @"D:\repo\src\Foo.fs";
        var runtime = CreateRuntime(relativePath, "let x = 1");

        var result = BuildDiagnosticIngest.Ingest(
            [
                new BuildDiagnosticIngest.BuildDiagnosticWire(
                    relativePath,
                    12,
                    3,
                    "CS0246",
                    "type not found"),
            ],
            runtime);

        Assert.Equal(0, result.SkippedUnregistered);
        Assert.Single(result.Diagnostics);
        Assert.Single(result.Runtime.Diagnostics);

        var shaped = result.Diagnostics[0];
        Assert.Equal("CS0246", shaped.Code);

        Assert.True(
            shaped.Spec.IsDiag,
            "BuildDiagnostic.Spec must be RelationSpec.Diag from DiagnosticIndex ingest");

        var found = DiagnosticIndexOps.tryFindByCode("CS0246", result.Runtime.Diagnostics);
        Assert.True(FSharpOption<Tuple<Identity<Diagnostic, NumericId>, DiagnosticRecord>>.get_IsSome(found));
        Assert.Equal("type not found", found!.Value.Item2.Message);
    }
}
