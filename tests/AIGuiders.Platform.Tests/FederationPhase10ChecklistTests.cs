#nullable enable

using System.Reflection;
using System.Text.Json;
using AIGuiders.Platform.Execution.Documentation.Correspondence;
using AIGuiders.Platform.Execution.Ide.Session;
using AIGuiders.Platform.Execution.LanguageIntelligence.Relations;
using AIGuiders.Platform.Execution.LanguageIntelligence.Relations.Conformance;
using AIGuiders.Platform.Modeling.Build;
using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;
using Xunit;

namespace AIGuiders.Platform.Tests;

/// <summary>Plan §10 TO-BE closure gate — audit presence + Execution boundary evidence (ship-51).</summary>
public sealed class FederationPhase10ChecklistTests
{
    [Fact]
    public void To_be_audit_document_exists_in_guiders_fsharp()
    {
        var audit = FindGuidersFsharpFile("docs", "federation", "model-extraction-to-be-audit.md");
        Assert.True(File.Exists(audit), audit);
        var text = File.ReadAllText(audit);
        Assert.Contains("CLOSED", text);
        Assert.Contains("| P4-01 |", text);
        Assert.Contains("verified", text);
    }

    [Fact]
    public void Adr_0042_documents_ir_language_retirement()
    {
        var adr = FindRepoFile("docs", "adr", "GUIDERS-ADR-0042-intermediate-representation-family.md");
        var text = File.ReadAllText(adr);
        Assert.Contains("IntermediateRepresentation.Language` is **retired**", text);
        Assert.Contains("Modeling.LanguageIntelligence.Relations", text);
    }

    static string FindRepoFile(params string[] parts)
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine([dir.FullName, .. parts]);
            if (File.Exists(candidate))
                return candidate;
        }

        throw new InvalidOperationException($"Could not locate {string.Join('/', parts)}.");
    }

    [Fact]
    public void IntermediateRepresentation_Language_project_is_deleted()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "src", "AIGuiders.Platform.IntermediateRepresentation.Language");
            if (Directory.Exists(candidate))
                Assert.Fail($"legacy IR.Language project still present: {candidate}");
        }
    }

    [Fact]
    public void BuildDiagnostic_model_exposes_RelationSpec_Spec_field()
    {
        var spec = typeof(BuildDiagnostic).GetProperty(nameof(BuildDiagnostic.Spec));
        Assert.NotNull(spec);
        Assert.Equal(typeof(RelationSpec), spec!.PropertyType);
    }

    [Fact]
    public void LegacyNavWireIngest_is_deleted_from_execution_boundary()
    {
        var executionAssembly = typeof(RelationWireBoundary).Assembly;
        Assert.Null(executionAssembly.GetType(
            "AIGuiders.Platform.Execution.LanguageIntelligence.Relations.LegacyNavWireIngest"));
    }

    [Fact]
    public void LegacyWireSpan_is_deleted_from_execution_boundary()
    {
        var modelingAssembly = typeof(AIGuiders.Platform.Modeling.Documentation.Correspondence.DocToCodeWitness).Assembly;
        var executionAssembly = typeof(RelationWireBoundary).Assembly;
        Assert.Null(modelingAssembly.GetType("AIGuiders.Platform.Modeling.LanguageIntelligence.LegacyWireSpan"));
        Assert.Null(executionAssembly.GetType("AIGuiders.Platform.Execution.LanguageIntelligence.Relations.LegacyWireSpan"));
        Assert.True(typeof(RelationWireBoundary).IsPublic);
    }

    [Fact]
    public void Federation_runtime_exposes_open_build_and_correspondence_hooks()
    {
        Assert.NotNull(typeof(FederationSessionRuntime).GetMethod(
            nameof(FederationSessionRuntime.Open),
            BindingFlags.Public | BindingFlags.Static));
        Assert.NotNull(typeof(FederationSessionRuntime).GetMethod(
            nameof(FederationSessionRuntime.TryRunBuildAndIngestDiagnostics),
            BindingFlags.Public | BindingFlags.Static));
        Assert.NotNull(typeof(CorrespondenceRelationIngest).GetMethod(
            nameof(CorrespondenceRelationIngest.IngestFromRegistry),
            BindingFlags.Public | BindingFlags.Static));
    }

    [Fact]
    public void Bracket_and_anchor_conformance_specs_are_kind_first_v2()
    {
        foreach (var resource in new[]
                 {
                     "AIGuiders.Platform.Tests.Fixtures.LanguageIntelligence.bracket-kind-canon.spec.json",
                     "AIGuiders.Platform.Tests.Fixtures.LanguageIntelligence.anchor-resolve.spec.json",
                     "AIGuiders.Platform.Tests.Fixtures.LanguageIntelligence.relation-spec-witness.spec.json",
                 })
        {
            var json = ConformanceFixture.LoadEmbedded(resource);
            using var doc = JsonDocument.Parse(json);
            Assert.True(doc.RootElement.TryGetProperty("version", out var version));
            Assert.True(version.GetInt32() >= 2, resource);
        }

        var anchor = RelationResolveSpecConformance.Load(
            ConformanceFixture.LoadEmbedded(
                "AIGuiders.Platform.Tests.Fixtures.LanguageIntelligence.anchor-resolve.spec.json"));
        Assert.All(anchor.Vectors, vector =>
            Assert.Equal("kind-spec", vector.Mode, StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public void CodeCenterHost_exists_for_ship_62_semantic_editor_slice()
    {
        var host = FindGuidersWpfFile("src", "AIGuiders.Surface.Wpf.CodeCenter", "CodeCenterHost.cs");
        Assert.True(File.Exists(host), host);
        var livingMatrix = FindGuidersFsharpFile("docs", "federation", "model-extraction-living-matrix.md");
        var matrix = File.ReadAllText(livingMatrix);
        Assert.Contains("ship-62", matrix);
        Assert.Contains("**shipped**", matrix);
    }

    static string FindGuidersWpfFile(params string[] parts)
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var sibling = Path.Combine([dir.FullName, "guiders-wpf", .. parts]);
            if (File.Exists(sibling))
                return sibling;
        }

        throw new InvalidOperationException($"Could not locate guiders-wpf/{string.Join('/', parts)}.");
    }

    static string FindGuidersFsharpFile(params string[] parts)
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var sibling = Path.Combine([dir.FullName, "guiders-fsharp", .. parts]);
            if (File.Exists(sibling))
                return sibling;
        }

        throw new InvalidOperationException($"Could not locate guiders-fsharp/{string.Join('/', parts)}.");
    }
}
