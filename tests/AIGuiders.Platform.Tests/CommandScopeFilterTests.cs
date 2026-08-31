#nullable enable

using AIGuiders.Platform.CommandPlane;
using AIGuiders.Platform.CommandPlane.Catalog.Sources;
using AIGuiders.Platform.IntermediateRepresentation.Command;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class CommandScopeFilterTests
{
    [Fact]
    public void Empty_descriptor_scope_matches_any_active_scope()
    {
        Assert.True(CommandScopeFilter.Matches([], ["dashboard"]));
        Assert.True(CommandScopeFilter.Matches([], []));
    }

    [Fact]
    public void Tagged_descriptor_requires_intersection()
    {
        var descriptor = new CommandDescriptor
        {
            Domain = "",
            Object = "",
            Intent = "",
            CommandId = "dash.select",
            Path = "select",
            Scope = ["dashboard"],
        };

        Assert.True(CommandScopeFilter.Matches(descriptor, ["dashboard"]));
        Assert.True(CommandScopeFilter.Matches(descriptor, ["dashboard", "controlcenter"]));
        Assert.False(CommandScopeFilter.Matches(descriptor, ["controlcenter"]));
        Assert.False(CommandScopeFilter.Matches(descriptor, []));
    }

    [Fact]
    public void WhereScope_filters_enumerable()
    {
        var universal = Descriptor("show", []);
        var dashboard = Descriptor("select", ["dashboard"]);
        var filtered = CommandScopeFilter
            .WhereScope([universal, dashboard], ["controlcenter"])
            .Select(descriptor => descriptor.CommandId)
            .ToList();

        Assert.Equal(["show"], filtered);
    }

    [Fact]
    public void FromToml_reads_scope_list()
    {
        const string toml = """
            [[command]]
            command_id = "cc.settings"
            path = "settings"
            scope = ["controlcenter"]
            """;

        var source = CommandSources.FromToml(toml);
        var descriptor = source.Load().Single();

        Assert.Equal(["controlcenter"], descriptor.Scope);
    }

    static CommandDescriptor Descriptor(string commandId, IReadOnlyList<string> scope) =>
        new()
        {
            Domain = "",
            Object = "",
            Intent = "",
            CommandId = commandId,
            Path = commandId,
            Scope = scope,
        };
}
