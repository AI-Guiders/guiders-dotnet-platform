using System.Reflection;
using AIGuiders.Platform.Execution.Documentation.Correspondence;
using AIGuiders.Platform.Execution.Ide.Session;
using AIGuiders.Platform.Execution.LanguageIntelligence.Relations;
using AIGuiders.Platform.Execution.LanguageIntelligence.Relations.Conformance;
using Xunit;

namespace AIGuiders.Platform.Tests;

/// <summary>Plan §10 Phase 2 verification gate for Execution transitional types.</summary>
public sealed class FederationPhase2ChecklistTests
{
    [Fact]
    public void BracketAnchorSpan_type_is_deleted_from_Execution()
    {
        var modelingAssembly = typeof(AIGuiders.Platform.Modeling.Documentation.Correspondence.DocToCodeWitness).Assembly;
        Assert.Null(modelingAssembly.GetType("AIGuiders.Platform.Modeling.LanguageIntelligence.BracketAnchorSpan"));
        Assert.Null(typeof(CodeEditResolveAxes).Assembly.GetType("AIGuiders.Platform.Execution.LanguageIntelligence.BracketAnchorSpan"));
    }

    [Fact]
    public void CodeEdit_resolve_projection_and_legacy_wire_types_are_public()
    {
        Assert.True(typeof(CodeEditResolveAxes).IsPublic);
        Assert.True(typeof(LegacyWireSpan).IsPublic);
        Assert.True(typeof(CodeEditResolveProjection).IsPublic);
    }

    [Fact]
    public void CorrespondenceRelationIngest_exposes_registry_scan_for_open_hook()
    {
        var method = typeof(CorrespondenceRelationIngest).GetMethod(
            nameof(CorrespondenceRelationIngest.IngestFromRegistry),
            BindingFlags.Public | BindingFlags.Static);

        Assert.NotNull(method);
    }

    [Fact]
    public void Build_diagnostic_producer_and_runtime_build_hook_are_public()
    {
        Assert.True(typeof(BuildDiagnosticProducer).IsPublic);
        Assert.NotNull(typeof(FederationSessionRuntime).GetMethod(
            nameof(FederationSessionRuntime.TryRunBuildAndIngestDiagnostics),
            BindingFlags.Public | BindingFlags.Static));
    }

    [Fact]
    public void Bracket_resolve_boundary_is_public_kind_first_entry()
    {
        Assert.True(typeof(BracketResolveBoundary).IsPublic);
        Assert.True(BracketResolveBoundary.TryParseToAxes(
            "[Kind:CodeEdit; File:a.cs; Member:B]",
            out _,
            out var path));
        Assert.Equal("kind-spec", path);
        Assert.False(BracketResolveBoundary.TryParseToAxes(
            "[F:a.cs; M:B]",
            out _,
            out _));
        Assert.True(BracketResolveBoundary.TryFormatCodeEdit(
            new CodeEditResolveAxes("a.cs", "B", null, null),
            out var wire));
        Assert.Contains("Kind:CodeEdit", wire);
    }

    [Fact]
    public void Anchor_resolve_conformance_is_kind_spec_only()
    {
        var json = ConformanceFixture.LoadEmbedded(
            "AIGuiders.Platform.Tests.Fixtures.LanguageIntelligence.anchor-resolve.spec.json");
        var spec = RelationResolveSpecConformance.Load(json);
        Assert.True(spec.Version >= 2);
        Assert.All(spec.Vectors, vector =>
            Assert.Equal("kind-spec", vector.Mode, StringComparer.OrdinalIgnoreCase));
    }
}
