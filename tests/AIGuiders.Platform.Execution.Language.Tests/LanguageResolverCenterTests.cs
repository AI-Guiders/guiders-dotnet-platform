using AIGuiders.Platform.Execution.Language;
using AIGuiders.Platform.Modeling.Language.Adapters.Fcs;
using AIGuiders.Platform.Modeling.Language.Adapters.Gdl;
using Xunit;

namespace AIGuiders.Platform.Execution.Language.Tests;

public class LanguageResolverCenterTests
{
    private static LanguageResolverCenter CreateResolver() =>
        new LanguageResolverBuilder()
            .Register(new FcsLanguageBackend())
            .Register(new GdlLanguageBackend())
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

    [Theory]
    [InlineData("App.fsproj", LanguageIds.Fsharp)]
    [InlineData("planet.gdlproj", LanguageIds.Gdl)]
    public void LanguagePathRules_resolve_expected_ids(string path, string expected)
    {
        Assert.Equal(expected, LanguagePathRules.ResolveLanguageId(path));
    }
}
