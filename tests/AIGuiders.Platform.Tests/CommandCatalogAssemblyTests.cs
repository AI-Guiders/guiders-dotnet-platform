#nullable enable

using AIGuiders.Platform.CommandPlane;
using AIGuiders.Platform.CommandPlane.Commands;
using AIGuiders.Platform.IntermediateRepresentation.Command;
using AIGuiders.Platform.LanguageIntelligence.Bundled.Commands;
using AIGuiders.Platform.LanguageIntelligence.Edit;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class CommandCatalogAssemblyTests
{
    [Fact]
    public void Build_merges_registry_rows_and_expanded_paths()
    {
        var registry = new PlatformCommandRegistry<EditorBufferContext>();
        registry.Register(new EditorLineSelectCommand());

        var expanded = CommandDescriptorRows.ForCommand(
            EditorLineSelectCommand.Id,
            [("alt select path", "Alt help")],
            defaults => defaults.ArgTail(CommandArgTailPolicy.ImplicitLineRange));

        var catalog = CommandCatalogAssembly.Build(registry, expanded);

        Assert.True(catalog.TryGet("editor line select", out _));
        Assert.True(catalog.TryGet("alt select path", out var alt));
        Assert.Equal(EditorLineSelectCommand.Id, alt.CommandId);
    }

    [Fact]
    public void Build_applies_active_scope_to_expanded_rows_only()
    {
        var registry = new PlatformCommandRegistry<EditorBufferContext>();
        registry.RegisterCatalog(new EditorLineSelectCommand(), builder => builder
            .Path("scoped registry path")
            .Help("Registry")
            .Scope("editor"));

        var expanded = CommandDescriptors.Describe("other")
            .Path("scoped expanded")
            .Help("Expanded")
            .Scope("dashboard")
            .Build();

        var catalog = CommandCatalogAssembly.Build(
            registry,
            [expanded],
            activeScope: ["editor"]);

        Assert.True(catalog.TryGet("scoped registry path", out _));
        Assert.False(catalog.TryGet("scoped expanded", out _));
    }
}
