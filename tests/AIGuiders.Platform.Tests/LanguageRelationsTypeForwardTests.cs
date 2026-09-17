#nullable enable

using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class LanguageRelationsTypeForwardTests
{
    [Fact]
    public void CSharp_Anchors_shim_assembly_forwards_resolver_type()
    {
        var anchorsAssembly = typeof(AIGuiders.Platform.Execution.Language.CSharp.Anchors.AnchorsTypeForwarders).Assembly;
        var forwarded = anchorsAssembly.GetForwardedTypes();

        Assert.Contains(
            typeof(AIGuiders.Platform.Execution.Language.CSharp.Relations.CSharpBracketAnchorResolve),
            forwarded);
    }

    [Fact]
    public void Xml_Anchors_shim_assembly_forwards_resolver_type()
    {
        var anchorsAssembly = typeof(AIGuiders.Platform.Execution.Language.Xml.Anchors.AnchorsTypeForwarders).Assembly;
        var forwarded = anchorsAssembly.GetForwardedTypes();

        Assert.Contains(
            typeof(AIGuiders.Platform.Execution.Language.Xml.Relations.XmlBracketAnchorResolve),
            forwarded);
    }
}
