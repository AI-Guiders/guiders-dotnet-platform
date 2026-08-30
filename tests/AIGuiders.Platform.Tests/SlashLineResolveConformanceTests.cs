#nullable enable
using System.Reflection;
using AIGuiders.Platform.CommandPlane.Conformance;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class SlashLineResolveConformanceTests
{
    [Fact]
    public void Slash_line_resolve_v1_vectors_conform()
    {
        var json = LoadText("AIGuiders.Platform.Tests.Fixtures.Slash.slash-line-resolve-v1.spec.json");
        var spec = SlashLineResolveSpecConformance.Load(json);
        Assert.Equal("slash-line-resolve", spec.Surface);
        Assert.Empty(SlashLineResolveSpecConformance.ValidateDocument(spec));
    }

    static string LoadText(string resourceName)
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Missing embedded resource: {resourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
