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
    public void StepCompletion_root_lists_domains_and_elision()
    {
        var catalog = SemanticTestCatalog();
        var items = SlashStepCompletion.GetSuggestions(catalog, "");
        var segments = items.Select(i => i.StepSegment).ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("intercom", segments);
        Assert.Contains("build", segments);
        Assert.Contains("git", segments);
        Assert.DoesNotContain("run", segments);
    }

    [Fact]
    public void StepCompletion_build_space_lists_intents()
    {
        var catalog = SemanticTestCatalog();
        var items = SlashStepCompletion.GetSuggestions(catalog, "build ");
        Assert.Contains(items, i => i.StepSegment == "run");
        Assert.Contains(items, i => i.StepSegment == "ui");
    }

    [Fact]
    public void StepCompletion_flat_path_catalog()
    {
        var catalog = SlashCatalogIndex.FromDescriptors([
            new SlashCommandDescriptor
            {
                Domain = "", Object = "", Intent = "",
                CommandId = "help", Path = "help", Help = "Help",
            },
            new SlashCommandDescriptor
            {
                Domain = "", Object = "", Intent = "",
                CommandId = "file.open", Path = "file open", Help = "Open file", ArgTail = "required",
            },
        ]);

        var root = SlashStepCompletion.GetSuggestions(catalog, "");
        Assert.Contains(root, i => i.StepSegment == "help");
        Assert.Contains(root, i => i.StepSegment == "file");

        var fileStep = SlashStepCompletion.GetSuggestions(catalog, "file ");
        Assert.Contains(fileStep, i => i.StepSegment == "open");
    }

    static SlashCatalogIndex SemanticTestCatalog() =>
        SlashCatalogIndex.FromDescriptors([
            new SlashCommandDescriptor
            {
                Domain = "intercom", Object = "topic", Intent = "list",
                CommandId = "intercom.topic.list", Path = "intercom topic list", Help = "Topics",
            },
            new SlashCommandDescriptor
            {
                Domain = "intercom", Object = "server", Intent = "list",
                CommandId = "intercom.server.list", Path = "intercom server list", Help = "Servers",
            },
            new SlashCommandDescriptor
            {
                Domain = "solution", Object = "build", Intent = "run",
                CommandId = "build.run", Path = "solution build run", PathAliases = ["build run"], Help = "Build run",
            },
            new SlashCommandDescriptor
            {
                Domain = "solution", Object = "build", Intent = "ui",
                CommandId = "build.ui", Path = "solution build ui", PathAliases = ["build ui"], Help = "Build UI",
            },
            new SlashCommandDescriptor
            {
                Domain = "git", Object = "status", Intent = "",
                CommandId = "git.status", Path = "git status", Help = "Git status",
            },
        ]);

    [Fact]
    public void DataBus_IdeHealth_events_exist()
    {
        var b = new AIGuiders.Platform.Cockpit.DataBus.BuildStateChanged(true, 0, true);
        Assert.True(b.IsBuilding);
    }
}
