#nullable enable

using AIGuiders.Platform.Execution.Language.CSharp.Relations;
using AIGuiders.Platform.Execution.LanguageIntelligence;
using AIGuiders.Platform.Modeling.Ide.Session;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class DependencyKernelPlatformTests
{
    [Fact]
    public void AdapterSlotRegistry_starts_empty_until_backend_registers()
    {
        AdapterSlotRegistry.ResetForTests();
        Assert.Empty(AdapterSlotRegistry.All);

        AdapterSlotRegistry.Register(new AdapterSlot(RoslynDependencyRelationEmitStub.ProfileId, "roslyn"));
        Assert.Single(AdapterSlotRegistry.All);
    }

    [Fact]
    public void RoslynDependencyRelationEmitStub_returns_empty_until_ingest()
    {
        var runtime = default(SessionRuntime);
        Assert.Empty(RoslynDependencyRelationEmitStub.EmitStub(runtime!));
    }
}
