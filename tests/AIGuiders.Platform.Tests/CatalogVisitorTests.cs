#nullable enable
using AIGuiders.Platform.CommandPlane;
using AIGuiders.Platform.CommandPlane.Commands;
using AIGuiders.Platform.CommandPlane.Editor;
using AIGuiders.Platform.CommandPlane.Editor.Commands;
using AIGuiders.Platform.CommandPlane.Sources;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class CatalogVisitorTests
{
    [Fact]
    public void Registry_visitor_collects_catalog_described_commands()
    {
        var registry = EditorCommandRegistry.CreateBundled();
        var descriptors = RegistryCatalogBuilder.CollectDescriptors(registry);

        Assert.Contains(descriptors, d => d.CommandId == EditorLineSelectCommand.Id);
        Assert.Contains(descriptors, d => d.CommandId == "bold");
        Assert.True(descriptors.Count >= 12);
    }

    [Fact]
    public void Registry_builds_slash_catalog_index()
    {
        var registry = EditorCommandRegistry.CreateBundled();
        var index = RegistryCatalogBuilder.BuildIndex(
            registry,
            descriptor => descriptor.CommandId.StartsWith("editor.line.", StringComparison.OrdinalIgnoreCase));

        Assert.True(index.TryGet("editor line select", out var select));
        Assert.Equal(EditorLineSelectCommand.Id, select.CommandId);
        Assert.Equal(SlashArgTailKind.ImplicitLineRange, select.ArgTailKind);
    }

    [Fact]
    public void Editor_surface_catalog_uses_registry_visitor()
    {
        var descriptors = EditorSurfaceCatalog.BundledEditorLineCommands();

        Assert.Equal(2, descriptors.Count);
        Assert.Contains(descriptors, d => d.CommandId == EditorLineSelectCommand.Id);
        Assert.Contains(descriptors, d => d.CommandId == EditorLineDeleteCommand.Id);
    }

    [Fact]
    public void Registry_command_source_composes_with_json_source()
    {
        const string json = """
            {
              "commands": [
                {
                  "commandId": "help",
                  "path": "help",
                  "help": "Help",
                  "argTail": "none"
                }
              ]
            }
            """;

        var catalog = SlashCatalogComposer.Build(
            RegistryCatalogBuilder.ToCommandSource(
                EditorCommandRegistry.CreateBundled(),
                predicate: d => d.CommandId == EditorLineSelectCommand.Id),
            CommandSources.FromJson(json));

        Assert.True(catalog.TryGet("help", out _));
        Assert.True(catalog.TryGet("editor line select", out _));
    }

    [Fact]
    public void Explicit_descriptor_registration_overrides_command_projection()
    {
        var registry = new PlatformCommandRegistry<EditorBufferContext>();
        var command = new EditorLineSelectCommand();
        registry.Register(
            command,
            new SlashCommandDescriptor
            {
                Domain = "editor",
                Object = "line",
                Intent = "select",
                CommandId = EditorLineSelectCommand.Id,
                Path = "custom select path",
                Help = "Custom help",
                ArgTail = "none",
            });

        var descriptors = RegistryCatalogBuilder.CollectDescriptors(registry);
        Assert.Single(descriptors);
        Assert.Equal("custom select path", descriptors[0].Path);
    }
}
