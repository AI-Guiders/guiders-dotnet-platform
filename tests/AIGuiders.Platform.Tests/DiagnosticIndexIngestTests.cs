#nullable enable

using AIGuiders.Platform.Execution.Ide.Session;
using AIGuiders.Platform.Modeling.Core.Identity;
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;
using AIGuiders.Platform.Modeling.Paths;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class DiagnosticIndexIngestTests
{
    static SessionRuntime CreateRuntime(string filePath, string contents)
    {
        var anchor = LogicalPath.Create(Path.GetFullPath(@"D:\repo\App.slnx"));
        var graph = SolutionGraphModule.create(anchor, ListModule.Empty<ProjectNode>(), ListModule.Empty<Relation>());
        var session = SolutionSessionModule.withPhase(LifecyclePhase.Unloaded, SolutionSessionModule.create(anchor, graph));

        var owner = AIGuiders.Platform.Modeling.Ide.Session.ProjectIdModule.create(Path.GetFullPath(@"D:\repo\App.fsproj"));
        var fullPath = Path.GetFullPath(filePath);
        FSharpMap<string, ProjectId> ownership =
            MapModule.OfSeq([Tuple.Create(fullPath, owner)]);

        return SessionOrchestrator.create(
            session,
            [Tuple.Create(fullPath, contents)],
            ownership);
    }

    [Fact]
    public void Ingest_resolves_path_to_DocId_and_mints_DiagnosticRef()
    {
        const string relativePath = @"D:\repo\src\Foo.fs";
        var runtime = CreateRuntime(relativePath, "let x = 1");

        var result = DiagnosticIndexIngest.Ingest(
            [
                new DiagnosticIndexIngest.LanguageDiagnostic(
                    "CS0246",
                    "error",
                    "type not found",
                    relativePath,
                    new TextSpan(1, 1, 1, 5),
                    Language: "fsharp"),
            ],
            runtime);

        Assert.Equal(1, result.Ingested);
        Assert.Equal(0, result.SkippedUnregistered);
        Assert.Equal(1, MapModule.Count(result.Runtime.Diagnostics));

        var found = DiagnosticIndexOps.tryFindByCode("CS0246", result.Runtime.Diagnostics);
        Assert.True(FSharpOption<Tuple<Identity<Diagnostic, NumericId>, DiagnosticRecord>>.get_IsSome(found));
        Assert.Equal("type not found", found!.Value.Item2.Message);
    }

    [Fact]
    public void Ingest_skips_unregistered_paths()
    {
        var runtime = CreateRuntime(@"D:\repo\src\Foo.fs", "let x = 1");

        var result = DiagnosticIndexIngest.Ingest(
            [
                new DiagnosticIndexIngest.LanguageDiagnostic(
                    "CS0001",
                    "error",
                    "orphan",
                    @"D:\repo\src\Missing.fs",
                    new TextSpan(1, 1, 1, 1)),
            ],
            runtime);

        Assert.Equal(0, result.Ingested);
        Assert.Equal(1, result.SkippedUnregistered);
        Assert.Equal(0, MapModule.Count(result.Runtime.Diagnostics));
    }
}
