#nullable enable
using System.Reflection;
using AIGuiders.Platform.Notations.Conformance;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class NotationConformanceTests
{
    [Fact]
    public void Command_slash_v1_vectors_conform()
    {
        var spec = LoadSpec("AIGuiders.Platform.Tests.Fixtures.Notation.command-slash-v1.spec.json");
        Assert.Equal("command-slash", spec.Surface);
        Assert.Empty(NotationSpecConformance.ValidateDocument(spec));
    }

    [Fact]
    public void Argument_kv_v1_vectors_conform()
    {
        var spec = LoadSpec("AIGuiders.Platform.Tests.Fixtures.Notation.argument-kv-v1.spec.json");
        Assert.Equal("argument-kv", spec.Surface);
        Assert.Empty(NotationSpecConformance.ValidateDocument(spec));
    }

    [Fact]
    public void Invocation_parity_v1_vectors_conform()
    {
        var spec = LoadSpec("AIGuiders.Platform.Tests.Fixtures.Notation.invocation-parity-v1.spec.json");
        Assert.Equal("invocation-parity", spec.Surface);
        Assert.Empty(NotationSpecConformance.ValidateDocument(spec));
    }

    static NotationSpecDocument LoadSpec(string resourceName)
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Missing embedded resource: {resourceName}");
        using var reader = new StreamReader(stream);
        return NotationSpecConformance.Load(reader.ReadToEnd());
    }
}
