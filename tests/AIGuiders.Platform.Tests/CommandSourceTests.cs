#nullable enable
using AIGuiders.Platform.CommandPlane;
using AIGuiders.Platform.CommandPlane.Sources;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class CommandSourceTests
{
    [Fact]
    public void Composer_merges_descriptor_and_json_sources()
    {
        var bundled = CommandSource.From([
            new SlashCommandDescriptor
            {
                Domain = "", Object = "", Intent = "",
                CommandId = "help", Path = "help", Help = "Help", ArgTail = "none",
            },
        ], "bundled");

        const string json = """
            {
              "commands": [
                {
                  "commandId": "file.open",
                  "path": "file open",
                  "help": "Open file",
                  "argTail": "required",
                  "argHint": "Path relative to repo root"
                }
              ]
            }
            """;

        var catalog = SlashCatalogComposer.Build(bundled, CommandSources.FromJson(json));
        Assert.True(catalog.TryGet("help", out _));
        Assert.True(catalog.TryGet("file open", out var file));
        Assert.Equal("file.open", file.CommandId);
        Assert.Equal(SlashArgTailKind.Required, file.ArgTailKind);
    }

    [Fact]
    public void FromToml_reads_command_table_array()
    {
        const string toml = """
            [[command]]
            command_id = "select.date"
            path = "select date"
            domain = "dash"
            object = "select"
            intent = "date"
            help = "Set date filter"
            arg_tail = "picker:enum:date_preset"
            """;

        var catalog = SlashCatalogComposer.Build(CommandSources.FromToml(toml));
        Assert.True(catalog.TryGet("select date", out var route));
        Assert.Equal("select.date", route.CommandId);
        Assert.Equal(SlashArgTailKind.Picker, route.ArgTailKind);
    }

    [Fact]
    public void FromXml_reads_command_elements()
    {
        const string xml = """
            <commands>
              <command commandId="build.run" path="build run" domain="solution" object="build" intent="run" help="Build run" argTail="optional" />
            </commands>
            """;

        var catalog = SlashCatalogComposer.Build(CommandSources.FromXml(xml));
        Assert.True(catalog.TryGet("build run", out var route));
        Assert.Equal("build.run", route.CommandId);
    }

    [Fact]
    public void FromDb_wraps_delegate_loader()
    {
        var source = DatabaseCommandSources.From(
            () =>
            [
                new SlashCommandDescriptor
                {
                    Domain = "", Object = "", Intent = "",
                    CommandId = "db.echo", Path = "db echo", Help = "From DB", ArgTail = "none",
                },
            ],
            "db:test");

        var catalog = SlashCatalogComposer.Build(source);

        Assert.True(catalog.TryGet("db echo", out var route));
        Assert.Equal("db.echo", route.CommandId);
    }

    [Fact]
    public void FromAssemblyResource_reads_embedded_toml_by_suffix()
    {
        var catalog = SlashCatalogComposer.Build(
            typeof(CommandSourceTests).Assembly.FromAssemblyResource("commands.toml"));

        Assert.True(catalog.TryGet("plugin help", out var route));
        Assert.Equal("plugin.help", route.CommandId);
    }
}
