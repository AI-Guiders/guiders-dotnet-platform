#nullable enable
using System.Reflection;
using AIGuiders.Platform.Execution.CommandPlane.Conformance;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class SlashConformanceTests
{
    [Fact]
    public void Slash_arg_completion_vectors_conform()
    {
        var spec = LoadSpec("AIGuiders.Platform.Tests.Fixtures.Slash.slash-arg-completion.spec.json");
        Assert.Equal("slash-arg-completion", spec.Surface);
        Assert.Empty(SlashSpecConformance.ValidateDocument(spec));
    }

    static SlashSpecDocument LoadSpec(string resourceName)
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Missing embedded resource: {resourceName}");
        using var reader = new StreamReader(stream);
        return SlashSpecLoader.Load(reader.ReadToEnd());
    }
}
