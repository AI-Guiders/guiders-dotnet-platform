#nullable enable

using AIGuiders.Platform.Execution.Documentation.Correspondence;
using Microsoft.FSharp.Core;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class CorrespondenceModelsInteropTests
{
    [Fact]
    public void DocToCodeWitness_ToModel_roundtrips_without_CLIMutable_initializer()
    {
        var seam = new DocToCodeWitness(
            "docs/adr/0063.md",
            "ADR-0063",
            CorrespondenceProvenance.Bracket,
            CorrespondenceKind.ImplementsObligation,
            "src/Foo.cs",
            10,
            null,
            "Bar",
            "[F:src/Foo.cs; M:Bar]");

        var model = seam.ToModel();

        Assert.Equal(seam.DocPath, model.DocPath);
        Assert.True(FSharpOption<string>.get_IsSome(model.MemberKey));
        Assert.Equal(seam.MemberKey, model.MemberKey!.Value);
        Assert.Equal(seam.Wire, model.Wire);
    }

    [Fact]
    public void CorrespondenceResult_ToModel_maps_nested_docs()
    {
        var seam = new CorrespondenceResult(
            @"D:\repo",
            "docs/adr/0063.md",
            "feature: federation",
            ["docs/adr/0063.md"],
            "GUIDERS-ADR-0063",
            [new ForwardDoc("docs/adr/0063.md", "ADR-0063")],
            [],
            ["modeling"],
            "guiders.toml");

        var model = seam.ToModel();

        Assert.Equal(seam.WorkspaceRoot, model.WorkspaceRoot);
        Assert.Single(model.ForwardDocs);
        Assert.Equal("ADR-0063", model.ForwardDocs[0].Title);
    }
}
