using AIGuiders.Platform.Execution.Language;
using AIGuiders.Platform.Language.CSharp;
using AIGuiders.Platform.Language.Sql;
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
            .Register(new SqlPostgresLanguageBackend())
            .Register(new SqlMssqlLanguageBackend())
            .Register(new SqlLanguageBackend())
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
    public void Resolve_sql_returns_generic_sql_backend()
    {
        var resolver = CreateResolver();
        var backend = resolver.Resolve("migrations/001_init.sql");
        Assert.NotNull(backend);
        Assert.Equal(LanguageIds.Sql, backend!.LanguageId);
    }

    [Fact]
    public void Resolve_sql_postgres_with_hint()
    {
        var resolver = CreateResolver();
        var backend = resolver.Resolve(
            "queries/report.sql",
            new ProjectHint { SessionDefaultLanguageId = LanguageIds.SqlPostgres });
        Assert.NotNull(backend);
        Assert.Equal(LanguageIds.SqlPostgres, backend!.LanguageId);
    }

    [Fact]
    public void Resolve_sql_mssql_with_hint()
    {
        var resolver = CreateResolver();
        var backend = resolver.Resolve(
            "queries/report.sql",
            new ProjectHint { SessionDefaultLanguageId = "tsql" });
        Assert.NotNull(backend);
        Assert.Equal(LanguageIds.SqlMssql, backend!.LanguageId);
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
    [InlineData("001_init.sql", LanguageIds.Sql)]
    [InlineData("dashboard.dash", LanguageIds.Dashspec)]
    [InlineData("report.dashspec", LanguageIds.Dashspec)]
    public void LanguagePathRules_resolve_expected_ids(string path, string expected)
    {
        Assert.Equal(expected, LanguagePathRules.ResolveLanguageId(path));
    }
}
