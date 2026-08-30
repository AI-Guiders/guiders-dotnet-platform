#nullable enable
using System.Reflection;
using AIGuiders.Platform.MCPlane.Conformance;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class McPlaneConformanceTests
{
    [Fact]
    public void Mcplane_pulse_default_v1_vectors_conform()
    {
        var spec = LoadSpec("AIGuiders.Platform.Tests.Fixtures.MCPlane.pulse-default-v1.spec.json");
        Assert.Equal("mcplane-pulse-default", spec.Surface);
        Assert.Empty(McPlaneSpecConformance.ValidateDocument(spec));
    }

    [Fact]
    public void Mcplane_next_hints_v1_vectors_conform()
    {
        var spec = LoadSpec("AIGuiders.Platform.Tests.Fixtures.MCPlane.next-hints-v1.spec.json");
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
