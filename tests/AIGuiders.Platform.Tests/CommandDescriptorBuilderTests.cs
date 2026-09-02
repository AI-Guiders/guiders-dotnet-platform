#nullable enable

using AIGuiders.Platform.Execution.CommandPlane;
using AIGuiders.Platform.Execution.CommandPlane.Commands;
using AIGuiders.Platform.Execution.LanguageIntelligence.Bundled.Commands;
using AIGuiders.Platform.Execution.LanguageIntelligence.Edit;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class CommandDescriptorBuilderTests
{
    [Fact]
    public void Describe_builds_descriptor_with_scope_and_surfaces()
    {
        var descriptor = CommandDescriptors.Describe("dash.show")
            .Path("show dashboard")
            .Help("Open dashboard")
            .Group("Host")
            .Scope([])
            .Surfaces("dash-ccl")
            .Build();

        Assert.Equal("dash.show", descriptor.CommandId);
        Assert.Equal("show dashboard", descriptor.Path);
        Assert.Empty(descriptor.Scope);
        Assert.Equal(["dash-ccl"], descriptor.Surfaces);
    }

    [Fact]
    public void Rows_expand_one_command_to_many_paths()
    {
        var rows = CommandDescriptorRows.ForCommand(
            "dash.show.surface",
            [("show dashboard", "Dashboard"), ("show controlcenter", "Control Center")],
            defaults => defaults.Group("Host").Surfaces("dash-ccl"));

        Assert.Equal(2, rows.Count);
        Assert.All(rows, row => Assert.Equal("dash.show.surface", row.CommandId));
        Assert.Contains(rows, row => row.Path == "show dashboard");
    }

    [Fact]
    public void RegisterCatalog_registers_explicit_descriptor()
    {
        var registry = new PlatformCommandRegistry<EditorBufferContext>();
        registry.RegisterCatalog(new EditorLineSelectCommand(), builder => builder
            .Path("custom editor select")
            .Help("Custom")
            .ArgTail("none"));

        var descriptors = RegistryCatalogBuilder.CollectDescriptors(registry);
        Assert.Single(descriptors);
        Assert.Equal("custom editor select", descriptors[0].Path);
    }
}
