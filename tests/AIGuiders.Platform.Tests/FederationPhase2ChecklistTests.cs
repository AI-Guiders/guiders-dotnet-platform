using System.Reflection;
using AIGuiders.Platform.Execution.Ide.Session;
using AIGuiders.Platform.Execution.LanguageIntelligence;
using AIGuiders.Platform.Modeling.Documentation.Correspondence;
using Xunit;

namespace AIGuiders.Platform.Tests;

/// <summary>Plan §10 Phase 2 verification gate for Execution transitional types.</summary>
public sealed class FederationPhase2ChecklistTests
{
    [Fact]
    public void BracketAnchorSpan_is_obsolete_Execution_only_legacy_wire_ir()
    {
        var modelingAssembly = typeof(DocToCodeWitness).Assembly;
        Assert.Null(modelingAssembly.GetType("AIGuiders.Platform.Modeling.LanguageIntelligence.BracketAnchorSpan"));

#pragma warning disable CS0618 // intentional gate on legacy wire IR type
        Assert.True(typeof(BracketAnchorSpan).IsPublic);
        Assert.NotNull(typeof(BracketAnchorSpan).GetCustomAttribute<ObsoleteAttribute>());
#pragma warning restore CS0618
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
