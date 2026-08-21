#nullable enable
using AIGuiders.Platform.CommandPlane;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class CommandPlaneTests
{
    [Theory]
    [InlineData("none", SlashArgTailKind.None)]
    [InlineData("required", SlashArgTailKind.Required)]
    [InlineData("picker:repo", SlashArgTailKind.Picker)]
    public void ArgTailPolicy_parse(string raw, SlashArgTailKind expected) =>
        Assert.Equal(expected, SlashArgTailPolicy.Parse(raw));

    [Fact]
    public void CatalogIndex_longest_prefix_and_merge()
    {
        var bundled = SlashCatalogIndex.FromDescriptors([
            new SlashCommandDescriptor
            {
                Domain = "solution", Object = "build", Intent = "run",
                CommandId = "build.run", Path = "build run", Help = "Build solution",
            },
        ]);
        var overlay = SlashCatalogIndex.FromDescriptors([
            new SlashCommandDescriptor
            {
                Domain = "forge", Object = "repo", Intent = "create",
                CommandId = "forge.repo.create", Path = "forge repo create",
                PathAliases = ["/repo create"], ArgTail = "required", Help = "Create repo",
            },
        ]);
        var merged = bundled.Merge(overlay);

        Assert.True(merged.TryGet("build run", out var build));
        Assert.Equal("build.run", build.CommandId);
        Assert.True(merged.TryGet("repo create", out var forge));
        Assert.Equal("forge.repo.create", forge.CommandId);
        Assert.Equal(SlashArgTailKind.Required, forge.ArgTailKind);
    }

    [Fact]
    public void LineResolver_build_run_with_args()
    {
        var catalog = SlashCatalogIndex.FromDescriptors([
            new SlashCommandDescriptor
            {
                Domain = "solution", Object = "build", Intent = "run",
                CommandId = "build.run", Path = "build run", ArgTail = "optional",
            },
        ]);

        Assert.True(SlashLineResolver.TryResolveSlashLine("/build run Release", catalog, out var res));
        Assert.Equal("build run", res.CanonicalPath);
        Assert.Equal("Release", res.ArgTail);
        Assert.True(res.IsRunnable);
    }

    [Fact]
    public void DataBus_IdeHealth_events_exist()
    {
        var b = new AIGuiders.Platform.Cockpit.DataBus.BuildStateChanged(true, 0, true);
        Assert.True(b.IsBuilding);
    }
}
