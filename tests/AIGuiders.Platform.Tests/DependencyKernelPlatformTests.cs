#nullable enable

using AIGuiders.Platform.Execution.Language.CSharp.Relations;
using AIGuiders.Platform.Execution.LanguageIntelligence;
using AIGuiders.Platform.Execution.LanguageIntelligence.Relations;
using AIGuiders.Platform.Modeling.Ide.Session;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class DependencyKernelPlatformTests
{
    [Fact]
    public void Roslyn_profile_registers_adapter_slot_and_seams()
    {
        AdapterSlotRegistry.ResetForTests();
        RelationSeamRegistry.ResetForTests();
        RoslynDependencyRelationEmitStub.ResetForTests();
        RoslynDependencyRelationEmitStub.RegisterDefaults();

        Assert.Single(AdapterSlotRegistry.All);
        Assert.Contains(RoslynDependencyRelationEmitStub.ProfileId, RelationSeamRegistry.RegisteredProfiles);
        Assert.Same(
            RelationSeamRegistry.ResolveFor(RoslynDependencyRelationEmitStub.ProfileId),
            RelationSeamRegistry.MaterializeFor(RoslynDependencyRelationEmitStub.ProfileId));
    }

    [Fact]
    public void RoslynDependencyRelationEmitStub_returns_empty_until_ingest()
    {
        AdapterSlotRegistry.ResetForTests();
        RelationSeamRegistry.ResetForTests();
        var runtime = default(SessionRuntime);
        Assert.Empty(RoslynDependencyRelationEmitStub.EmitStub(runtime!));
        Assert.Single(AdapterSlotRegistry.All);
    }
}
