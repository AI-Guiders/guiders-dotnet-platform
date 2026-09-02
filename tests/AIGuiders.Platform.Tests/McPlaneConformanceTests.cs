#nullable enable
using System.Reflection;
using AIGuiders.Platform.Execution.MCPlane.Conformance;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class McPlaneConformanceTests
{
    [Fact]
    public void Mcplane_pulse_default_vectors_conform()
    {
        var spec = LoadSpec("AIGuiders.Platform.Tests.Fixtures.MCPlane.pulse-default.spec.json");
        Assert.Equal("mcplane-pulse-default", spec.Surface);
        Assert.Empty(McPlaneSpecConformance.ValidateDocument(spec));
    }

    [Fact]
    public void Mcplane_next_hints_vectors_conform()
    {
        var spec = LoadSpec("AIGuiders.Platform.Tests.Fixtures.MCPlane.next-hints.spec.json");
        Assert.Equal("mcplane-next-hints", spec.Surface);
        Assert.Empty(McPlaneSpecConformance.ValidateDocument(spec));
    }

    static McPlaneSpecDocument LoadSpec(string resourceName)
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Missing embedded resource: {resourceName}");
        using var reader = new StreamReader(stream);
        return McPlaneSpecConformance.Load(reader.ReadToEnd());
    }
}
