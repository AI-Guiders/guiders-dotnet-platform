using AIGuiders.Platform.Execution.Language;
using AIGuiders.Platform.Language.CSharp;
using AIGuiders.Platform.Modeling.Language;
using AIGuiders.Platform.Modeling.Language.Adapters.Fcs;
using AIGuiders.Platform.Modeling.Language.Adapters.Gdl;
using Xunit;

namespace AIGuiders.Platform.Execution.Language.Tests;

public class LanguageResolverCenterTests
{
    private static LanguageResolverCenter CreateResolver() =>
        new LanguageResolverBuilder()
            .WithActivation(new PathRulesLanguageActivationCatalog())
            .Register(new FcsLanguageBackend(null))
            .Register(new GdlLanguageBackend())
            .Register(new CsharpLanguageBackend())
            .Build();

    [Fact]
    public void Resolve_fs_returns_fsharp_backend()
    {
        var resolver = CreateResolver();
        var backend = resolver.Resolve("src/Module.fs");
        Assert.NotNull(backend);
        Assert.Equal(LanguageIds.Fsharp, backend!.LanguageId);
    }

    [Fact]
    public void Resolve_deck_gdl_returns_gdl_backend()
    {
        var resolver = CreateResolver();
        var backend = resolver.Resolve("authoring/dashspec-studio.deck.gdl");
        Assert.NotNull(backend);
        Assert.Equal(LanguageIds.Gdl, backend!.LanguageId);
    }

    [Fact]
    public void Resolve_cs_returns_csharp_backend()
    {
        var resolver = CreateResolver();
        var backend = resolver.Resolve("src/Program.cs");
        Assert.NotNull(backend);
        Assert.Equal(LanguageIds.Csharp, backend!.LanguageId);
    }

    [Theory]
    [InlineData("App.fsproj", LanguageIds.Fsharp)]
    [InlineData("planet.gdlproj", LanguageIds.Gdl)]
    [InlineData("App.csproj", LanguageIds.Csharp)]
    [InlineData("dashboard.dash", LanguageIds.Dashspec)]
    [InlineData("report.dashspec", LanguageIds.Dashspec)]
    public void LanguagePathRules_resolve_expected_ids(string path, string expected)
    {
        Assert.Equal(expected, LanguagePathRules.ResolveLanguageId(path));
    }
}
