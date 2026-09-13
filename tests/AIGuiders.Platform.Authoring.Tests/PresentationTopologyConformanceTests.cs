#nullable enable

using System.Reflection;
using AIGuiders.Platform.Notations.Presentation.Topology.Conformance;
using Xunit;

namespace AIGuiders.Platform.Authoring.Tests;

public sealed class PresentationTopologyConformanceTests
{
    [Fact]
    public void Presentation_topology_vectors_parse_to_ir()
    {
        var spec = TopologySpecConformance.Load(LoadEmbedded(
            "AIGuiders.Platform.Authoring.Tests.Fixtures.Notation.presentation-topology.spec.json"));
        Assert.Equal("notation.presentation.topology", spec.Kind);
        Assert.Empty(TopologySpecConformance.ValidateDocument(spec));
    }

    static string LoadEmbedded(string resourceName)
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Missing embedded resource: {resourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
