using System.Reflection;
using AIGuiders.Platform.Execution.Ide.Session;
using AIGuiders.Platform.Execution.LanguageIntelligence.Relations;
using AIGuiders.Platform.Modeling.Documentation.Correspondence;
using Xunit;

namespace AIGuiders.Platform.Tests;

/// <summary>Plan §10 Phase 2 verification gate for Execution transitional types.</summary>
public sealed class FederationPhase2ChecklistTests
{
    [Fact]
    public void BracketAnchorSpan_type_is_deleted_from_Execution()
    {
        var modelingAssembly = typeof(DocToCodeWitness).Assembly;
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
}
