#nullable enable
using AIGuiders.Platform.LanguageIntelligence.Adapters.Roslyn;
using AIGuiders.Platform.LanguageIntelligence.Anchors;
using AIGuiders.Platform.Notations.Bracket;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class BracketDocSymbolTests
{
    [Fact]
    public void DocSymbol_profile_parses_family_type_member()
    {
        Assert.True(
            BracketReader.Default.TryRead(
                "[Family:doc; Package:Notations.Argument; Type:NormalizedArguments; Member:ReaderId]",
                BracketProfiles.DocSymbol,
                BracketAxisValuePlans.DocSymbol,
                out var wire,
                out var error),
            error);

        Assert.NotNull(wire);
        Assert.Equal(4, wire!.Axes.Count);
    }

    [Fact]
    public void DocSymbol_resolver_finds_platform_type_and_member()
    {
        var repoRoot = FindRepoRoot(AppContext.BaseDirectory)
            ?? throw new InvalidOperationException("repo root not found");
        var catalog = RoslynDocSymbolCatalog.BuildFromSourceRoot(Path.Combine(repoRoot, "src"));
        var resolver = new DocSymbolAnchorResolver(catalog);

        Assert.True(
            BracketReader.Default.TryRead(
                "[Family:doc; Package:Notations.Argument; Type:NormalizedArguments; Member:ReaderId]",
                BracketProfiles.DocSymbol,
                BracketAxisValuePlans.DocSymbol,
                out var wire,
                out var error),
            error);

        Assert.True(resolver.TryResolve(wire!, out var resolveError), resolveError);
    }

    static string? FindRepoRoot(string start)
    {
        var dir = new DirectoryInfo(Path.GetFullPath(start));
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "AIGuiders.Platform.sln")))
                return dir.FullName;
            dir = dir.Parent;
        }

        return null;
    }
}
